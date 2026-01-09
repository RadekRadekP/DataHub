using DataHub.Core.Models.Interfaces; // For IDataService ? No, IDataService in Core.Services
using DataHub.Core.Interfaces;
using Grinding.Shared.Models;

namespace Grinding.Services
{
    public interface IAlarmService : IDataService<Alarm>
    {
    }
}
