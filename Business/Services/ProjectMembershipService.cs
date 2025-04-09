using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace Business.Services
{
    public class ProjectMembershipService(
        IProjectRespository projectRepository,
        IUserRepository userRepository,
        IProjectRequestRepository requestRepository) : IProjectMembershipService
    {
        private readonly IProjectRespository _projectRepository = projectRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IProjectRequestRepository _requestRepository = requestRepository;

        public async Task<ProjectManagementResult<IEnumerable<UserEntity>>> GetProjectMembersAsync(Guid projectId)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);

                if (project == null)
                    return new ProjectManagementResult<IEnumerable<UserEntity>>
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "Project not found"
                    };

                return new ProjectManagementResult<IEnumerable<UserEntity>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = project.Users ?? new List<UserEntity>()
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<UserEntity>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<ProjectEntity>>> GetUserProjectsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetWithProjectsAsync(userId);

                if (user == null)
                    return new ProjectManagementResult<IEnumerable<ProjectEntity>>
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "User not found"
                    };

                return new ProjectManagementResult<IEnumerable<ProjectEntity>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = user.Projects ?? new List<ProjectEntity>()
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<ProjectEntity>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult> AddUserToProjectAsync(Guid projectId, Guid userId)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);
                if (project == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Project not found" };

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "User not found" };

                if (project.Users == null)
                    project.Users = new List<UserEntity>();

                if (project.Users.Any(u => u.Id == userId))
                    return new ProjectManagementResult { Succeeded = true, StatusCode = 200, Message = "User already assigned to project" };

                project.Users.Add(user);
                await _projectRepository.SaveChangesAsync();

                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult> RemoveUserFromProjectAsync(Guid projectId, Guid userId)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);
                if (project == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Project not found" };

                var user = project.Users?.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "User not found in project" };

                project.Users.Remove(user);
                await _projectRepository.SaveChangesAsync();

                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<UserEntity>>> GetAvailableUsersForProjectAsync(Guid projectId)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);
                if (project == null)
                    return new ProjectManagementResult<IEnumerable<UserEntity>> { Succeeded = false, StatusCode = 404, Error = "Project not found" };

                var projectUserIds = project.Users?.Select(u => u.Id).ToList() ?? new List<Guid>();
                var availableUsers = await _userRepository.GetAllExceptAsync(projectUserIds);

                return new ProjectManagementResult<IEnumerable<UserEntity>> { Succeeded = true, StatusCode = 200, Result = availableUsers };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<UserEntity>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<ProjectEntity>>> GetAvailableProjectsForUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetWithProjectsAsync(userId);
                if (user == null)
                    return new ProjectManagementResult<IEnumerable<ProjectEntity>> { Succeeded = false, StatusCode = 404, Error = "User not found" };

                var userProjectIds = user.Projects?.Select(p => p.Id).ToList() ?? new List<Guid>();
                var availableProjects = await _projectRepository.GetAllExceptAsync(userProjectIds);

                return new ProjectManagementResult<IEnumerable<ProjectEntity>> { Succeeded = true, StatusCode = 200, Result = availableProjects };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<ProjectEntity>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult> RequestProjectAssignmentAsync(Guid projectId, Guid userId, string message)
        {
            try
            {
                // Check if user is already assigned to the project
                var project = await _projectRepository.GetWithUsersAsync(projectId);
                if (project == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Project not found" };

                if (project.Users?.Any(u => u.Id == userId) ?? false)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "User is already assigned to this project" };

                // Check if there's already a pending request
                var existingRequest = await _requestRepository.GetPendingRequestAsync(projectId, userId);
                if (existingRequest != null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "A pending request already exists" };

                // Create new request
                var request = new ProjectRequest
                {
                    ProjectId = projectId,
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    Message = message,
                    Status = "Pending"
                };

                await _requestRepository.CreateAsync(request);
                return new ProjectManagementResult { Succeeded = true, StatusCode = 201 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult> ApproveProjectRequestAsync(Guid requestId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Request not found" };

                if (request.Status != "Pending")
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "Request is not in pending status" };

                var addResult = await AddUserToProjectAsync(request.ProjectId, request.UserId);
                if (!addResult.Succeeded)
                    return addResult;

                request.Status = "Approved";
                request.ResolutionDate = DateTime.UtcNow;
                await _requestRepository.UpdateAsync(request);

                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult> RejectProjectRequestAsync(Guid requestId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Request not found" };

                if (request.Status != "Pending")
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "Request is not in pending status" };

                request.Status = "Rejected";
                request.ResolutionDate = DateTime.UtcNow;
                await _requestRepository.UpdateAsync(request);

                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetPendingRequestsForProjectAsync(Guid projectId)
        {
            try
            {
                var requests = await _requestRepository.GetPendingRequestsForProjectAsync(projectId);
                return new ProjectManagementResult<IEnumerable<ProjectRequest>> { Succeeded = true, StatusCode = 200, Result = requests };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<ProjectRequest>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetPendingRequestsForUserAsync(Guid userId)
        {
            try
            {
                var requests = await _requestRepository.GetPendingRequestsForUserAsync(userId);
                return new ProjectManagementResult<IEnumerable<ProjectRequest>> { Succeeded = true, StatusCode = 200, Result = requests };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<ProjectRequest>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }
    }
}