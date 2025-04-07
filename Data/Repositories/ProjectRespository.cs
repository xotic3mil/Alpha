using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Extensions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;


namespace Data.Repositories;

public class ProjectRespository(DataContext context) : BaseRepository<ProjectEntity, Project>(context), IProjectRespository
{

    private readonly DataContext _context = context;


    public async Task<RepositoryResult<Project>> GetWithDetailsAsync(Expression<Func<ProjectEntity, bool>> predicate)
    {
        try
        {
            var entity = await _context.Projects
                .Include(p => p.Status)
                .Include(p => p.Service)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(predicate);

            if (entity == null)
                return new RepositoryResult<Project> { Succeeded = false, StatusCode = 404, Error = "Project not found." };

            var project = entity.MapTo<Project>();
            return new RepositoryResult<Project> { Succeeded = true, StatusCode = 200, Result = project };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<Project> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

}

