using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    /* ============================================================================
     *  Model: Product
     *  Mục đích:
     *      - Đại diện cho một sản phẩm trong hệ thống CoffeeSeeu
     *      - Lưu các dữ liệu cơ bản: tên, mô tả, giá, ảnh và xếp hạng
     *
     *  Ghi chú:
     *      - Rating (số sao) được cập nhật thông qua chức năng đánh giá
     *      - Dữ liệu này được sử dụng trong trang sản phẩm + quản trị Admin
     * ============================================================================ */

    public class Product
    {
        // Khóa chính của sản phẩm
        public int Id { get; set; }

        // Tên sản phẩm (bắt buộc nhập)
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; } = "";

        // Mô tả sản phẩm (không bắt buộc)
        public string Description { get; set; } = "";

        // Giá sản phẩm (bắt buộc)
        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        public decimal Price { get; set; }

        // Ảnh hiển thị sản phẩm (nếu không có thì dùng ảnh mặc định)
        public string ImageUrl { get; set; } = "/img/default.png";

        // Số sao đánh giá (0–5)
        // Giá trị này được cập nhật từ tính năng RateProduct
        public int Rating { get; set; } = 0;
    }
}
