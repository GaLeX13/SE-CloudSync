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
        Task UpdateFileAsync(FileItem file);
        Task<List<Folder>> GetFoldersByUserAsync(string userId);
        Task<Folder> CreateFolderAsync(string userId, string folderName);
        Task<Folder?> GetFolderByIdAsync(int id);
        Task DeleteFolderAsync(Folder folder);
        Task<IEnumerable<FileItem>> GetFilesByFolderAsync(string userId, int folderId);
        Task UpdateFilesAsync(IEnumerable<FileItem> files);
        Task<bool> DeleteFilesAsync(List<string> fileNames);


    }
}
