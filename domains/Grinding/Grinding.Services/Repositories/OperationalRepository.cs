using Grinding.Services.Data;
using Grinding.Shared.Models;
using DataHub.Platform.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using Grinding.Services.Interfaces;

namespace Grinding.Services.Repositories
{
    public class OperationalRepository : Repository<Operational, GrindingDbContext>, IOperationalRepository
    {
        public OperationalRepository(IDbContextFactory<GrindingDbContext> dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}
