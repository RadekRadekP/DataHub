using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Models.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataHub.Core.Services.Generic
{
    public interface IDataService<TEntity, TUIModel, TDataRequest, TDataResult, TContext>
        where TEntity : class
        where TUIModel : class
        where TDataRequest : DataRequestBase
        where TDataResult : class
    {
        Task<TDataResult> GetPagedAsync(int pageNumber, int pageSize, string? rawQuery = null, bool getAll = false);
        Task<TDataResult> GetPagedAsync(int pageNumber, int pageSize, List<FilterCriterion>? filters = null, List<SortCriterion>? sorts = null, bool getAll = false);
        Task<IEnumerable<TUIModel>> GetAllFilteredAsync(TDataRequest request);
        Task<IEnumerable<TUIModel>> GetAllAsync();
        Task<TUIModel?> GetByIdAsync(int id);
        Task<TUIModel> AddAsync(TUIModel uiModel);
        Task<IEnumerable<TUIModel>> AddRangeAsync(IEnumerable<TUIModel> uiModels);
        Task<TUIModel?> UpdateAsync(TUIModel uiModel);
        Task<bool> DeleteAsync(int id);
    }
}
