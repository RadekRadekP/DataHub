using DataHub.Core.Services;
using Grinding.Services;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DataHub.Core.Interfaces; 

namespace Grinding.Services
{
    public class DummyItemService : DataHub.Core.Services.DummyItemService, IDummyItemService
    {
         public DummyItemService(ILogger<DataHub.Core.Services.DummyItemService> logger, QueryParserService queryParserService, IMapper mapper) 
            : base(logger, queryParserService, mapper)
         {
         }
    }
}