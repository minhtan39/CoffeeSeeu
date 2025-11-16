using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CoffeeSeeu.Models
{
    /* ============================================================================
     *  Model: Order
     *  Mục đích:
     *      - Đại diện cho 1 đơn hàng mà khách đặt
     *      - Lưu thông tin người mua + thời gian + tổng tiền + danh sách sản phẩm
     *
     *  Ghi chú:
     *      - Order là bảng cha, còn OrderItem là bảng con (1 đơn có nhiều mặt hàng)
     *      - Model này không liên kết trực tiếp với User qua FK, 
     *        mà chỉ lưu Username (tùy theo thiết kế)
     * ============================================================================ */

    public class Order
    {
        // Khóa chính của đơn hàng
        public int Id { get; set; }

        // Tên tài khoản đã đặt hàng (nếu người dùng đăng nhập)
        // Có thể chuyển sang UserId nếu muốn quan hệ chặt hơn
        public string? Username { get; set; }

        // Tên khách hàng (bắt buộc)
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string CustomerName { get; set; } = "";

        // Địa chỉ giao hàng
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string Address { get; set; } = "";

        // Số điện thoại khách hàng
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        // Thời gian tạo đơn hàng (mặc định theo giờ UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Tổng giá trị đơn hàng
        public decimal TotalPrice { get; set; }

        // Danh sách các sản phẩm trong đơn hàng (quan hệ 1 - nhiều)
        public List<OrderItem> Items { get; set; } = new();
    }
}
