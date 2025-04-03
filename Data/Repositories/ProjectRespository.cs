using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace Data.Repositories;

public class ProjectRespository(DataContext context) : BaseRepository<ProjectEntity, Project>(context), IProjectRespository
{

    private readonly DataContext _context = context;

}

