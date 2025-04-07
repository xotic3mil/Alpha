using Data.Entities;
using Data.Models;
using Domain.Models;
using System.Linq.Expressions;

namespace Data.Interfaces;

public interface IProjectRespository : IBaseRepository<ProjectEntity, Project>
{
    public Task<RepositoryResult<Project>> GetWithDetailsAsync(Expression<Func<ProjectEntity, bool>> predicate);
}
