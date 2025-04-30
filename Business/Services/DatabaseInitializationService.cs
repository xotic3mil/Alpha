using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Business.Services
{
    public class DatabaseInitializationService(DataContext dataContext)
    {
        private readonly DataContext _dataContext = dataContext;

        public async Task<bool> IsDatabaseInitializedAsync()
        {
            return await _dataContext.Users.AnyAsync();
        }
    }
}