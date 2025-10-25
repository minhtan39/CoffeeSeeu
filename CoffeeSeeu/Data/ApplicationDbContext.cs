using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSeeu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Bảng người dùng
        public DbSet<User> Users { get; set; }

        // Bảng sản phẩm
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình độ chính xác cho cột Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Seed tài khoản admin
            var hasher = new PasswordHasher<User>();
            var admin = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@coffeeseeu.com",
                Role = "Admin"
            };

            var password = "123456";
            admin.Password = hasher.HashPassword(admin, password);

            modelBuilder.Entity<User>().HasData(admin);


            // Seed sẵn 4 sản phẩm mẫu 
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Cà phê hạt Arabica", Price = 120000, Description = "Cà phê nguyên chất, vị chua nhẹ", ImageUrl = "/img/products/blubery-Matcha_Late.jpg", Rating = 5 },
                new Product { Id = 2, Name = "Cà phê hạt Robusta", Price = 95000, Description = "Đậm vị, thơm lâu", ImageUrl = "/img/products/cookies.jpg", Rating = 4 },
                new Product { Id = 3, Name = "Cà phê sữa đá", Price = 45000, Description = "Thức uống truyền thống Việt Nam", ImageUrl = "/img/products/hot-cacao.jpg", Rating = 5 },
                new Product { Id = 4, Name = "Cà phê pha phin", Price = 55000, Description = "Cà phê rang xay sẵn, tiện pha", ImageUrl = "/img/products/mango-machiato.jpg", Rating = 3 }
            );
        }
    }
}
