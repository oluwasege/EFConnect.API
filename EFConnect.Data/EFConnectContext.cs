using EFConnect.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Data
{
    public class EFConnectContext:DbContext
    {
        public EFConnectContext(DbContextOptions<EFConnectContext> options) : base(options)
        { 
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
    }
}
