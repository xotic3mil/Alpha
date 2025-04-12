using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Extensions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace Data.Repositories
{
    public class TimeEntryRepository(DataContext context) : ITimeEntryRepository
    {
        private readonly DataContext _context = context;
        private readonly DbSet<TimeEntryEntity> _dbSet = context.Set<TimeEntryEntity>();

        public async Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByProjectIdAsync(Guid projectId)
        {

            try
            {
                var entries = await _dbSet
                    .Include(t => t.User)
                    .Include(t => t.Task)
                    .Where(t => t.ProjectId == projectId)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = entries
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching time entries by project: {ex.Message}");
                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var entries = await _dbSet
                    .Include(t => t.Project)
                    .Include(t => t.Task)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = entries
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching time entries by user: {ex.Message}");
                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByTaskIdAsync(Guid taskId)
        {
            try
            {
                var entries = await _dbSet
                    .Include(t => t.User) // Ensure that TimeEntry has a User property
                    .Include(t => t.Project)
                    .Where(t => t.TaskId == taskId)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = entries
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching time entries by task: {ex.Message}");
                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                var entries = await _dbSet
                    .Include(t => t.User) 
                    .Include(t => t.Project)
                    .Include(t => t.Task)
                    .Where(t => t.Date >= start && t.Date <= end)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = entries
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching time entries by date range: {ex.Message}");
                return new RepositoryResult<IEnumerable<TimeEntryEntity>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<double>> GetTotalHoursByProjectAsync(Guid projectId, bool? billable = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.ProjectId == projectId);

                if (billable.HasValue)
                {
                    query = query.Where(t => t.IsBillable == billable.Value);
                }

                var totalHours = await query.SumAsync(t => t.Hours);

                return new RepositoryResult<double>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = totalHours
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating total hours by project: {ex.Message}");
                return new RepositoryResult<double>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<double>> GetTotalHoursByUserAsync(Guid userId, bool? billable = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.UserId == userId);

                if (billable.HasValue)
                {
                    query = query.Where(t => t.IsBillable == billable.Value);
                }

                var totalHours = await query.SumAsync(t => t.Hours);

                return new RepositoryResult<double>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = totalHours
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating total hours by user: {ex.Message}");
                return new RepositoryResult<double>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<double>> GetTotalHoursByTaskAsync(Guid taskId, bool? billable = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.TaskId == taskId);

                if (billable.HasValue)
                {
                    query = query.Where(t => t.IsBillable == billable.Value);
                }

                var totalHours = await query.SumAsync(t => t.Hours);

                return new RepositoryResult<double>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = totalHours
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating total hours by task: {ex.Message}");
                return new RepositoryResult<double>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<RepositoryResult<bool>> CreateAsync(TimeEntryEntity entity)
        {
            if (entity == null)
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Time entry cannot be null" };

            try
            {
                if (entity.Date.Kind != DateTimeKind.Utc)
                {
                    entity.Date = DateTime.SpecifyKind(entity.Date, DateTimeKind.Utc);
                }

                if (entity.CreatedAt.Kind != DateTimeKind.Utc)
                {
                    entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
                }

                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return new RepositoryResult<bool> { Succeeded = true, StatusCode = 201, Result = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating time entry: {ex.Message}");
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<RepositoryResult<bool>> DeleteAsync(TimeEntryEntity entity)
        {
            if (entity == null)
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Time entry cannot be null" };

            try
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return new RepositoryResult<bool> { Succeeded = true, StatusCode = 200, Result = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting time entry: {ex.Message}");
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<RepositoryResult<IEnumerable<TimeEntry>>> GetAllAsync()
        {
            try
            {
                var timeEntities = await _dbSet
                    .Include(t => t.Project)
                    .Include(t => t.Task)
                    .Include(t => t.User)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                var timeEntries = timeEntities.Select(t => t.MapTo<TimeEntry>()).ToList();
                return new RepositoryResult<IEnumerable<TimeEntry>> { Succeeded = true, StatusCode = 200, Result = timeEntries };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting all time entries: {ex.Message}");
                return new RepositoryResult<IEnumerable<TimeEntry>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<RepositoryResult<TimeEntry>> GetAsync(Func<TimeEntryEntity, bool> where)
        {
            try
            {
                var entities = await _dbSet
                    .Include(t => t.User)
                    .Include(t => t.Project)
                    .Include(t => t.Task)
                    .ToListAsync();

                var timeEntity = entities.FirstOrDefault(where);

                if (timeEntity == null)
                    return new RepositoryResult<TimeEntry> { Succeeded = false, StatusCode = 404, Error = "Time entry not found" };

                var timeEntry = timeEntity.MapTo<TimeEntry>();
                return new RepositoryResult<TimeEntry> { Succeeded = true, StatusCode = 200, Result = timeEntry };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting time entry: {ex.Message}");
                return new RepositoryResult<TimeEntry> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<RepositoryResult<bool>> UpdateAsync(TimeEntryEntity entity)
        {
            if (entity == null)
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Time entry cannot be null" };

            try
            {
                if (entity.Date.Kind != DateTimeKind.Utc)
                {
                    entity.Date = DateTime.SpecifyKind(entity.Date, DateTimeKind.Utc);
                }

                if (entity.CreatedAt.Kind != DateTimeKind.Utc)
                {
                    entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
                }

                var existingEntry = await _dbSet.FindAsync(entity.Id);
                if (existingEntry == null)
                    return new RepositoryResult<bool> { Succeeded = false, StatusCode = 404, Error = "Time entry not found" };

                existingEntry.Date = entity.Date;
                existingEntry.Hours = entity.Hours;
                existingEntry.Description = entity.Description;
                existingEntry.IsBillable = entity.IsBillable;
                existingEntry.HourlyRate = entity.HourlyRate;

                if (entity.ProjectId != Guid.Empty)
                    existingEntry.ProjectId = entity.ProjectId;

                if (entity.TaskId.HasValue && entity.TaskId != Guid.Empty)
                    existingEntry.TaskId = entity.TaskId;

                if (entity.UserId != Guid.Empty)
                    existingEntry.UserId = entity.UserId;

                _context.Entry(existingEntry).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return new RepositoryResult<bool> { Succeeded = true, StatusCode = 200, Result = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating time entry: {ex.Message}");
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<RepositoryResult<TimeEntryEntity>> GetEntityByIdAsync(Guid id)
        {
            try
            {
                var entity = await _dbSet
                    .Include(t => t.User)
                    .Include(t => t.Project)
                    .Include(t => t.Task)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (entity == null)
                    return new RepositoryResult<TimeEntryEntity> { Succeeded = false, StatusCode = 404, Error = "Time entry not found" };

                return new RepositoryResult<TimeEntryEntity> { Succeeded = true, StatusCode = 200, Result = entity };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting time entry entity: {ex.Message}");
                return new RepositoryResult<TimeEntryEntity> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

    }
}

