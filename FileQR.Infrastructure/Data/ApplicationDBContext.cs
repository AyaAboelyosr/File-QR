using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FileQR.Domain.Entities;


namespace FileQR.Infrastructure.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext()
        {
        }
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }


        public DbSet<Domain.Entities.File> Files { get; set; }
        public DbSet<QRSetting> QRSettings { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-V68GU8B\\MSSQLSERVER02;Database= FileQR-DB;Trusted_Connection=True;TrustServerCertificate=True"); // Ensure a default configuration
            }
        }
    }
}
