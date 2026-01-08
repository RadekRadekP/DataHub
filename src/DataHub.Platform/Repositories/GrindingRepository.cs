using DataHub.Platform.Data;
using DataHub.Core.Models;
using DataHub.Platform.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories
{
    public class GrindingRepository : Repository<Grinding, ApplicationDbContext>, IGrindingRepository
    {
        public GrindingRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}