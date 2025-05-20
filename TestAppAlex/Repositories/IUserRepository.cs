using TestAppAlex.Models;

namespace TestAppAlex.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task AddAsync(User user);
        Task DeleteByEmailAsync(string email);
        Task DeleteAsync(User user);
        Task SaveAsync();

    }
}
