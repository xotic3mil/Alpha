using Data.Entities;
using Data.Models;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface ITimeEntryRepository 
    {
        Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByProjectIdAsync(Guid projectId);
        Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByUserIdAsync(Guid userId);
        Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByTaskIdAsync(Guid taskId);
        Task<RepositoryResult<IEnumerable<TimeEntryEntity>>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<RepositoryResult<double>> GetTotalHoursByProjectAsync(Guid projectId, bool? billable = null);
        Task<RepositoryResult<double>> GetTotalHoursByUserAsync(Guid userId, bool? billable = null);
        Task<RepositoryResult<double>> GetTotalHoursByTaskAsync(Guid taskId, bool? billable = null);
        Task<RepositoryResult<bool>> CreateAsync(TimeEntryEntity entity);
        Task<RepositoryResult<bool>> DeleteAsync(TimeEntryEntity entity);
        Task<RepositoryResult<bool>> UpdateAsync(TimeEntryEntity entity);
        Task<RepositoryResult<IEnumerable<TimeEntry>>> GetAllAsync();
        Task<RepositoryResult<TimeEntry>> GetAsync(Func<TimeEntryEntity, bool> where);
        Task<RepositoryResult<TimeEntryEntity>> GetEntityByIdAsync(Guid id);

    }
}
