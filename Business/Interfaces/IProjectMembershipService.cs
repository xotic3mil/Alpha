using Business.Dtos;
using Data.Entities;
using Domain.Models;

namespace Business.Interfaces;

public interface IProjectMembershipService
{
    Task<ProjectManagementResult<IEnumerable<UserEntity>>> GetProjectMembersAsync(Guid projectId);
    Task<ProjectManagementResult<IEnumerable<ProjectEntity>>> GetUserProjectsAsync(Guid userId);
    Task<ProjectManagementResult> AddUserToProjectAsync(Guid projectId, Guid userId);
    Task<ProjectManagementResult> RemoveUserFromProjectAsync(Guid projectId, Guid userId);
    Task<ProjectManagementResult<IEnumerable<UserEntity>>> GetAvailableUsersForProjectAsync(Guid projectId);
    Task<ProjectManagementResult<IEnumerable<ProjectEntity>>> GetAvailableProjectsForUserAsync(Guid userId);
    Task<ProjectManagementResult> RequestProjectAssignmentAsync(Guid projectId, Guid userId, string message);
    Task<ProjectManagementResult> ApproveProjectRequestAsync(Guid requestId);
    Task<ProjectManagementResult> RejectProjectRequestAsync(Guid requestId);
    Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetPendingRequestsForProjectAsync(Guid projectId);
    Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetPendingRequestsForUserAsync(Guid userId);
}
