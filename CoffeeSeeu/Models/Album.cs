using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }  // Ví dụ: "Không gian quán", "Menu", "Nhân viên"

        public string? Description { get; set; }

        // Liên kết 1-nhiều với ảnh
        public List<Image>? Images { get; set; }
    }
}
