using System.ComponentModel.DataAnnotations;

namespace TestAppAlex.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Plan { get; set; } = "Basic";
        public int MaxStorageMB { get; set; } = 5000; // ex: 5GB pentru Basic

        [Required]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";
    }
}
