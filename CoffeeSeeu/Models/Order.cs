using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";
    }
}
