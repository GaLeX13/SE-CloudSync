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

        public async Task<List<Folder>> GetFoldersByUserAsync(string userId)
        {
            return await _context.Folders
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Folder> CreateFolderAsync(string userId, string folderName)
        {
            var exists = await _context.Folders.AnyAsync(f => f.UserId == userId && f.Name == folderName);
            if (exists) throw new Exception("Folder already exists.");

            var folder = new Folder
            {
                UserId = userId,
                Name = folderName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
            return folder;
        }
        
        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            return await _context.Folders.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FileItem>> GetFilesByFolderAsync(string userId, int folderId)
        {
            return await _context.Files
                .Where(f => f.UserId == userId && f.FolderId == folderId) // ✅ IMPORTANT
                .ToListAsync();
        }


        public async Task UpdateFilesAsync(IEnumerable<FileItem> files)
        {
            _context.Files.UpdateRange(files);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteFolderAsync(Folder folder)
        {
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteFilesAsync(List<string> fileNames)
        {
            if (fileNames == null || fileNames.Count == 0) return false;

            var files = await _context.Files
                .Where(f => fileNames.Contains(f.FileName))
                .ToListAsync();

            if (!files.Any()) return false;

            _context.Files.RemoveRange(files);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
