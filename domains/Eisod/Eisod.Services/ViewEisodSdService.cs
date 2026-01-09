using AutoMapper;
using Eisod.Shared.Models;
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Models.UI;
using Eisod.Services.Interfaces; // Use local interfaces
using Eisod.Services.Data; // Use local context
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using DataHub.Core.Services;
using DataHub.Core.Data; // For base generic types if needed? No, context is EisodDbContext

namespace Eisod.Services
{
    public class ViewEisodSdService : DataService<ViewEisodSd>, IViewEisodSdService
    {
        // private new readonly ILogger<ViewEisodSdService> _logger;

        public ViewEisodSdService(IViewEisodSdRepository repository, IMapper mapper, ILogger<ViewEisodSdService> logger, QueryParserService queryParserService) : base(repository, mapper, logger, queryParserService)
        {
        }
    }
}