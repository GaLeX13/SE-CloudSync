using Microsoft.EntityFrameworkCore;
using TestAppAlex.Models;

namespace TestAppAlex.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FileItem> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Legătură între FileItem și Folder
            modelBuilder.Entity<FileItem>()
                .HasOne(f => f.Folder)
                .WithMany(folder => folder.Files)
                .HasForeignKey(f => f.FolderId)
                .OnDelete(DeleteBehavior.SetNull); // Dacă ștergi folderul, fișierele rămân
        }
    }
}
