using EFConnect.Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Data
{
    public class Seed
    {
        private readonly EFConnectContext _context;
        public Seed(EFConnectContext context)
        {
            _context = context;
        }

        public void SeedUsers()
        {

            if (_context.Users.Any())                   // check if users are in the databaes
            {
                _context.Users.RemoveRange(_context.Users);         // if so, remove them
                _context.SaveChanges();
            }

            var userData = File.ReadAllText("../EFConnect.Data/UserSeedData.json");    // read json
            var users = JsonConvert.DeserializeObject<List<User>>(userData);        // deserialize to a list of Users

            foreach (var user in users)
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("password", out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Username = user.Username.ToLower();
                user.LastActive = user.Created;

                _context.Users.Add(user);
            }

            _context.SaveChanges();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
