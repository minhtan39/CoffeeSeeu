using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
        public string Password { get; set; } = "";

        // Mặc định là khách hàng (Customer). 
        // Admin thì sẽ set riêng trong DB hoặc code.
        public string Role { get; set; } = "Customer";
    }
}
