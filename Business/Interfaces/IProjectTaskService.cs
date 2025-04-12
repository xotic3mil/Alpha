using Business.Dtos;
using Domain.Models;

namespace Business.Interfaces;

public interface IProjectTaskService
{
    Task<ProjectTaskResult<IEnumerable<ProjectTask>>> GetAllTasksAsync();

    Task<ProjectTaskResult<IEnumerable<ProjectTask>>> GetTasksByProjectAsync(Guid projectId);

    Task<ProjectTaskResult<IEnumerable<ProjectTask>>> GetTasksAssignedToUserAsync(Guid userId);

    Task<ProjectTaskResult<ProjectTask>> GetTaskByIdAsync(Guid id);

    Task<ProjectTaskResult<ProjectTask>> CreateTaskAsync(ProjectTask task);

    Task<ProjectTaskResult> UpdateTaskAsync(ProjectTask task);

    Task<ProjectTaskResult> DeleteTaskAsync(Guid id);

    Task<ProjectTaskResult> CompleteTaskAsync(Guid id);

    Task<ProjectTaskResult<ProjectTaskSummary>> GetTaskSummaryByProjectAsync(Guid projectId);

}
