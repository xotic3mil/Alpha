using Business.Dtos;
using Business.Models;
using Data.Entities;

namespace Business.Factories;

public class ServiceFactory
{
    public static ServiceRegForm Create() => new();

    public static ServiceEntity Create(ServiceRegForm form) => new()
    {
        ServiceName = form.ServiceName,
        ServiceDescription = form.ServiceDescription,
        Budget = form.Budget,

    };

    public static Models.Services Create(ServiceEntity entity) => new()
    {
        Id = entity.Id,
        ServiceName = entity.ServiceName,
        ServiceDescription = entity.ServiceDescription,
        Budget = entity.Budget,

    };

    public static ServiceEntity Create(Models.Services service, int Id) => new()
    {
        Id = service.Id,
        ServiceName = service.ServiceName,
        ServiceDescription = service.ServiceDescription,
        Budget = service.Budget,


    };
}
