using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Models;

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

        // Bảng album và ảnh
        public DbSet<Album> Albums { get; set; }
        public DbSet<Image> Images { get; set; }

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
            admin.Password = hasher.HashPassword(admin, "123456");

            Microsoft.EntityFrameworkCore.Metadata.Builders.DataBuilder<User> dataBuilder = modelBuilder.Entity<User>().HasData(admin);

            // Seed sản phẩm mẫu
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Cà phê hạt Arabica", Price = 120000, Description = "Cà phê nguyên chất, vị chua nhẹ", ImageUrl = "/img/products/blubery-Matcha_Late.jpg", Rating = 5 },
                new Product { Id = 2, Name = "Cà phê hạt Robusta", Price = 95000, Description = "Đậm vị, thơm lâu", ImageUrl = "/img/products/cookies.jpg", Rating = 4 },
                new Product { Id = 3, Name = "Cà phê sữa đá", Price = 45000, Description = "Thức uống truyền thống Việt Nam", ImageUrl = "/img/products/hot-cacao.jpg", Rating = 5 },
                new Product { Id = 4, Name = "Cà phê pha phin", Price = 55000, Description = "Cà phê rang xay sẵn, tiện pha", ImageUrl = "/img/products/mango-machiato.jpg", Rating = 3 }
            );

            // Seed Album mẫu
            modelBuilder.Entity<Album>().HasData(
                new Album { Id = 1, Name = "Không gian quán", Description = "Không gian ấm cúng, gần gũi" },
                new Album { Id = 2, Name = "Đội ngũ", Description = "Những người pha chế tài năng" }
            );

            // Seed hình ảnh mẫu
            modelBuilder.Entity<Image>().HasData(
                new Image { Id = 1, AlbumId = 1, ImagePath = "/img/shop-1.jpg", Description = "Góc chill tầng 1" },
                new Image { Id = 2, AlbumId = 1, ImagePath = "/img/shop-2.jpg", Description = "Không gian sân thượng" },
                new Image { Id = 3, AlbumId = 1, ImagePath = "/img/shop-3.jpg", Description = "Không gian đọc sách" },
                new Image { Id = 4, AlbumId = 2, ImagePath = "/img/staff-1.jpg", Description = "Minh Anh" },
                new Image { Id = 5, AlbumId = 2, ImagePath = "/img/staff-2.jpg", Description = "Tuấn Kiệt" },
                new Image { Id = 6, AlbumId = 2, ImagePath = "/img/staff-3.jpg", Description = "Hồng Nhung" }
            );
        }
    }
}
