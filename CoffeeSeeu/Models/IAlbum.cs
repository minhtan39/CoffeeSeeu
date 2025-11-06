
namespace CoffeeSeeu.Models
{
    public interface IAlbum
    {
        string? Description { get; set; }
        int Id { get; set; }
        List<Image>? Images { get; set; }
        string? Name { get; set; }
    }
}