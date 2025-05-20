using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestAppAlex.Models;
using TestAppAlex.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task AddAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        return Task.FromResult(_users.Any(u => u.Email == email));
    }

    public Task DeleteByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        if (user != null)
            _users.Remove(user);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user)
    {
        _users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveAsync()
    {
        return Task.CompletedTask; // no DB here
    }
}
