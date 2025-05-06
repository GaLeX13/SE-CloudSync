using TestAppAlex.Models;

namespace TestAppAlex.Repositories
{
    public interface IFileRepository
    {
        Task SaveAsync(FileItem file);
        Task<IEnumerable<FileItem>> GetAllAsync();
    }
}
