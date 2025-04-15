using Business.Dtos;
using Data.Entities;
using Domain.Models;

namespace Business.Interfaces;

public interface IProjectMembershipService
{
    public Task<ProjectManagementResult<IEnumerable<User>>> GetProjectMembersAsync(Guid projectId);
    public Task<ProjectManagementResult<IEnumerable<Project>>> GetUserProjectsAsync(Guid userId);
    public Task<ProjectManagementResult> AddUserToProjectAsync(Guid projectId, Guid userId);
    public Task<ProjectManagementResult> RemoveUserFromProjectAsync(Guid projectId, Guid userId);
    public Task<ProjectManagementResult<IEnumerable<User>>> GetAvailableUsersForProjectAsync(Guid projectId);
    public Task<ProjectManagementResult<IEnumerable<Project>>> GetAvailableProjectsForUserAsync(Guid userId);
    public Task<ProjectManagementResult> RequestProjectAssignmentAsync(Guid projectId, Guid userId, string message);
    public Task<ProjectManagementResult> ApproveProjectRequestAsync(Guid requestId);
    public Task<ProjectManagementResult> RejectProjectRequestAsync(Guid requestId);
    public Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetPendingRequestsForProjectAsync(Guid projectId);
    public Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetPendingRequestsForUserAsync(Guid userId);
    public Task<ProjectManagementResult> CancelProjectRequestAsync(Guid requestId);
    public Task<ProjectManagementResult<ProjectRequest>> GetRequestByIdAsync(Guid requestId);
    public Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetAllPendingRequestsAsync();

    public Task<ProjectManagementResult<IEnumerable<Project>>> GetAvailableActiveProjectsForUserAsync(Guid userId);
}
