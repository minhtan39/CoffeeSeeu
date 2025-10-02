using System.ComponentModel.DataAnnotations;

namespace CoffeeSeeu.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        [Required]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = "/img/default.png";
    }
}
