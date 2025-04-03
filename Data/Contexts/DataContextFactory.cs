using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Data.Contexts
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseNpgsql("Server = 192.168.1.6; Database = postpi; User Id = postpi; Password = !a*!FE^bKr$84RRg;");
            return new DataContext(optionsBuilder.Options);
        }
    }
}
