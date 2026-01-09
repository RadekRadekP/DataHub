using Microsoft.EntityFrameworkCore;
using Eisod.Services.Data;
using Eisod.Shared.Models;
using DataHub.Platform.Repositories.Generic;
using Microsoft.Extensions.Logging;
using Eisod.Services.Interfaces;

namespace Eisod.Services.Repositories
{
    public class ViewEisodSdRepository : Repository<ViewEisodSd, EisodDbContext>, IViewEisodSdRepository
    {
        public ViewEisodSdRepository(IDbContextFactory<EisodDbContext> dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}
