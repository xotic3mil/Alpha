using Business.Dtos;
using Domain.Models;

namespace Business.Interfaces;

public interface IServicesService
{
    public Task<ServiceResult<IEnumerable<Service>>> GetServicesAsync();
    public Task<ServiceResult<Service>> GetServiceByNameAsync(string serviceName);

    public Task<ServiceResult<Service>> GetServiceByIdAsync(Guid id);

    public Task<ServiceResult> CreateServiceAsync(ServiceRegForm form);

    public Task<ServiceResult> UpdateServiceAsync(ServiceRegForm form);

    public Task<ServiceResult<Service>> DeleteServiceAsync(Guid id);

}
