using Data.Contexts;
using Data.Interfaces;
using Data.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Domain.Extensions;


namespace Data.Repositories;

public class ProjectRequestRepository(DataContext context) : IProjectRequestRepository
{
    private readonly DataContext _context = context;


    public async Task<ProjectRequest> GetByIdAsync(Guid requestId)
    {
        var entity = await _context.ProjectRequests
            .Include(r => r.Project)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        return entity?.MapTo<ProjectRequest>();
    }

    public async Task<ProjectRequest> GetPendingRequestAsync(Guid projectId, Guid userId)
    {
        var entity = await _context.ProjectRequests
            .Include(r => r.Project)
            .Include(r => r.User)
            .FirstOrDefaultAsync
            (r => r.ProjectId == projectId &&
            r.UserId == userId &&
            r.Status == "Pending");

        return entity?.MapTo<ProjectRequest>();
    }

    public async Task<IEnumerable<ProjectRequest>> GetPendingRequestsForProjectAsync(Guid projectId)
    {
        var entities = await _context.ProjectRequests
            .Include(r => r.Project)
            .Include(r => r.User)
            .Where(r => r.ProjectId == projectId && r.Status == "Pending")
            .ToListAsync();

        return entities.Select(e => e.MapTo<ProjectRequest>());
    }

    public async Task<IEnumerable<ProjectRequest>> GetPendingRequestsForUserAsync(Guid userId)
    {
        var entities = await _context.ProjectRequests
            .Include(r => r.Project)
            .Include(r => r.User)
            .Where(r => r.UserId == userId && r.Status == "Pending")
            .ToListAsync();

        return entities.Select(e => e.MapTo<ProjectRequest>());
    }

    public async Task CreateAsync(ProjectRequest request)
    {
        var entity = request.MapTo<ProjectRequestEntity>();
        entity.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

        _context.ProjectRequests.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProjectRequest request)
    {
        var existingEntity = await _context.ProjectRequests.FindAsync(request.Id);
        if (existingEntity == null)
        {
            throw new KeyNotFoundException($"ProjectRequest with ID {request.Id} not found");
        }
        existingEntity.Status = request.Status;
        existingEntity.ResolutionDate = request.ResolutionDate;
        existingEntity.Message = request.Message;

        _context.ProjectRequests.Update(existingEntity);
        await _context.SaveChangesAsync();
    }
}



