using TestAppAlex.Models;

namespace TestAppAlex.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string name, string email, string password);
        Task<User?> LoginAsync(string email, string password);
        Task UpdatePlanAsync(string email, string plan);
        Task DeleteAccountAsync(string email);

    }
}
