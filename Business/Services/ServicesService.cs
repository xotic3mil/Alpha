using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using System.Diagnostics;

namespace Business.Services;

public class ServicesService(IServiceRepository serviceRepository) : IServicesService
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    public async Task<ServiceResult<IEnumerable<Service>>> GetServicesAsync()
    {
        var result = await _serviceRepository.GetAllAsync();
        return result.Succeeded
            ? new ServiceResult<IEnumerable<Service>> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new ServiceResult<IEnumerable<Service>> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<ServiceResult<Service>> GetServiceByNameAsync(string serviceName)
    {
        var result = await _serviceRepository.GetAsync(x => x.ServiceName == serviceName);
        return result.Succeeded
            ? new ServiceResult<Service> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new ServiceResult<Service> { Succeeded = false, StatusCode = 500, Error = result.Error };

    }

    public async Task<ServiceResult<Service>> GetServiceByIdAsync(Guid id)
    {
        var result = await _serviceRepository.GetAsync(x => x.Id == id);
        return result.Succeeded
            ? new ServiceResult<Service> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new ServiceResult<Service> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<ServiceResult> CreateServiceAsync(ServiceRegForm form)
    {
        if (form == null)
            return new ServiceResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };
        var serviceEntity = form.MapTo<ServiceEntity>();

        var result = await _serviceRepository.CreateAsync(serviceEntity);

        return result.Succeeded
            ? new ServiceResult { Succeeded = true, StatusCode = 201 }
            : new ServiceResult { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<ServiceResult<Service>> DeleteServiceAsync(Guid id)
    {
        var serviceResponse = await _serviceRepository.GetAsync(x => x.Id == id);

        if (!serviceResponse.Succeeded || serviceResponse.Result == null)
        {
            return new ServiceResult<Service> { Succeeded = false, StatusCode = 404, Error = $"Service with ID '{id}' not found." };
        }

        var serviceEntity = serviceResponse.Result.MapTo<ServiceEntity>();
        var deleteResponse = await _serviceRepository.DeleteAsync(serviceEntity);

        return deleteResponse.Succeeded
            ? new ServiceResult<Service> { Succeeded = true, StatusCode = 200 }
            : new ServiceResult<Service> { Succeeded = false, StatusCode = 500, Error = "Failed to delete project." };
    }

    public async Task<ServiceResult> UpdateServiceAsync(ServiceRegForm form)
    {
        if (form == null || form.Id == Guid.Empty)
            return new ServiceResult { Succeeded = false, StatusCode = 400, Error = "Invalid Service data" };
        try
        {
            var result = await _serviceRepository.UpdateAsync(form, p => p.Id == form.Id);

            return result.Succeeded
                ? new ServiceResult { Succeeded = true, StatusCode = 200 }
                : new ServiceResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating project: {ex.Message}");
            return new ServiceResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred: {ex.Message}" };
        }
    }



}
