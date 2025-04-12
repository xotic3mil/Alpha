using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Models;
using System.Diagnostics;

namespace Business.Services;

public class ProjectTaskService(IProjectTaskRepository taskRepository) : IProjectTaskService
{
    private readonly IProjectTaskRepository _taskRepository = taskRepository;

    public async Task<ProjectTaskResult<IEnumerable<ProjectTask>>> GetAllTasksAsync()
    {
        try
        {
            var result = await _taskRepository.GetAllAsync();
            return new ProjectTaskResult<IEnumerable<ProjectTask>>
            { Succeeded = result.Succeeded, StatusCode = result.StatusCode, Result = result.Result, Error = result.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving all tasks: {ex.Message}");
            return new ProjectTaskResult<IEnumerable<ProjectTask>>
            { Succeeded = false, StatusCode = 500, Error = $"An error occurred while retrieving tasks: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult<IEnumerable<ProjectTask>>> GetTasksByProjectAsync(Guid projectId)
    {
        try
        {
            var result = await _taskRepository.GetByProjectIdAsync(projectId);

            if (!result.Succeeded)
            {
                return new ProjectTaskResult<IEnumerable<ProjectTask>>
                { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
            }

            var tasks = result.Result.Select(entity => entity.MapTo<ProjectTask>()).ToList();

            return new ProjectTaskResult<IEnumerable<ProjectTask>>
            { Succeeded = true, StatusCode = 200, Result = tasks };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving tasks by project: {ex.Message}");
            return new ProjectTaskResult<IEnumerable<ProjectTask>>
            { Succeeded = false, StatusCode = 500, Error = $"An error occurred while retrieving project tasks: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult<IEnumerable<ProjectTask>>> GetTasksAssignedToUserAsync(Guid userId)
    {
        try
        {
            var result = await _taskRepository.GetTasksAssignedToUserAsync(userId);

            if (!result.Succeeded)
            {
                return new ProjectTaskResult<IEnumerable<ProjectTask>>
                { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
            }

            var tasks = result.Result.Select(entity => entity.MapTo<ProjectTask>()).ToList();

            return new ProjectTaskResult<IEnumerable<ProjectTask>>
            { Succeeded = true, StatusCode = 200, Result = tasks };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving tasks assigned to user: {ex.Message}");
            return new ProjectTaskResult<IEnumerable<ProjectTask>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving assigned tasks: {ex.Message}"
            };
        }
    }

    public async Task<ProjectTaskResult<ProjectTask>> GetTaskByIdAsync(Guid id)
    {
        try
        {
            var result = await _taskRepository.GetAsync(t => t.Id == id);

            if (!result.Succeeded)
            {
                return new ProjectTaskResult<ProjectTask>
                { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error ?? "Task not found" };
            }

            return new ProjectTaskResult<ProjectTask>
            { Succeeded = true, StatusCode = 200, Result = result.Result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving task by ID: {ex.Message}");
            return new ProjectTaskResult<ProjectTask>
            { Succeeded = false, StatusCode = 500, Error = $"An error occurred while retrieving task: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult<ProjectTask>> CreateTaskAsync(ProjectTask task)
    {
        if (task == null)
            return new ProjectTaskResult<ProjectTask> { Succeeded = false, StatusCode = 400, Error = "Task data cannot be null" };

        try
        {
            task.NormalizeTimeProperties();

            var taskEntity = task.MapTo<ProjectTaskEntity>();
            var result = await _taskRepository.CreateAsync(taskEntity);

            if (!result.Succeeded)
                return new ProjectTaskResult<ProjectTask> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

            var getResult = await _taskRepository.GetAsync(t => t.Id == taskEntity.Id);
            return new ProjectTaskResult<ProjectTask> { Succeeded = true, StatusCode = 201, Result = getResult.Result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating task: {ex.Message}");
            return new ProjectTaskResult<ProjectTask> { Succeeded = false, StatusCode = 500, Error = $"An error occurred while creating task: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult> UpdateTaskAsync(ProjectTask task)
    {
        if (task == null || task.Id == Guid.Empty)
            return new ProjectTaskResult { Succeeded = false, StatusCode = 400, Error = "Task data cannot be null and must have a valid ID" };

        try
        {

            task.NormalizeTimeProperties();

            
            if (task.CompletedAt.HasValue && task.CompletedAt.Value.Kind != DateTimeKind.Utc)
            {
                task.CompletedAt = DateTime.SpecifyKind(task.CompletedAt.Value, DateTimeKind.Utc);
            }

            
            if (task.DueDate.HasValue && task.DueDate.Value.Kind != DateTimeKind.Utc)
            {
                task.DueDate = DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc);
            }

         
            if (task.CreatedAt.Kind != DateTimeKind.Utc)
            {
                task.CreatedAt = DateTime.SpecifyKind(task.CreatedAt, DateTimeKind.Utc);
            }

            var taskEntity = task.MapTo<ProjectTaskEntity>();
            var result = await _taskRepository.UpdateTaskAsync(taskEntity);

            return new ProjectTaskResult
            {
                Succeeded = result.Succeeded,
                StatusCode = result.StatusCode,
                Error = result.Error
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating task: {ex.Message}");
            return new ProjectTaskResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred while updating task: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult> DeleteTaskAsync(Guid id)
    {
        try
        {
            var getResult = await _taskRepository.GetAsync(t => t.Id == id);
            if (!getResult.Succeeded)
            {
                return new ProjectTaskResult
                {
                    Succeeded = false,
                    StatusCode = getResult.StatusCode,
                    Error = getResult.Error ?? "Task not found"
                };
            }

            var result = await _taskRepository.DeleteAsync(getResult.Result.MapTo<ProjectTaskEntity>());
            return new ProjectTaskResult
            {
                Succeeded = result.Succeeded,
                StatusCode = result.StatusCode,
                Error = result.Error
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting task: {ex.Message}");
            return new ProjectTaskResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred while deleting task: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult> CompleteTaskAsync(Guid id)
    {
        try
        {
            var result = await _taskRepository.MarkAsCompletedAsync(id);
            return new ProjectTaskResult
            {
                Succeeded = result.Succeeded,
                StatusCode = result.StatusCode,
                Error = result.Error
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error completing task: {ex.Message}");
            return new ProjectTaskResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred while completing task: {ex.Message}" };
        }
    }

    public async Task<ProjectTaskResult<ProjectTaskSummary>> GetTaskSummaryByProjectAsync(Guid projectId)
    {
        try
        {
            var totalTasksResult = await _taskRepository.GetTotalTaskCountByProjectAsync(projectId);
            if (!totalTasksResult.Succeeded)
            {
                return new ProjectTaskResult<ProjectTaskSummary>
                {
                    Succeeded = false,
                    StatusCode = totalTasksResult.StatusCode,
                    Error = totalTasksResult.Error
                };
            }

            var completedTasksResult = await _taskRepository.GetCompletedTaskCountByProjectAsync(projectId);
            if (!completedTasksResult.Succeeded)
            {
                return new ProjectTaskResult<ProjectTaskSummary>
                {
                    Succeeded = false,
                    StatusCode = completedTasksResult.StatusCode,
                    Error = completedTasksResult.Error
                };
            }

            var totalTasks = totalTasksResult.Result;
            var completedTasks = completedTasksResult.Result;
            var completionPercentage = totalTasks > 0
                ? Math.Round((double)completedTasks / totalTasks * 100, 2)
                : 0;

            var summary = new ProjectTaskSummary
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                CompletionPercentage = completionPercentage
            };

            return new ProjectTaskResult<ProjectTaskSummary> { Succeeded = true, StatusCode = 200, Result = summary };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting task summary: {ex.Message}");
            return new ProjectTaskResult<ProjectTaskSummary>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving task summary: {ex.Message}"
            };
        }
    }
}

