using Microsoft.EntityFrameworkCore;
using TestAppAlex.Models;
using TestAppAlex.Data;

namespace TestAppAlex.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;

        public UserRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(User user)
        {
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _ctx.Users.AnyAsync(u => u.Email == email);
        }

        public async Task DeleteByEmailAsync(string email)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                _ctx.Users.Remove(user);
                await _ctx.SaveChangesAsync();
            }
        }
        public async Task DeleteAsync(User user)
        {
            _ctx.Users.Remove(user);
            await _ctx.SaveChangesAsync(); 
        }


        public async Task SaveAsync()
        {
            await _ctx.SaveChangesAsync();
        }


    }
}
