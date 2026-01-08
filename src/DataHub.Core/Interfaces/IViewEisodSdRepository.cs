using DataHub.Core.Models;
using DataHub.Core.Interfaces;
using DataHub.Platform.Data;

namespace DataHub.Core.Interfaces
{
    public interface IViewEisodSdRepository : IRepository<ViewEisodSd, EisodDbContext>
    {
    }
}