using Data.Entities;
using Data.Models;
using Domain.Models;
using System.Linq.Expressions;

namespace Data.Interfaces;

public interface IProjectRespository : IBaseRepository<ProjectEntity, Project>
{
    public Task<RepositoryResult<Project>> GetWithDetailsAsync(Expression<Func<ProjectEntity, bool>> predicate);

    public Task<ProjectEntity> GetWithUsersAsync(Guid projectId);

    public Task<ProjectEntity> GetWithProjectsAsync(Guid projectId);

    public Task<IEnumerable<ProjectEntity>> GetAllExceptAsync(List<Guid> excludeProjectIds);

    public Task SaveChangesAsync();



}
