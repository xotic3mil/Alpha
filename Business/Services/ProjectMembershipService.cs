using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace Business.Services
{
    public class ProjectMembershipService(
        IProjectRespository projectRepository,
        IUserRepository userRepository,
        IProjectRequestRepository requestRepository,
        INotificationService notificationService) : IProjectMembershipService
    {
        private readonly IProjectRespository _projectRepository = projectRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IProjectRequestRepository _requestRepository = requestRepository;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<ProjectManagementResult<IEnumerable<User>>> GetProjectMembersAsync(Guid projectId)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);

                if (project == null)
                    return new ProjectManagementResult<IEnumerable<User>>
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "Project not found"
                    };

                var users = project.Users?.Select(u => u.MapTo<User>()) ?? Enumerable.Empty<User>();

                return new ProjectManagementResult<IEnumerable<User>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = users
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<User>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<Project>>> GetUserProjectsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetWithProjectsAsync(userId);

                if (user == null)
                    return new ProjectManagementResult<IEnumerable<Project>>
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "User not found"
                    };

                var projects = user.Projects?.Select(p => p.MapTo<Project>()) ?? Enumerable.Empty<Project>();

                return new ProjectManagementResult<IEnumerable<Project>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = projects
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<Project>>
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

        public async Task<ProjectManagementResult<IEnumerable<User>>> GetAvailableUsersForProjectAsync(Guid projectId)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);
                if (project == null)
                    return new ProjectManagementResult<IEnumerable<User>> { Succeeded = false, StatusCode = 404, Error = "Project not found" };

                var projectUserIds = project.Users?.Select(u => u.Id).ToList() ?? new List<Guid>();
                var availableUsers = await _userRepository.GetAllExceptAsync(projectUserIds);

                if (!availableUsers.Any())
                    return new ProjectManagementResult<IEnumerable<User>>
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "No available users found. Please create users first."
                    };
                var users = availableUsers.Select(u => u.MapTo<User>());

                return new ProjectManagementResult<IEnumerable<User>> { Succeeded = true, StatusCode = 200, Result = users };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<User>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<Project>>> GetAvailableActiveProjectsForUserAsync(Guid userId)
        {
            try
            {
                var availableProjectsResult = await GetAvailableProjectsForUserAsync(userId);
                if (!availableProjectsResult.Succeeded)
                    return availableProjectsResult;

                var activeProjects = availableProjectsResult.Result.Where(p =>
                    p.Status == null ||
                    !p.Status.StatusName.Equals("Completed", StringComparison.OrdinalIgnoreCase));

                return new ProjectManagementResult<IEnumerable<Project>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = activeProjects
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<Project>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<Project>>> GetAvailableProjectsForUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetWithProjectsAsync(userId);
                if (user == null)
                    return new ProjectManagementResult<IEnumerable<Project>> { Succeeded = false, StatusCode = 404, Error = "User not found" };

                var userProjectIds = user.Projects?.Select(p => p.Id).ToList() ?? new List<Guid>();

                var pendingRequestProjectIds = (await _requestRepository.GetPendingRequestsForUserAsync(userId))
                    .Select(r => r.ProjectId)
                    .ToList();

                var excludeProjectIds = userProjectIds.Union(pendingRequestProjectIds).ToList();

                var availableProjects = await _projectRepository.GetAllExceptAsync(excludeProjectIds);
                var projects = availableProjects.Select(p => p.MapTo<Project>());

                return new ProjectManagementResult<IEnumerable<Project>> { Succeeded = true, StatusCode = 200, Result = projects };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<Project>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult> RequestProjectAssignmentAsync(Guid projectId, Guid userId, string message)
        {
            try
            {
                var project = await _projectRepository.GetWithUsersAsync(projectId);
                if (project == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Project not found" };

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "User not found" };

                if (project.Users?.Any(u => u.Id == userId) ?? false)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "User is already assigned to this project" };

                var existingRequest = await _requestRepository.GetPendingRequestAsync(projectId, userId);
                if (existingRequest != null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "A pending request already exists" };

                var request = new ProjectRequest
                {
                    ProjectId = projectId,
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    Message = message,
                    Status = "Pending",
                    Id = Guid.NewGuid()
                };

                await _requestRepository.CreateAsync(request);

                await _notificationService.CreateAdminNotificationAsync(
                    title: "New Project Join Request",
                    message: $"{user.FirstName} {user.LastName} has requested to join \"{project.Name}\" project",
                    type: "ProjectJoinRequest",
                    relatedEntityId: request.Id);

                await _notificationService.CreateProjectManagerNotificationAsync(
                    title: "New Project Join Request",
                    message: $"{user.FirstName} {user.LastName} has requested to join \"{project.Name}\" project",
                    type: "ProjectJoinRequest",
                    relatedEntityId: request.Id);

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

        public async Task<ProjectManagementResult> CancelProjectRequestAsync(Guid requestId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 404, Error = "Request not found" };

                if (request.Status != "Pending")
                    return new ProjectManagementResult { Succeeded = false, StatusCode = 400, Error = "Only pending requests can be canceled" };

                request.Status = "Canceled";
                request.ResolutionDate = DateTime.UtcNow;
                await _requestRepository.UpdateAsync(request);

                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<ProjectRequest>> GetRequestByIdAsync(Guid requestId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);

                if (request == null)
                    return new ProjectManagementResult<ProjectRequest>
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "Project request not found"
                    };

                return new ProjectManagementResult<ProjectRequest>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = request
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<ProjectRequest>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }


        }

        public async Task<ProjectManagementResult<IEnumerable<ProjectRequest>>> GetAllPendingRequestsAsync()
        {
            try
            {
                var requests = await _requestRepository.GetAllPendingRequestsAsync();
                return new ProjectManagementResult<IEnumerable<ProjectRequest>>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = requests
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<ProjectRequest>>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }



    }

}