using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    /* ============================================================================
     *  Model: Album
     *  Mục đích: Đại diện cho một bộ sưu tập (album) chứa nhiều hình ảnh
     *  Ứng dụng: Hiển thị hình ảnh không gian quán, đội ngũ nhân viên, menu...
     * ============================================================================ */

    public class Album
    {
        // Khóa chính của album
        public int Id { get; set; }

        // Tên album (bắt buộc nhập, tối đa 100 ký tự)
        [Required(ErrorMessage = "Tên album không được để trống")]
        [StringLength(100, ErrorMessage = "Tên album không được vượt quá 100 ký tự")]
        public string? Name { get; set; }
        // Ví dụ: "Không gian quán", "Đội ngũ", "Menu"

        // Mô tả ngắn cho album (không bắt buộc)
        public string? Description { get; set; }

        // Quan hệ 1 - nhiều:
        // Một album chứa nhiều ảnh (List<Image>)
        public List<Image>? Images { get; set; }
    }
}
