using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAppAlex.Services;
using TestAppAlex.Repositories;
using TestAppAlex.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace SEuniTests
{
    [TestClass]
    public class UserServiceTests
    {
        [TestMethod]
        public async Task Register_ShouldSucceed_WithValidData()
        {
            var repo = new InMemoryUserRepository();
            var service = new UserService(repo);

            var result = await service.RegisterAsync("Test", "test@email.com", "1234");

            Assert.IsTrue(result);
        }
    }

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public Task AddAsync(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            return Task.FromResult(_users.Any(u => u.Email == email));
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        }

        public Task DeleteAsync(User user)
        {
            _users.Remove(user);
            return Task.CompletedTask;
        }

        public Task DeleteByEmailAsync(string email)
        {
            _users.RemoveAll(u => u.Email == email);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
