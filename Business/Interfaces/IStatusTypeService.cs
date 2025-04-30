using Business.Dtos;
using Domain.Models;

namespace Business.Interfaces;

public interface IStatusTypeService
{

    public Task<StatusTypeResults<IEnumerable<Status>>> GetStatusesAsync();
    public Task<StatusTypeResults<Status>> GetStatusByNameAsync(string statusName);
    public Task<StatusTypeResults<Status>> GetStatusByIdAsync(Guid id);
    public Task<StatusTypeResults> CreateStatus(StatusTypeRegForm form);
    public Task<StatusTypeResults<Status>> DeleteStatusAsync(Guid id);
    public Task<StatusTypeResults> UpdateStatusAsync(StatusTypeRegForm form);
}
