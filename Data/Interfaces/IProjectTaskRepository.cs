using Data.Entities;
using Data.Models;
using Domain.Models;

namespace Data.Interfaces;

public interface IProjectTaskRepository : IBaseRepository<ProjectTaskEntity, ProjectTask>
{
    Task<RepositoryResult<IEnumerable<ProjectTaskEntity>>> GetByProjectIdAsync(Guid projectId);
    Task<RepositoryResult<IEnumerable<ProjectTaskEntity>>> GetTasksAssignedToUserAsync(Guid userId);
    Task<RepositoryResult<bool>> MarkAsCompletedAsync(Guid id);
    Task<RepositoryResult<int>> GetCompletedTaskCountByProjectAsync(Guid projectId);
    Task<RepositoryResult<int>> GetTotalTaskCountByProjectAsync(Guid projectId);
    Task<RepositoryResult<bool>> UpdateTaskAsync(ProjectTaskEntity taskEntity);
}
