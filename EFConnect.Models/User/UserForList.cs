using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Models.User
{
    public class UserForList
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Specialty { get; set; }

        public int Age { get; set; }

        public string KnownAs { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastActive { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PhotoUrl { get; set; }
    }
}
