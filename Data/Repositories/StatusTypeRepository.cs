using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;

namespace Data.Repositories;

public class StatusTypeRepository(DataContext context) : BaseRepository<StatusEntity, Status>(context), IStatusTypeRepository
{
     private readonly DataContext _context = context;


}
