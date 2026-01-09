using Grinding.Shared.Models;
using DataHub.Core.Interfaces;
using Grinding.Services.Data;

namespace Grinding.Services.Interfaces
{
    public interface IGrindingRepository : IRepository<Grinding.Shared.Models.Grinding>
    {
    }
}
