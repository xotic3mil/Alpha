using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using System.Diagnostics;

namespace Business.Services;

public class ProjectService(IProjectRespository projectRespository, IStatusTypeService statusTypeService) : IProjectsService
{
    private readonly IProjectRespository _projectRespository = projectRespository;
    private readonly IStatusTypeService _statusTypeService = statusTypeService;

    public async Task<ProjectResult<Project>> CreateProjectAsync(ProjectRegForm form)
    {
        if (form == null)
            return new ProjectResult<Project> { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var projectEntity = form.MapTo<ProjectEntity>();

        var statusResult = await _statusTypeService.GetStatusByIdAsync(form.StatusId);
        var status = statusResult.Result;

        if (status == null)
        {
            return new ProjectResult<Project> { Succeeded = false, StatusCode = 404, Error = "Status not found." };
        }

        var result = await _projectRespository.CreateAsync(projectEntity);

        if (result.Succeeded)
        {
            var createdProject = await _projectRespository.GetAsync(p => p.Id == projectEntity.Id);
            return new ProjectResult<Project>
            {
                Succeeded = true,
                StatusCode = 201,
                Result = createdProject.Result
            };
        }
        else
        {
            return new ProjectResult<Project>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = result.Error
            };
        }
    }

    public async Task<ProjectResult<IEnumerable<Project>>> GetProjectsAsync()
    {
        var response = await _projectRespository.GetAllAsync(
            orderbyDescending: true,
            expression: x => x.Created,
            where: null,
            include => include.Status,
            include => include.Customer,
            include => include.Service,
            include => include.Users
        );

        var result = new ProjectResult<IEnumerable<Project>> { Succeeded = true, StatusCode = 200, Result = response.Result };
        return result;
    }

    public async Task<ProjectResult<Project>> GetProjectAsync(Guid id)
    {
        var response = await _projectRespository.GetAsync
            (
                where: x => x.Id == id,
                include => include.Status,
                include => include.Customer
            );
        return response.Succeeded
            ? new ProjectResult<Project> { Succeeded = true, StatusCode = 200, Result = response.Result }
            : new ProjectResult<Project> { Succeeded = false, StatusCode = 404, Error = $"Project with '{id}' was not found." };
    }

    public async Task<ProjectResult<Project>> GetProjectWithDetailsAsync(Guid id)
    {
        var result = await _projectRespository.GetWithDetailsAsync(p => p.Id == id);
        return result.Succeeded
            ? new ProjectResult<Project> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new ProjectResult<Project> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<ProjectResult<Project>> DeleteProjectAsync(Guid id)
    {
        var projectResponse = await _projectRespository.GetAsync(x => x.Id == id);

        if (!projectResponse.Succeeded || projectResponse.Result == null)
        {
            return new ProjectResult<Project> { Succeeded = false, StatusCode = 404, Error = $"Project with ID '{id}' not found." };
        }

        var projectEntity = projectResponse.Result.MapTo<ProjectEntity>();
        var deleteResponse = await _projectRespository.DeleteAsync(projectEntity);

        return deleteResponse.Succeeded
            ? new ProjectResult<Project> { Succeeded = true, StatusCode = 200 }
            : new ProjectResult<Project> { Succeeded = false, StatusCode = 500, Error = "Failed to delete project." };
    }

    public async Task<ProjectResult> UpdateProjectAsync(ProjectRegForm form)
    {
        if (form == null || form.Id == Guid.Empty)
            return new ProjectResult { Succeeded = false, StatusCode = 400, Error = "Invalid project data" };

        try
        {
            var result = await _projectRespository.UpdateAsync(form, p => p.Id == form.Id);

            return result.Succeeded
                ? new ProjectResult { Succeeded = true, StatusCode = 200 }
                : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating project: {ex.Message}");
            return new ProjectResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred: {ex.Message}" };
        }
    }


}


