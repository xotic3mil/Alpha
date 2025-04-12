
using Business.Dtos;
using Domain.Models;

namespace Business.Interfaces;

public interface ITimeEntryService
{
    Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetAllTimeEntriesAsync();
    Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByProjectAsync(Guid projectId);
    Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByUserAsync(Guid userId);
    Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByTaskAsync(Guid taskId);
    Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByDateRangeAsync(DateTime start, DateTime end);
    Task<TimeEntryResult<TimeEntry>> GetTimeEntryByIdAsync(Guid id);
    Task<TimeEntryResult<TimeEntry>> CreateTimeEntryAsync(TimeEntry timeEntry);
    Task<TimeEntryResult> UpdateTimeEntryAsync(TimeEntry timeEntry);
    Task<TimeEntryResult> DeleteTimeEntryAsync(Guid id);
    Task<TimeEntryResult<TimeEntrySummary>> GetTimeEntrySummaryByProjectAsync(Guid projectId);
    Task<TimeEntryResult<TimeEntrySummary>> GetTimeEntrySummaryByUserAsync(Guid userId);
    Task<TimeEntryResult<TimeEntrySummary>> GetTimeEntrySummaryByTaskAsync(Guid taskId);

}
