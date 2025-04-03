using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;

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


    public async Task<ServiceResult> CreateService(ServiceRegForm form)
    {
        var existingService = await _serviceRepository.ExistsAsync(x => x.ServiceName == form.ServiceName);
        if (existingService.Result)
            return new ServiceResult { Succeeded = false, StatusCode = 409, Error = "Status already exists." };

        var serviceEntity = new ServiceEntity { ServiceName = form.ServiceName };
        var result = await _serviceRepository.CreateAsync(serviceEntity);

        return result.Succeeded
            ? new ServiceResult { Succeeded = true, StatusCode = 201 }
            : new ServiceResult { Succeeded = false, StatusCode = 500, Error = "Failed to create status." };
    }


}
