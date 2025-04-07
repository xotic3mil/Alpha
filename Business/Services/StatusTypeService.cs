using Business.Dtos;
using Business.Interfaces;
using Domain.Extensions;
using Data.Interfaces;
using System.Diagnostics;
using Domain.Models;
using Data.Entities;
using Data.Repositories;

namespace Business.Services;

public class StatusTypeService(IStatusTypeRepository statusTypeRepository) : IStatusTypeService
{
    private readonly IStatusTypeRepository _statusTypeRepository = statusTypeRepository;

    public async Task<StatusTypeResults<IEnumerable<Status>>> GetStatusesAsync()
    {
        var result = await _statusTypeRepository.GetAllAsync();
        return result.Succeeded
            ? new StatusTypeResults<IEnumerable<Status>> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusTypeResults<IEnumerable<Status>> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<StatusTypeResults<Status>> GetStatusByNameAsync(string StatusName)
    {
        var result = await _statusTypeRepository.GetAsync(x => x.StatusName == StatusName);
        return result.Succeeded
            ? new StatusTypeResults<Status> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusTypeResults<Status> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<StatusTypeResults<Status>> GetStatusByIdAsync(Guid id)
    {
        var result = await _statusTypeRepository.GetAsync(x => x.Id == id);
        return result.Succeeded
            ? new StatusTypeResults<Status> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusTypeResults<Status> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<StatusTypeResults> CreateStatus(StatusTypeRegForm form)
    {
        if (form == null)
            return new StatusTypeResults { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };
        var statusEntity = form.MapTo<StatusEntity>();

        var result = await _statusTypeRepository.CreateAsync(statusEntity);

        return result.Succeeded
            ? new StatusTypeResults { Succeeded = true, StatusCode = 201 }
            : new StatusTypeResults { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<StatusTypeResults<Status>> DeleteStatusAsync(Guid id)
    {
        var StatusResponse = await _statusTypeRepository.GetAsync(x => x.Id == id);

        if (!StatusResponse.Succeeded || StatusResponse.Result == null)
        {
            return new StatusTypeResults<Status> { Succeeded = false, StatusCode = 404, Error = $"Project with ID '{id}' not found." };
        }

        var StatusEntity = StatusResponse.Result.MapTo<StatusEntity>();
        var deleteResponse = await _statusTypeRepository.DeleteAsync(StatusEntity);

        return deleteResponse.Succeeded
            ? new StatusTypeResults<Status> { Succeeded = true, StatusCode = 200 }
            : new StatusTypeResults<Status> { Succeeded = false, StatusCode = 500, Error = "Failed to delete project." };
    }

    public async Task<StatusTypeResults> UpdateStatusAsync(StatusTypeRegForm form)
    {
        if (form == null || form.Id == Guid.Empty)
            return new StatusTypeResults { Succeeded = false, StatusCode = 400, Error = "Invalid project data" };

        try
        {
            var result = await _statusTypeRepository.UpdateAsync(form, p => p.Id == form.Id);

            return result.Succeeded
                ? new StatusTypeResults { Succeeded = true, StatusCode = 200 }
                : new StatusTypeResults { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating project: {ex.Message}");
            return new StatusTypeResults { Succeeded = false, StatusCode = 500, Error = $"An error occurred: {ex.Message}" };
        }
    }
}
