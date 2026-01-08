using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Services.Generic;
using System.Threading.Tasks;
using RPK_BlazorApp.Data;

namespace RPK_BlazorApp.Services
{
    public interface IServerViewEisodSdService : IDataService<ViewEisodSd, ViewEisodSdUIModel, DataRequestBase, DataResult<ViewEisodSdUIModel>, EisodDbContext>
    {
    }
}