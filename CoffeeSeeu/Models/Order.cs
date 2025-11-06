using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CoffeeSeeu.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Nếu user đăng nhập, bạn có thể lưu Username hoặc UserId (nếu có FK)
        public string? Username { get; set; }

        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Tổng tiền của đơn (đơn vị: cùng với Product.Price)
        public decimal TotalPrice { get; set; }

        // Navigation: danh sách mặt hàng trong đơn
        public List<OrderItem> Items { get; set; } = new();
    }
}
