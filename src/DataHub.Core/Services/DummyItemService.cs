using Microsoft.Extensions.Logging;
using DataHub.Core.Models;
using DataHub.Core.Models.DataGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataHub.Core.Services; // For ExpressionBuilder
using AutoMapper; // For IMapper
using System.Linq.Dynamic.Core; // For OrderBy and ThenBy
using DataHub.Core.Models.UI;
using DataHub.Core.Interfaces; 

namespace DataHub.Core.Services
{
    public class DummyItemService : IDataService<DummyItem>
    {
        private readonly ILogger<DummyItemService> _logger;
        private readonly QueryParserService _queryParserService;
        private readonly IMapper _mapper;
        private readonly List<DummyItem> _dummyItems;
        private int _nextId = 1;

        public DummyItemService(ILogger<DummyItemService> logger, QueryParserService queryParserService, IMapper mapper)
        {
            _logger = logger;
            _queryParserService = queryParserService;
            _mapper = mapper;
            _dummyItems = new List<DummyItem>();
            // Seed 1000 initial data
            for (int i = 1; i <= 1000; i++)
            {
                _dummyItems.Add(new DummyItem
                {
                    Id = _nextId++,
                    Name = $"Dummy Item {i}",
                    Category = $"Category {(char)('A' + (i % 3))}", // Categories A, B, C
                    CreatedDate = DateTime.UtcNow.AddDays(-i),
                    IsActive = (i % 2 == 0), // Alternating active status
                    ServerTimestamp = DateTime.UtcNow,
                    ChangeCounter = 0
                });
            }
        }

        public Task<DummyItem?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting DummyItem by ID: {Id}", id);
            return Task.FromResult(_dummyItems.FirstOrDefault(item => item.Id == id));
        }

        public Task<DummyItem> AddAsync(DummyItem item)
        {
            item.Id = _nextId++;
            item.ServerTimestamp = DateTime.UtcNow;
            item.ChangeCounter = 0;
            _dummyItems.Add(item);
            _logger.LogInformation("Added DummyItem with ID: {Id}", item.Id);
            return Task.FromResult(item);
        }

        public Task<DummyItem> UpdateAsync(DummyItem item)
        {
            var existingItem = _dummyItems.FirstOrDefault(d => d.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.Category = item.Category;
                existingItem.CreatedDate = item.CreatedDate;
                existingItem.IsActive = item.IsActive;
                existingItem.ServerTimestamp = DateTime.UtcNow;
                existingItem.ChangeCounter++;
                _logger.LogInformation("Updated DummyItem with ID: {Id}", item.Id);
            }
            else
            {
                _logger.LogWarning("Attempted to update non-existent DummyItem with ID: {Id}", item.Id);
            }
            return Task.FromResult(item);
        }

        public Task DeleteAsync(int id)
        {
            var itemToRemove = _dummyItems.FirstOrDefault(item => item.Id == id);
            if (itemToRemove != null)
            {
                _dummyItems.Remove(itemToRemove);
                _logger.LogInformation("Deleted DummyItem with ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent DummyItem with ID: {Id}", id);
            }
            return Task.CompletedTask;
        }

        public Task<DataResult<DummyItem>> GetPagedAsync(DataRequestBase request)
        {
            // Cast to ServerDataRequest if possible, or use properties from DataRequestBase/DataFilterModel
            // Since DataFilterModel inherits DataRequestBase (indirectly via inheritance fix, wait...)
            // Actually ServerDataRequest inherits DataFilterModel which inherits DataRequestBase.
            // So we can check if request is DataFilterModel to access Criteria and RawQuery.
            
            var filterModel = request as DataFilterModel; 
            // Note: If request comes from GenericDataView, it is ServerDataRequest which is DataFilterModel.

            _logger.LogInformation("Getting paged DummyItems. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            IQueryable<DummyItem> query = _dummyItems.AsQueryable();

            if (filterModel != null)
            {
                 // Apply filtering and sorting from RawQuery
                if (!string.IsNullOrWhiteSpace(filterModel.RawQuery))
                {
                    try
                    {
                        var parsedCriteria = _queryParserService.ParseQuery(filterModel.RawQuery);
                        // Combine parsed criteria with existing criteria
                        if (filterModel.Criteria == null) filterModel.Criteria = new List<FilterCriterion>();
                        filterModel.Criteria.AddRange(parsedCriteria.Filters);
                        
                        // Combine parsed sorts
                         if (filterModel.Sorts == null) filterModel.Sorts = new List<SortCriterion>(); // DataRequestBase has Sorts
                         // Actually parseQuery returns Sorts too.
                         // But DataRequestBase has Sorts property.
                         // Let's use the ones from ParsedCriteria.
                         request.Sorts.AddRange(parsedCriteria.Sorts);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error applying raw query in DummyItemService.GetPagedAsync: {RawQuery}", filterModel.RawQuery);
                    }
                }

                // Apply Criteria (from RawQuery parsing or passed directly)
                if (filterModel.Criteria != null && filterModel.Criteria.Any())
                {
                    var parameter = Expression.Parameter(typeof(DummyItem), "x");
                    var finalFilterExpression = ExpressionBuilder.BuildCombinedExpression<DummyItem>(filterModel.Criteria, parameter);
                    if (finalFilterExpression != null)
                    {
                        var lambda = Expression.Lambda<Func<DummyItem, bool>>(finalFilterExpression, parameter);
                        query = query.Where(lambda);
                    }
                }
            }

            // Apply sorting from DataRequestBase
            if (request.Sorts != null && request.Sorts.Any())
            {
                IOrderedQueryable<DummyItem>? orderedQuery = null;
                foreach (var sortCriterion in request.Sorts)
                {
                    var sortDirection = sortCriterion.SortDirection == "ASC" ? "ASC" : "DESC";
                    var sortExpression = $"{sortCriterion.FieldName} {sortDirection}";
                    orderedQuery = orderedQuery == null ? query.OrderBy(sortExpression) : orderedQuery.ThenBy(sortExpression);
                }
                query = orderedQuery ?? query;
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((request.Page - 1) * request.PageSize)
                                 .Take(request.PageSize)
                                 .ToList();

            return Task.FromResult(new DataResult<DummyItem>
            {
                Data = pagedData.AsQueryable(),
                TotalCount = totalCount,
                TotalUnfilteredCount = _dummyItems.Count
            });
        }
    }
}
