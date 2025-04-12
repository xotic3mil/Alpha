using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Models;
using System.Diagnostics;

namespace Business.Services;

public class TimeEntryService(ITimeEntryRepository timeEntryRepository) : ITimeEntryService
{
    private readonly ITimeEntryRepository _timeEntryRepository = timeEntryRepository;

    public async Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetAllTimeEntriesAsync()
    {
        try
        {
            var result = await _timeEntryRepository.GetAllAsync();
            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.StatusCode,
                Result = result.Result,
                Error = result.Error
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving all time entries: {ex.Message}");
            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving time entries: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByProjectAsync(Guid projectId)
    {
        try
        {
            var result = await _timeEntryRepository.GetByProjectIdAsync(projectId);

            if (!result.Succeeded)
            {
                return new TimeEntryResult<IEnumerable<TimeEntry>>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error
                };
            }

            var entries = result.Result.Select(entity => entity.MapTo<TimeEntry>()).ToList();

            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = entries
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving time entries by project: {ex.Message}");
            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving project time entries: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByUserAsync(Guid userId)
    {
        try
        {
            var result = await _timeEntryRepository.GetByUserIdAsync(userId);

            if (!result.Succeeded)
            {
                return new TimeEntryResult<IEnumerable<TimeEntry>>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error
                };
            }

            var entries = result.Result.Select(entity => entity.MapTo<TimeEntry>()).ToList();

            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = entries
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving time entries by user: {ex.Message}");
            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving user time entries: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByTaskAsync(Guid taskId)
    {
        try
        {
            var result = await _timeEntryRepository.GetByTaskIdAsync(taskId);

            if (!result.Succeeded)
            {
                return new TimeEntryResult<IEnumerable<TimeEntry>>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error
                };
            }

            var entries = result.Result.Select(entity => entity.MapTo<TimeEntry>()).ToList();

            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = entries
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving time entries by task: {ex.Message}");
            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving task time entries: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<IEnumerable<TimeEntry>>> GetTimeEntriesByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            var result = await _timeEntryRepository.GetByDateRangeAsync(start, end);

            if (!result.Succeeded)
            {
                return new TimeEntryResult<IEnumerable<TimeEntry>>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error
                };
            }

            var entries = result.Result.Select(entity => entity.MapTo<TimeEntry>()).ToList();

            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = entries
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving time entries by date range: {ex.Message}");
            return new TimeEntryResult<IEnumerable<TimeEntry>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving time entries by date range: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<TimeEntry>> GetTimeEntryByIdAsync(Guid id)
    {
        try
        {
            var result = await _timeEntryRepository.GetAsync(t => t.Id == id);

            if (!result.Succeeded)
            {
                return new TimeEntryResult<TimeEntry>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error ?? "Time entry not found"
                };
            }

            return new TimeEntryResult<TimeEntry>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving time entry by ID: {ex.Message}");
            return new TimeEntryResult<TimeEntry>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving time entry: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<TimeEntry>> CreateTimeEntryAsync(TimeEntry timeEntry)
    {
        if (timeEntry == null)
            return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = 400, Error = "Time entry data cannot be null" };

        // Validate required fields
        if (timeEntry.ProjectId == Guid.Empty)
            return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = 400, Error = "Project ID is required" };

        if (timeEntry.UserId == Guid.Empty)
            return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = 400, Error = "User ID is required" };

        if (timeEntry.Hours <= 0)
            return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = 400, Error = "Hours must be greater than 0" };

        if (string.IsNullOrWhiteSpace(timeEntry.Description))
            return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = 400, Error = "Description is required" };

        try
        {

            timeEntry.Id = Guid.NewGuid();
            timeEntry.CreatedAt = DateTime.UtcNow;
            timeEntry.NormalizeTimeProperties();

            var timeEntryEntity = new TimeEntryEntity
            {
                Id = timeEntry.Id,
                Date = timeEntry.Date, 
                Hours = timeEntry.Hours,
                Description = timeEntry.Description ?? string.Empty,
                IsBillable = timeEntry.IsBillable,
                HourlyRate = timeEntry.HourlyRate,
                ProjectId = timeEntry.ProjectId,
                UserId = timeEntry.UserId,
                TaskId = timeEntry.TaskId,
                CreatedAt = timeEntry.CreatedAt 
            };

            var result = await _timeEntryRepository.CreateAsync(timeEntryEntity);

            if (!result.Succeeded)
                return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

            var getResult = await _timeEntryRepository.GetAsync(t => t.Id == timeEntryEntity.Id);
            return new TimeEntryResult<TimeEntry> { Succeeded = true, StatusCode = 201, Result = getResult.Result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating time entry: {ex.Message}");
            return new TimeEntryResult<TimeEntry> { Succeeded = false, StatusCode = 500, Error = $"An error occurred while creating time entry: {ex.Message}" };
        }
    }

    public async Task<TimeEntryResult> UpdateTimeEntryAsync(TimeEntry timeEntry)
    {
        if (timeEntry == null || timeEntry.Id == Guid.Empty)
            return new TimeEntryResult { Succeeded = false, StatusCode = 400, Error = "Time entry data cannot be null and must have a valid ID" };

        try
        {
            timeEntry.NormalizeTimeProperties();

            var timeEntryEntity = timeEntry.MapTo<TimeEntryEntity>();
            var result = await _timeEntryRepository.UpdateAsync(timeEntryEntity);

            return new TimeEntryResult
            {
                Succeeded = result.Succeeded,
                StatusCode = result.StatusCode,
                Error = result.Error
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating time entry: {ex.Message}");
            return new TimeEntryResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred while updating time entry: {ex.Message}" };
        }
    }

    public async Task<TimeEntryResult> DeleteTimeEntryAsync(Guid id)
    {
        try
        {
            var entityResult = await _timeEntryRepository.GetEntityByIdAsync(id);

            if (!entityResult.Succeeded)
            {
                return new TimeEntryResult
                {
                    Succeeded = false,
                    StatusCode = entityResult.StatusCode,
                    Error = entityResult.Error ?? "Time entry not found"
                };
            }

            var result = await _timeEntryRepository.DeleteAsync(entityResult.Result);
            return new TimeEntryResult
            {
                Succeeded = result.Succeeded,
                StatusCode = result.StatusCode,
                Error = result.Error
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting time entry: {ex.Message}");
            return new TimeEntryResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred while deleting time entry: {ex.Message}" };
        }
    }

    public async Task<TimeEntryResult<TimeEntrySummary>> GetTimeEntrySummaryByProjectAsync(Guid projectId)
    {
        try
        {
            var totalHoursResult = await _timeEntryRepository.GetTotalHoursByProjectAsync(projectId);
            if (!totalHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = totalHoursResult.StatusCode,
                    Error = totalHoursResult.Error
                };
            }

            var billableHoursResult = await _timeEntryRepository.GetTotalHoursByProjectAsync(projectId, true);
            if (!billableHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = billableHoursResult.StatusCode,
                    Error = billableHoursResult.Error
                };
            }

            var nonBillableHoursResult = await _timeEntryRepository.GetTotalHoursByProjectAsync(projectId, false);
            if (!nonBillableHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = nonBillableHoursResult.StatusCode,
                    Error = nonBillableHoursResult.Error
                };
            }


            var entriesResult = await _timeEntryRepository.GetByProjectIdAsync(projectId);
            if (!entriesResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = entriesResult.StatusCode,
                    Error = entriesResult.Error
                };
            }


            decimal totalBillableAmount = 0;
            foreach (var entry in entriesResult.Result)
            {
                if (entry.IsBillable)
                {
                    totalBillableAmount += (decimal)entry.Hours * entry.HourlyRate;
                }
            }

            var summary = new TimeEntrySummary
            {
                TotalHours = totalHoursResult.Result,
                BillableHours = billableHoursResult.Result,
                NonBillableHours = nonBillableHoursResult.Result,
                TotalBillableAmount = totalBillableAmount
            };

            return new TimeEntryResult<TimeEntrySummary> { Succeeded = true, StatusCode = 200, Result = summary };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting time entry summary by project: {ex.Message}");
            return new TimeEntryResult<TimeEntrySummary>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving time entry summary: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<TimeEntrySummary>> GetTimeEntrySummaryByUserAsync(Guid userId)
    {
        try
        {
            var totalHoursResult = await _timeEntryRepository.GetTotalHoursByUserAsync(userId);
            if (!totalHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = totalHoursResult.StatusCode,
                    Error = totalHoursResult.Error
                };
            }

            var billableHoursResult = await _timeEntryRepository.GetTotalHoursByUserAsync(userId, true);
            if (!billableHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = billableHoursResult.StatusCode,
                    Error = billableHoursResult.Error
                };
            }

            var nonBillableHoursResult = await _timeEntryRepository.GetTotalHoursByUserAsync(userId, false);
            if (!nonBillableHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = nonBillableHoursResult.StatusCode,
                    Error = nonBillableHoursResult.Error
                };
            }


            var entriesResult = await _timeEntryRepository.GetByUserIdAsync(userId);
            if (!entriesResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = entriesResult.StatusCode,
                    Error = entriesResult.Error
                };
            }


            decimal totalBillableAmount = 0;
            foreach (var entry in entriesResult.Result)
            {
                if (entry.IsBillable)
                {
                    totalBillableAmount += (decimal)entry.Hours * entry.HourlyRate;
                }
            }

            var summary = new TimeEntrySummary
            {
                TotalHours = totalHoursResult.Result,
                BillableHours = billableHoursResult.Result,
                NonBillableHours = nonBillableHoursResult.Result,
                TotalBillableAmount = totalBillableAmount
            };

            return new TimeEntryResult<TimeEntrySummary> { Succeeded = true, StatusCode = 200, Result = summary };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting time entry summary by user: {ex.Message}");
            return new TimeEntryResult<TimeEntrySummary>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving user time entry summary: {ex.Message}"
            };
        }
    }

    public async Task<TimeEntryResult<TimeEntrySummary>> GetTimeEntrySummaryByTaskAsync(Guid taskId)
    {
        try
        {
            var totalHoursResult = await _timeEntryRepository.GetTotalHoursByTaskAsync(taskId);
            if (!totalHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = totalHoursResult.StatusCode,
                    Error = totalHoursResult.Error
                };
            }

            var billableHoursResult = await _timeEntryRepository.GetTotalHoursByTaskAsync(taskId, true);
            if (!billableHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = billableHoursResult.StatusCode,
                    Error = billableHoursResult.Error
                };
            }

            var nonBillableHoursResult = await _timeEntryRepository.GetTotalHoursByTaskAsync(taskId, false);
            if (!nonBillableHoursResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = nonBillableHoursResult.StatusCode,
                    Error = nonBillableHoursResult.Error
                };
            }

            var entriesResult = await _timeEntryRepository.GetByTaskIdAsync(taskId);
            if (!entriesResult.Succeeded)
            {
                return new TimeEntryResult<TimeEntrySummary>
                {
                    Succeeded = false,
                    StatusCode = entriesResult.StatusCode,
                    Error = entriesResult.Error
                };
            }
            decimal totalBillableAmount = 0;
            foreach (var entry in entriesResult.Result)
            {
                if (entry.IsBillable)
                {
                    totalBillableAmount += (decimal)entry.Hours * entry.HourlyRate;
                }
            }

            var summary = new TimeEntrySummary
            {
                TotalHours = totalHoursResult.Result,
                BillableHours = billableHoursResult.Result,
                NonBillableHours = nonBillableHoursResult.Result,
                TotalBillableAmount = totalBillableAmount
            };

            return new TimeEntryResult<TimeEntrySummary> { Succeeded = true, StatusCode = 200, Result = summary };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting time entry summary by task: {ex.Message}");
            return new TimeEntryResult<TimeEntrySummary>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = $"An error occurred while retrieving task time entry summary: {ex.Message}"
            };
        }
    }
}