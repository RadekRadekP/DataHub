using Microsoft.EntityFrameworkCore;
using DataHub.Platform.Data;
using DataHub.Core.Models;
using DataHub.Platform.Repositories.Generic;
using Microsoft.Extensions.Logging;
using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories
{
    public class ViewEisodSdRepository : Repository<ViewEisodSd, EisodDbContext>, IViewEisodSdRepository
    {
        // The logger is typically used in the service layer, not directly in the repository
        // private readonly ILogger<ViewEisodSdRepository> _logger;

        public ViewEisodSdRepository(IDbContextFactory<EisodDbContext> dbContextFactory) : base(dbContextFactory)
        {
            // _logger = logger;
        }
    }
}
