using DataHub.Platform.Data;
using DataHub.Core.Models;
using DataHub.Platform.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories
{
    public class OperationalRepository : Repository<Operational, ApplicationDbContext>, IOperationalRepository
    {
        public OperationalRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}