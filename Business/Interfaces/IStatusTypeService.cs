using Business.Dtos;
using Domain.Models;

namespace Business.Interfaces;

public interface IStatusTypeService
{

    public Task<StatusTypeResults<IEnumerable<Status>>> GetStatusesAsync();
    public Task<StatusTypeResults<Status>> GetStatusByNameAsync(string statusName);
    public Task<StatusTypeResults<Status>> GetStatusByIdAsync(Guid id);
}
