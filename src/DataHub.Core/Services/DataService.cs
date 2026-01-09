using DataHub.Core.Interfaces;
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Models.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace DataHub.Core.Services
{
    public abstract class DataService<TItem> : IDataService<TItem> 
        where TItem : class 
    {
        protected readonly IRepository<TItem> _repository;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;
        protected readonly QueryParserService _queryParserService;

        public DataService(IRepository<TItem> repository, IMapper mapper, ILogger logger, QueryParserService queryParserService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _queryParserService = queryParserService;
        }

        public virtual async Task<TItem?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public virtual async Task<TItem> AddAsync(TItem item)
        {
            await _repository.AddAsync(item);
            return item;
        }

        public virtual async Task<TItem> UpdateAsync(TItem item)
        {
            await _repository.UpdateAsync(item);
            return item;
        }

        public virtual async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public virtual async Task<DataResult<TItem>> GetPagedAsync(DataRequestBase request)
        {
             var filterModel = request as DataFilterModel; 
             IQueryable<TItem> query = _repository.GetAll();

             if (filterModel != null)
             {
                if (!string.IsNullOrWhiteSpace(filterModel.RawQuery))
                {
                    try
                    {
                        var parsedCriteria = _queryParserService.ParseQuery(filterModel.RawQuery);
                        if (filterModel.Criteria == null) filterModel.Criteria = new List<FilterCriterion>();
                        filterModel.Criteria.AddRange(parsedCriteria.Filters);
                        
                         if (request.Sorts == null) request.Sorts = new List<SortCriterion>();
                         request.Sorts.AddRange(parsedCriteria.Sorts);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing query: {RawQuery}", filterModel.RawQuery);
                    }
                }

                if (filterModel.Criteria != null && filterModel.Criteria.Any())
                {
                    var parameter = Expression.Parameter(typeof(TItem), "x");
                    var finalFilterExpression = ExpressionBuilder.BuildCombinedExpression<TItem>(filterModel.Criteria, parameter);
                    if (finalFilterExpression != null)
                    {
                        var lambda = Expression.Lambda<Func<TItem, bool>>(finalFilterExpression, parameter);
                        query = query.Where(lambda);
                    }
                }
             }

             if (request.Sorts != null && request.Sorts.Any())
             {
                 string sortString = string.Join(",", request.Sorts.Select(s => $"{s.FieldName} {(s.SortDirection == "ASC" ? "ascending" : "descending")}"));
                 
                 if(!string.IsNullOrWhiteSpace(sortString))
                 {
                    query = query.OrderBy(sortString);
                 }
             }

             var totalCount = await query.CountAsync();
             var pagedData = await query.Skip((request.Page - 1) * request.PageSize)
                                  .Take(request.PageSize)
                                  .ToListAsync();

             return new DataResult<TItem>
             {
                 Data = pagedData.AsQueryable(),
                 TotalCount = totalCount,
                 TotalUnfilteredCount = 0 
             };
        }

        protected virtual async Task<DataResult<TItem>> GetPagedAsync(int page, int pageSize, string? rawQuery, bool useRawOnly = false)
        {
            var request = new DataFilterModel 
            { 
                Page = page, 
                PageSize = pageSize, 
                RawQuery = rawQuery 
            };
            return await GetPagedAsync(request);
        }
    }
}
