using AutoMapper;
using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Reflection;
using RPK_BlazorApp.Services.Generic;
using RPK_BlazorApp.Data;

namespace RPK_BlazorApp.Services
{
    public class ServerViewEisodSdService : DataService<ViewEisodSd, ViewEisodSdUIModel, DataRequestBase, DataResult<ViewEisodSdUIModel>, EisodDbContext>, IServerViewEisodSdService
    {
        private new readonly ILogger<ServerViewEisodSdService> _logger;

        public ServerViewEisodSdService(IViewEisodSdRepository repository, IMapper mapper, ILogger<ServerViewEisodSdService> logger, QueryParserService queryParserService) : base(repository, mapper, logger, queryParserService)
        {
            _logger = logger;
        }
    }
}