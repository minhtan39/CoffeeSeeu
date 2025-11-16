using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeSeeu.Models
{
    /* ============================================================================
     *  Model: OrderItem
     *  Mục đích:
     *      - Đại diện cho 1 dòng sản phẩm trong đơn hàng
     *      - Mỗi Order có nhiều OrderItem (quan hệ 1 - nhiều)
     *
     *  Đặc biệt:
     *      - OrderItem lưu "snapshot" thông tin sản phẩm tại thời điểm mua
     *        (tên + giá), để nếu sau này sản phẩm đổi giá thì hóa đơn
     *        vẫn giữ nguyên.
     * ============================================================================ */

    public class OrderItem
    {
        // Khóa chính
        public int Id { get; set; }

        // ==================== THÔNG TIN ĐƠN HÀNG ====================

        // Khóa ngoại liên kết đến bảng Order
        public int OrderId { get; set; }

        // Navigation property: tham chiếu đến Order chứa dòng này
        public Order? Order { get; set; }

        // ==================== THÔNG TIN SẢN PHẨM ====================

        // Id sản phẩm (sử dụng để truy ngược sản phẩm gốc nếu cần)
        public int ProductId { get; set; }

        // Tên sản phẩm tại thời điểm đặt hàng
        public string ProductName { get; set; } = "";

        // Giá sản phẩm tại thời điểm đặt hàng (decimal 18,2)
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Số lượng mua
        public int Quantity { get; set; }
    }
}
