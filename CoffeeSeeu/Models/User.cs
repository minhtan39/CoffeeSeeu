using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    public class User
    {
        // Khóa chính định danh người dùng
        [Key]
        public int Id { get; set; }

        // Tên đăng nhập
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string? Username { get; set; }

        // Mật khẩu (đã mã hóa)
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string? Password { get; set; }

        // Địa chỉ email
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        // Họ và tên người dùng
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        // Số điện thoại
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        // Quyền hạn của người dùng (Admin / User)
        [Required]
        public string? Role { get; set; } = "User";

        // Ảnh đại diện (mặc định)
        [Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; } = "/img/default-avatar.png";
    }
}
