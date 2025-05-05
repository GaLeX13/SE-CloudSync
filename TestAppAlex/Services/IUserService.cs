using TestAppAlex.Models;

namespace TestAppAlex.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string name, string email, string password);
        Task<User?> LoginAsync(string email, string password);
    }
}
