using AutoMapper;
using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using RPK_BlazorApp.Services.Generic;
using RPK_BlazorApp.Data;

namespace RPK_BlazorApp.Services
{
    public class ViewEisodSdService : DataService<ViewEisodSd, ViewEisodSdUIModel, DataRequestBase, DataResult<ViewEisodSdUIModel>, EisodDbContext>, IViewEisodSdService
    {
        private new readonly ILogger<ViewEisodSdService> _logger;

        public ViewEisodSdService(IViewEisodSdRepository repository, IMapper mapper, ILogger<ViewEisodSdService> logger, QueryParserService queryParserService) : base(repository, mapper, logger, queryParserService)
        {
            _logger = logger;
        }
    }
}