using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Tạo mật khẩu hash bằng PasswordHasher
            var hasher = new PasswordHasher<User>();
            var admin = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@coffeeseeu.com",
                Role = "Admin"
            };


            //  Kiểm tra null tránh cảnh báo
            string password = "123456";
                
            //  Hash mật khẩu
            admin.Password = hasher.HashPassword(admin, password);


            modelBuilder.Entity<User>().HasData(admin);
        }
    }
}
