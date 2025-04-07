using Business.Dtos;
using Domain.Models;


namespace Business.Interfaces
{
    public interface IProjectsService
    {
        public Task<ProjectResult> CreateProjectAsync(ProjectRegForm form);
        public Task<ProjectResult<IEnumerable<Project>>> GetProjectsAsync();
        public Task<ProjectResult<Project>> GetProjectAsync(Guid id);
        public Task<ProjectResult<Project>> DeleteProjectAsync(Guid id);
        public Task<ProjectResult> UpdateProjectAsync(ProjectRegForm form);
        public Task<ProjectResult<Project>> GetProjectWithDetailsAsync(Guid id);
    }
}
