using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestAppAlex.Data;
using TestAppAlex.Models;
using Microsoft.EntityFrameworkCore;

namespace TestAppAlex.Services
{
    public class FileService : IFileService
    {
        private readonly AppDbContext _context;

        public FileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddFileAsync(FileItem file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<FileItem>> GetAllFilesAsync()
        {
            return await _context.Files.ToListAsync();
        }

        public async Task<FileItem?> FindByNameAsync(string fileName)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.FileName == fileName);
        }

        public async Task DeleteFileAsync(FileItem file)
        {
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FileName == fileName);
            if (file == null) return false;

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FileItem>> GetFilesByUserAsync(string userId)
        {
            return await _context.Files
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<long> GetUsedStorageByUserAsync(string userId)
        {
            return await _context.Files
                .Where(f => f.UserId == userId)
                .SumAsync(f => (long?)f.FileSize) ?? 0;
        }
        
        public async Task UpdateFileAsync(FileItem file)
        {
            _context.Files.Update(file);
            await _context.SaveChangesAsync();
        }

    }
}
