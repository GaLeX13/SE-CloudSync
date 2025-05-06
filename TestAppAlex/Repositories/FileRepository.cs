using Microsoft.EntityFrameworkCore;
using TestAppAlex.Data;
using TestAppAlex.Models;

namespace TestAppAlex.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(FileItem file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<FileItem>> GetAllAsync()
        {
            return await _context.Files.OrderByDescending(f => f.UploadedAt).ToListAsync();
        }
    }
}
