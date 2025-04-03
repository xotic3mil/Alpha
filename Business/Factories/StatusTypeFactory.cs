using Business.Dtos;
using Business.Models;
using Data.Entities;

namespace Business.Factories;

public class StatusTypeFactory
{
    public static StatusTypeRegForm Create() => new();

    public static StatusTypesEntity Create(StatusTypeRegForm form) => new()
    {
        StatusName = form.StatusName
    };

    public static StatusTypes Create(StatusTypesEntity entity) => new()
    {
        Id = entity.Id,
        StatusName = entity.StatusName
    };
    public static StatusTypesEntity Create(StatusTypes status) => new()
    {
        Id = status.Id,
        StatusName = status.StatusName
    };
}
