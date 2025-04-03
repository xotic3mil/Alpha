using Domain.Models;

namespace Business.Dtos;

public class ServiceResult : StatusResults
{
    public IEnumerable<Service>? Result { get; set; }

}

public class ServiceResult<T> : StatusResults
{
    public T? Result { get; set; }
}
