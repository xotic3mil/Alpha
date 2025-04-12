using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace Data.Repositories;

public class ProjectTaskRepository(DataContext context) : BaseRepository<ProjectTaskEntity, ProjectTask>(context), IProjectTaskRepository
{

    public async Task<RepositoryResult<IEnumerable<ProjectTaskEntity>>> GetByProjectIdAsync(Guid projectId)
    {
        try
        {
            var tasks = await _dbSet
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return new RepositoryResult<IEnumerable<ProjectTaskEntity>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = tasks
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<IEnumerable<ProjectTaskEntity>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }

    public async Task<RepositoryResult<IEnumerable<ProjectTaskEntity>>> GetTasksAssignedToUserAsync(Guid userId)
    {
        try
        {
            var tasks = await _dbSet
                .Include(t => t.Project)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == userId && !t.IsCompleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return new RepositoryResult<IEnumerable<ProjectTaskEntity>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = tasks
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<IEnumerable<ProjectTaskEntity>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }

    public async Task<RepositoryResult<bool>> MarkAsCompletedAsync(Guid id)
    {
        try
        {
            var task = await _dbSet.FindAsync(id);
            if (task == null)
            {
                return new RepositoryResult<bool>
                {
                    Succeeded = false,
                    StatusCode = 404,
                    Error = "Task not found"
                };
            }

            task.IsCompleted = !task.IsCompleted;

            if (task.IsCompleted)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                task.CompletedAt = null;
            }

            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return new RepositoryResult<bool>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<bool>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }

    public async Task<RepositoryResult<int>> GetCompletedTaskCountByProjectAsync(Guid projectId)
    {
        try
        {
            var count = await _dbSet
                .CountAsync(t => t.ProjectId == projectId && t.IsCompleted);

            return new RepositoryResult<int>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = count
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<int>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }

    public async Task<RepositoryResult<int>> GetTotalTaskCountByProjectAsync(Guid projectId)
    {
        try
        {
            var count = await _dbSet
                .CountAsync(t => t.ProjectId == projectId);

            return new RepositoryResult<int>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = count
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<int>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }

    public async Task<RepositoryResult<bool>> UpdateTaskAsync(ProjectTaskEntity taskEntity)
    {
        try
        {
            var existingTask = await _dbSet
                .Where(t => t.Id == taskEntity.Id)
                .FirstOrDefaultAsync();

            if (existingTask == null)
            {
                return new RepositoryResult<bool>
                {
                    Succeeded = false,
                    StatusCode = 404,
                    Error = "Task not found"
                };
            }
            existingTask.Title = taskEntity.Title;
            existingTask.Description = taskEntity.Description;
            existingTask.Priority = taskEntity.Priority;
            existingTask.DueDate = taskEntity.DueDate != null ? DateTime.SpecifyKind(taskEntity.DueDate.Value, DateTimeKind.Utc) : null;
            existingTask.IsCompleted = taskEntity.IsCompleted;
            existingTask.AssignedToId = taskEntity.AssignedToId;
            existingTask.EstimatedHours = taskEntity.EstimatedHours;

            if (taskEntity.IsCompleted)
            {
                existingTask.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                existingTask.CompletedAt = null;
            }

            _context.Entry(existingTask).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return new RepositoryResult<bool>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<bool>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }
}

