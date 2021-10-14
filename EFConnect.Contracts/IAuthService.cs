using EFConnect.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Contracts
{
    public interface IAuthService
    {
        Task<User> Register(User user, string password); 
        Task<User> Login(string username, string password); 
        Task<bool> UserExists(string username);
    }
}
