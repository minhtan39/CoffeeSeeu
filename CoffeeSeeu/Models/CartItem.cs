namespace CoffeeSeeu.Models
{
    /* ============================================================================
     *  Mục đích:
     *      - Đại diện cho 1 sản phẩm nằm trong giỏ hàng (session-based cart)
     *      - Không liên kết trực tiếp với database, chỉ dùng để lưu tạm trong Session
     * ============================================================================ */

    public class CartItem
    {
        // Id của sản phẩm (dùng để truy vấn trong DB)
        public int ProductId { get; set; }

        // Tên sản phẩm
        public string? Name { get; set; }

        // Giá sản phẩm tại thời điểm thêm vào giỏ
        public decimal Price { get; set; }

        // Số lượng sản phẩm trong giỏ (mặc định = 1)
        public int Quantity { get; set; } = 1;

        // Đường dẫn ảnh sản phẩm để hiển thị trong giỏ hàng
        public string? ImageUrl { get; set; }
    }
}
