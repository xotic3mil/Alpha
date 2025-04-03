using Business.Dtos;
using Business.Interfaces;
using Business.Models;
using Domain.Extensions;
using Data.Interfaces;
using System.Diagnostics;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Data.Entities;

namespace Business.Services;

public class StatusTypeService(IStatusTypeRepository statusTypeRepository) : IStatusTypeService
{
    private readonly IStatusTypeRepository _statusTypeRepository = statusTypeRepository;


    public async Task<StatusTypeResults<IEnumerable<Domain.Models.Status>>> GetStatusesAsync() 
    {
        var result = await _statusTypeRepository.GetAllAsync();
        return result.Succeeded
            ? new StatusTypeResults<IEnumerable<Domain.Models.Status>> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusTypeResults<IEnumerable<Domain.Models.Status>> { Succeeded = false, StatusCode = 500, Error = result.Error };
       
    }

    public async Task<StatusTypeResults<Domain.Models.Status>> GetStatusByNameAsync(string StatusName)
    {
        var result = await _statusTypeRepository.GetAsync(x => x.StatusName == StatusName);
        return result.Succeeded
            ? new StatusTypeResults<Domain.Models.Status> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusTypeResults<Domain.Models.Status> { Succeeded = false, StatusCode = 500, Error = result.Error };

    }

    public async Task<StatusTypeResults<Domain.Models.Status>> GetStatusByIdAsync(Guid id)
    {
        var result = await _statusTypeRepository.GetAsync(x => x.Id == id);
        return result.Succeeded
            ? new StatusTypeResults<Domain.Models.Status> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusTypeResults<Domain.Models.Status> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }


    public async Task<StatusTypeResults> CreateStatus(StatusTypeRegForm form)
    {
        var existingStatus = await _statusTypeRepository.ExistsAsync(x => x.StatusName == form.StatusName);
        if (existingStatus.Result)
            return new StatusTypeResults { Succeeded = false, StatusCode = 409, Error = "Status already exists." };

        var statusEntity = new StatusEntity { StatusName = form.StatusName };
        var result = await _statusTypeRepository.CreateAsync(statusEntity);

        return result.Succeeded
            ? new StatusTypeResults { Succeeded = true, StatusCode = 201 }
            : new StatusTypeResults { Succeeded = false, StatusCode = 500, Error = "Failed to create status." };
    }



}
