using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IProjectRequestRepository
    {
        Task<ProjectRequest> GetByIdAsync(Guid requestId);
        Task<ProjectRequest> GetPendingRequestAsync(Guid projectId, Guid userId);
        Task<IEnumerable<ProjectRequest>> GetPendingRequestsForProjectAsync(Guid projectId);
        Task<IEnumerable<ProjectRequest>> GetPendingRequestsForUserAsync(Guid userId);
        Task CreateAsync(ProjectRequest request);
        Task UpdateAsync(ProjectRequest request);
    }
}
