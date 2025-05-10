using System.ComponentModel.DataAnnotations;

namespace TestAppAlex.Models
{
    public class Folder
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string UserId { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<FileItem> Files { get; set; } = new();

    }
}
