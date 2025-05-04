using System.Security.Cryptography;
using System.Text;
using TestAppAlex.Models;
using TestAppAlex.Repositories;

namespace TestAppAlex.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            if (await _repo.ExistsByEmailAsync(email))
                return false;

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = Hash(password)
            };

            await _repo.AddAsync(user);
            return true;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user == null) return null;

            return user.PasswordHash == Hash(password) ? user : null;
        }

        private string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
