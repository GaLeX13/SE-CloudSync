using System;
using System.ComponentModel.DataAnnotations;

namespace TestAppAlex.Models
{
    public class FileItem
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public string UserId { get; set; } = string.Empty;

    }
}
