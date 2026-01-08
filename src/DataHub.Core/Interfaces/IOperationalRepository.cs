using DataHub.Core.Models;
using DataHub.Core.Interfaces;
using DataHub.Platform.Data;

namespace DataHub.Core.Interfaces
{
    public interface IOperationalRepository : IRepository<Operational, ApplicationDbContext>
    {
    }
}
