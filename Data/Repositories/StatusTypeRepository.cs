using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;

namespace Data.Repositories;

public class StatusTypeRepository(DataContext context) : BaseRepository<StatusTypesEntity, Status>(context), IStatusTypeRepository
{
     private readonly DataContext _context = context;


}
