using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;


namespace Data.Repositories
{
    public class UserRepository(DataContext context) : BaseRepository<UserEntity, User>(context), IUserRepository
    {

    }
}

