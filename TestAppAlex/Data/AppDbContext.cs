using Microsoft.EntityFrameworkCore;
using TestAppAlex.Models;

namespace TestAppAlex.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FileItem> Files { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
