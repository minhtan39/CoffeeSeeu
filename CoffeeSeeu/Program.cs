using CoffeeSeeu.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Cho phép truy cập HttpContext
        builder.Services.AddHttpContextAccessor();

        // 1️⃣ Kết nối Database
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // 2️⃣ Cấu hình Cookie Authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
                options.Cookie.Name = ".CoffeeSeeu.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });

        // 3️⃣ Cấu hình Session
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".CoffeeSeeu.Session";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        });

        // 4️⃣ Thêm Authorization và MVC
        builder.Services.AddAuthorization();
        builder.Services.AddControllersWithViews();

        //  Chỉ build 1 lần ở đây
        var app = builder.Build();

        // 5️⃣ Middleware
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Thứ tự quan trọng
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        // 6️⃣ Routing
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // 7️⃣ Tạo admin mặc định
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                var hasher = new PasswordHasher<CoffeeSeeu.Models.User>();
                var admin = new CoffeeSeeu.Models.User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Role = "Admin"
                };

                // Hash mật khẩu trước khi lưu
                admin.Password = hasher.HashPassword(admin, "123");

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }

        // ✅ Chạy app
        app.Run();
    }
}
