using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Repositories.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using RPK_BlazorApp.Services;
using AutoMapper;
using System.Collections.Generic;
using RPK_BlazorApp.Models.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

using RPK_BlazorApp.Models.Interfaces;

namespace DataHub.Core.Services.Generic
{
    public class DataService<TEntity, TUIModel, TDataRequest, TDataResult, TContext> : IDataService<TEntity, TUIModel, TDataRequest, TDataResult, TContext>, IDataService<TEntity>
        where TEntity : class
        where TUIModel : class
        where TDataRequest : DataRequestBase
        where TDataResult : class
        where TContext : DbContext
    {
        protected readonly IRepository<TEntity, TContext> _repository;
        protected readonly IMapper _mapper;
        protected readonly ILogger<DataService<TEntity, TUIModel, TDataRequest, TDataResult, TContext>> _logger;
        protected readonly QueryParserService _queryParserService;

        public DataService(IRepository<TEntity, TContext> repository, IMapper mapper, ILogger<DataService<TEntity, TUIModel, TDataRequest, TDataResult, TContext>> logger, QueryParserService queryParserService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _queryParserService = queryParserService;
        }

        private IQueryable<TEntity> ApplyFilteringAndSorting(IQueryable<TEntity> query, DataFilterModel request)
        {
            if (!string.IsNullOrWhiteSpace(request.RawQuery))
            {
                _logger.LogInformation("DataService: ApplyFilteringAndSorting - Parsing RawQuery: {RawQuery}", request.RawQuery);
                try
                {
                    var parsedCriteria = _queryParserService.ParseQuery(request.RawQuery);
                    request.Criteria = parsedCriteria.Filters ?? new List<FilterCriterion>();
                    request.Sorts = parsedCriteria.Sorts ?? new List<SortCriterion>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing RawQuery: {RawQuery}", request.RawQuery);
                }
            }

            if (request.Criteria != null && request.Criteria.Any())
            {
                _logger.LogInformation("DataService: ApplyFilteringAndSorting - Applying Criteria. Count: {FilterCount}", request.Criteria.Count);
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var finalFilterExpression = ExpressionBuilder.BuildCombinedExpression<TEntity>(request.Criteria, parameter);
                if (finalFilterExpression != null)
                {
                    var lambda = Expression.Lambda<Func<TEntity, bool>>(finalFilterExpression, parameter);
                    query = query.Where(lambda);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                _logger.LogInformation("DataService: ApplyFilteringAndSorting - Applying Sorts. Count: {SortsCount}", request.Sorts.Count);
                IOrderedQueryable<TEntity>? orderedQuery = null;
                foreach (var sortCriterion in request.Sorts)
                {
                    var sortDirection = sortCriterion.SortDirection == "ASC" ? "ASC" : "DESC"; // Ensure consistent casing
                    var sortExpression = $"{sortCriterion.FieldName} {sortDirection}";
                    _logger.LogInformation("Applying sort expression: {SortExpression}", sortExpression);
                    orderedQuery = orderedQuery == null ? query.OrderBy(sortExpression) : orderedQuery.ThenBy(sortExpression);
                }
                query = orderedQuery ?? query;
            }

            return query;
        }

        public async Task<TDataResult> GetPagedAsync(int pageNumber, int pageSize, string? rawQuery = null, bool getAll = false)
        {
            _logger.LogInformation("DataService ({ServiceType}): GetPagedAsync received rawQuery: '{RawQuery}', getAll: {GetAll}", this.GetType().Name, rawQuery, getAll);
            return await GetPagedAsync(pageNumber, pageSize, _queryParserService.ParseQuery(rawQuery ?? string.Empty).Filters, _queryParserService.ParseQuery(rawQuery ?? string.Empty).Sorts, getAll);
        }

        public async Task<TDataResult> GetPagedAsync(int pageNumber, int pageSize, List<FilterCriterion>? filters = null, List<SortCriterion>? sorts = null, bool getAll = false)
        {
            _logger.LogInformation("DataService: GetPagedAsync (filters/sorts overload) - Filters count: {FilterCount}, Sorts count: {SortsCount}, getAll: {GetAll}", filters?.Count ?? 0, sorts?.Count ?? 0, getAll);
            var query = _repository.GetAll();

            var dataFilterModel = new DataFilterModel
            {
                Page = pageNumber,
                PageSize = pageSize,
                Criteria = filters ?? new List<FilterCriterion>(),
                Sorts = sorts ?? new List<SortCriterion>()
            };

            query = ApplyFilteringAndSorting(query, dataFilterModel);
            
            var totalCount = await query.CountAsync();
            List<TUIModel> mappedDataList;

            if (getAll)
            {
                var data = await query.ToListAsync(); // Materialize the data first
                _logger.LogInformation("DataService: GetPagedAsync (getAll) - Materialized data count: {DataCount}", data.Count);
                mappedDataList = _mapper.Map<List<TUIModel>>(data); // Then map it
                _logger.LogInformation("DataService: GetPagedAsync (getAll) - Mapped data count: {MappedDataCount}", mappedDataList.Count);
            }
            else
            {
                var pagedQuery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                mappedDataList = await _mapper.ProjectTo<TUIModel>(pagedQuery).ToListAsync();
            }

            var dataResult = new DataResult<TUIModel>
            {
                Data = mappedDataList,
                TotalCount = totalCount
            };

            return dataResult as TDataResult ?? throw new InvalidCastException($"Cannot cast DataResult<TUIModel> to {typeof(TDataResult)}");
        }

        public async Task<IEnumerable<TUIModel>> GetAllFilteredAsync(TDataRequest request)
        {
            var query = _repository.GetAll();
            // This method still uses TDataRequest, so we need to cast it to DataFilterModel
            // This might need further refinement if TDataRequest is not always DataFilterModel
            query = ApplyFilteringAndSorting(query, request as DataFilterModel ?? new DataFilterModel());
            var entities = await query.ToListAsync();
            return _mapper.Map<IEnumerable<TUIModel>>(entities);
        }

        public async Task<IEnumerable<TUIModel>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TUIModel>>(entities);
        }

        public async Task<TUIModel?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is not null)
            {
                await _repository.UpdateAsync(entity);
            }
            return _mapper.Map<TUIModel>(entity);
        }

        async Task<TEntity?> IDataService<TEntity>.GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity;
        }

        public async Task<TUIModel> AddAsync(TUIModel uiModel)
        {
            var entity = _mapper.Map<TEntity>(uiModel);
            await _repository.AddAsync(entity);
            return _mapper.Map<TUIModel>(entity);
        }

        async Task<TEntity> IDataService<TEntity>.AddAsync(TEntity item)
        {
            await _repository.AddAsync(item);
            return item;
        }

        public async Task<IEnumerable<TUIModel>> AddRangeAsync(IEnumerable<TUIModel> uiModels)
        {
            var entities = _mapper.Map<IEnumerable<TEntity>>(uiModels);
            await _repository.AddRangeAsync(entities);
            return _mapper.Map<IEnumerable<TUIModel>>(entities);
        }

        public async Task<TUIModel?> UpdateAsync(TUIModel uiModel)
        {
            var entity = _mapper.Map<TEntity>(uiModel);
            await _repository.UpdateAsync(entity);
            return _mapper.Map<TUIModel>(entity);
        }

        async Task<TEntity> IDataService<TEntity>.UpdateAsync(TEntity item)
        {
            await _repository.UpdateAsync(item);
            return item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            return true;
        }

        async Task IDataService<TEntity>.DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        async Task<ServerDataResult<TEntity>> IDataService<TEntity>.GetPagedAsync(ServerDataRequest request, Expression<Func<TEntity, bool>>? filter = null)
        {
            var query = _repository.GetAll();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(request.RawQuery))
            {
                try
                {
                    var parsedCriteria = _queryParserService.ParseQuery(request.RawQuery);
                    var dataFilterModel = new DataFilterModel
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        Criteria = parsedCriteria.Filters ?? new List<FilterCriterion>(),
                        Sorts = parsedCriteria.Sorts ?? new List<SortCriterion>()
                    };
                    query = ApplyFilteringAndSorting(query, dataFilterModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying raw query in IDataService<TEntity>.GetPagedAsync: {RawQuery}", request.RawQuery);
                }
            }

            var totalCount = await query.CountAsync();
            List<TEntity> pagedData;

            if (request.GetAll)
            {
                pagedData = await query.ToListAsync();
            }
            else
            {
                pagedData = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();
            }

            return new ServerDataResult<TEntity>
            {
                Data = pagedData.AsQueryable(),
                TotalCount = totalCount
            };
        }
    }
}
