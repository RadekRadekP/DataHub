using Grinding.Services.Data;
using Grinding.Shared.Models;
using DataHub.Platform.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using Grinding.Services.Interfaces;

namespace Grinding.Services.Repositories
{
    public class GrindingRepository : Repository<Grinding.Shared.Models.Grinding, GrindingDbContext>, IGrindingRepository
    {
        public GrindingRepository(IDbContextFactory<GrindingDbContext> dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}
