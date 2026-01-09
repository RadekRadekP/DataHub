using AutoMapper;
using Eisod.Shared.Models;
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Models.UI;
using Eisod.Services.Interfaces;
using Eisod.Services.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Reflection;
using DataHub.Core.Services;
using DataHub.Core.Data;

namespace Eisod.Services
{
    public class ServerViewEisodSdService : DataService<ViewEisodSd>, IServerViewEisodSdService
    {
        // private new readonly ILogger<ServerViewEisodSdService> _logger;

        public ServerViewEisodSdService(IViewEisodSdRepository repository, IMapper mapper, ILogger<ServerViewEisodSdService> logger, QueryParserService queryParserService) : base(repository, mapper, logger, queryParserService)
        {
        }
    }
}