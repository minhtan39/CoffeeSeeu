using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSeeu.Data
{
    // DbContext đại diện cho CSDL của ứng dụng CoffeeSeeu
    public class ApplicationDbContext : DbContext
    {
        // Constructor nhận tùy chọn cấu hình DbContext từ DI
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ======================= CÁC BẢNG TRONG DATABASE =========================

        // Bảng người dùng (tài khoản)
        public DbSet<User> Users { get; set; }

        // Bảng sản phẩm
        public DbSet<Product> Products { get; set; }

        // Bảng album (dùng để nhóm ảnh lại, ví dụ: Không gian, Đội ngũ)
        public DbSet<Album> Albums { get; set; }

        // Bảng ảnh thuộc album
        public DbSet<Image> Images { get; set; }

        // Bảng đơn hàng
        public DbSet<Order> Orders { get; set; }

        // Bảng chi tiết đơn hàng
        public DbSet<OrderItem> OrderItems { get; set; }

        // ======================= CẤU HÌNH MÔ HÌNH ================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Cấu hình số chữ số thập phân cho các trường tiền tệ ---

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); // Giá sản phẩm: tối đa 18 số, 2 số sau dấu phẩy

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2); // Giá mỗi sản phẩm trong đơn hàng

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2); // Tổng giá trị đơn hàng

            // ======================= DỮ LIỆU MẪU (SEED DATA) ======================

            // --- Tạo tài khoản admin mặc định ---
            var hasher = new PasswordHasher<User>();
            var admin = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@coffeeseeu.com",
                Role = "Admin",
                AvatarUrl = "/img/default-avatar.png"
            };
            admin.Password = hasher.HashPassword(admin, "123456");
            modelBuilder.Entity<User>().HasData(admin);

            // --- Seed 4 sản phẩm mẫu ---
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Cà phê hạt Arabica",
                    Price = 120000m,
                    Description = "Cà phê nguyên chất, vị chua nhẹ",
                    ImageUrl = "/img/products/blubery-Matcha_Late.jpg",
                    Rating = 5
                },
                new Product
                {
                    Id = 2,
                    Name = "Cà phê hạt Robusta",
                    Price = 95000m,
                    Description = "Đậm vị, thơm lâu",
                    ImageUrl = "/img/products/cookies.jpg",
                    Rating = 4
                },
                new Product
                {
                    Id = 3,
                    Name = "Cà phê sữa đá",
                    Price = 45000m,
                    Description = "Thức uống truyền thống Việt Nam",
                    ImageUrl = "/img/products/hot-cacao.jpg",
                    Rating = 5
                },
                new Product
                {
                    Id = 4,
                    Name = "Cà phê pha phin",
                    Price = 55000m,
                    Description = "Cà phê rang xay sẵn, tiện pha",
                    ImageUrl = "/img/products/mango-machiato.jpg",
                    Rating = 3
                }
            );

            // --- Seed dữ liệu album ---
            modelBuilder.Entity<Album>().HasData(
                new Album { Id = 1, Name = "Không gian quán", Description = "Không gian ấm cúng, gần gũi" },
                new Album { Id = 2, Name = "Đội ngũ", Description = "Những người pha chế tài năng" }
            );

            // --- Seed dữ liệu ảnh trong album ---
            modelBuilder.Entity<Image>().HasData(
                new Image { Id = 1, AlbumId = 1, ImagePath = "/img/shop-1.jpg", Description = "Góc chill tầng 1" },
                new Image { Id = 2, AlbumId = 1, ImagePath = "/img/shop-2.jpg", Description = "Không gian sân thượng" },
                new Image { Id = 3, AlbumId = 1, ImagePath = "/img/shop-3.jpg", Description = "Không gian đọc sách" },
                new Image { Id = 4, AlbumId = 2, ImagePath = "/img/staff-1.jpg", Description = "Minh Tân" },
                new Image { Id = 5, AlbumId = 2, ImagePath = "/img/staff-2.jpg", Description = "Hà Nguyễn Thị" },
                new Image { Id = 6, AlbumId = 2, ImagePath = "/img/staff-3.jpg", Description = "Hồng Nhung" }
            );
        }
    }
}
