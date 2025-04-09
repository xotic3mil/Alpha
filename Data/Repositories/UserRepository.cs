using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace Data.Repositories
{
    public class UserRepository(DataContext context) : BaseRepository<UserEntity, User>(context), IUserRepository
    {

        public async Task<UserEntity> GetByIdAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<UserEntity> GetWithProjectsAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Projects)
                .ThenInclude(p => p.Status)
                .Include(u => u.Projects)
                .ThenInclude(p => p.Customer)
                .Include(u => u.Projects)
                .ThenInclude(p => p.Service)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<UserEntity>> GetAllExceptAsync(List<Guid> excludeIds)
        {
            return await _context.Users
                .Where(u => !excludeIds.Contains(u.Id))
                .ToListAsync();
        }
    }

}


