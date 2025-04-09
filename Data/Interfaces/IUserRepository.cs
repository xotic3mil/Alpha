using Data.Entities;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IUserRepository : IBaseRepository<UserEntity, User>
    {
        public Task<IEnumerable<UserEntity>> GetAllExceptAsync(List<Guid> excludeIds);

        public Task<UserEntity> GetWithProjectsAsync(Guid userId);

        public Task<UserEntity> GetByIdAsync(Guid userId);

        
    }
}
