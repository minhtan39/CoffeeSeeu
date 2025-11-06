using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeSeeu.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // FK tới Order
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Thông tin sản phẩm snapshot tại thời điểm đặt hàng
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
