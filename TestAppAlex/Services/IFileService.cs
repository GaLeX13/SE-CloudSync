using TestAppAlex.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestAppAlex.Services
{
    public interface IFileService
    {
        Task AddFileAsync(FileItem file);
        Task<IEnumerable<FileItem>> GetAllFilesAsync();
        Task<FileItem?> FindByNameAsync(string fileName);
        Task DeleteFileAsync(FileItem file);
        Task<bool> DeleteFileAsync(string fileName);
        Task<IEnumerable<FileItem>> GetFilesByUserAsync(string userId);
        Task<long> GetUsedStorageByUserAsync(string userId);
    }
}
