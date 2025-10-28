using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Models
{
    public class Image
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; } // Đường dẫn ảnh (~/images/abc.jpg)

        public string? Description { get; set; }

        // Khóa ngoại liên kết Album
        [ForeignKey("Album")]
        public int AlbumId { get; set; }

        public Album? Album { get; set; }
    }
}
