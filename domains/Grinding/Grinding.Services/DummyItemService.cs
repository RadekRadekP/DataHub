using Microsoft.Extensions.Logging;
using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RPK_BlazorApp.Models.Interfaces;
using RPK_BlazorApp.Services.Generic; // For ExpressionBuilder
using AutoMapper; // For IMapper
using System.Linq.Dynamic.Core; // For OrderBy and ThenBy
using RPK_BlazorApp.Models.UI; // Added for DataFilterModel, FilterCriterion, SortCriterion
using RPK_BlazorApp.Models.DataGrid; // Added for DataFilterModel, FilterCriterion, SortCriterion

namespace RPK_BlazorApp.Services
{
    public class DummyItemService : IDummyItemService
    {
        private readonly ILogger<DummyItemService> _logger;
        private readonly QueryParserService _queryParserService;
        private readonly IMapper _mapper; // Added IMapper
        private readonly List<DummyItem> _dummyItems;
        private int _nextId = 1;

        public DummyItemService(ILogger<DummyItemService> logger, QueryParserService queryParserService, IMapper mapper)
        {
            _logger = logger;
            _queryParserService = queryParserService;
            _mapper = mapper; // Assigned IMapper
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

        public Task<ServerDataResult<DummyItem>> GetPagedAsync(ServerDataRequest request, Expression<Func<DummyItem, bool>>? filter = null)
        {
            _logger.LogInformation("Getting paged DummyItems. Page: {Page}, PageSize: {PageSize}, RawQuery: {RawQuery}", request.Page, request.PageSize, request.RawQuery);

            IQueryable<DummyItem> query = _dummyItems.AsQueryable();

            // Apply external filter if provided (e.g., from GenericDataView)
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply filtering and sorting from RawQuery
            if (!string.IsNullOrWhiteSpace(request.RawQuery))
            {
                try
                {
                    var parsedCriteria = _queryParserService.ParseQuery(request.RawQuery);
                    var dataFilterModel = new DataFilterModel
                    {
                        Criteria = parsedCriteria.Filters,
                        Sorts = parsedCriteria.Sorts
                    };

                    // Apply filtering
                    if (dataFilterModel.Criteria != null && dataFilterModel.Criteria.Any())
                    {
                        var parameter = Expression.Parameter(typeof(DummyItem), "x");
                        var finalFilterExpression = ExpressionBuilder.BuildCombinedExpression<DummyItem>(dataFilterModel.Criteria, parameter);
                        if (finalFilterExpression != null)
                        {
                            var lambda = Expression.Lambda<Func<DummyItem, bool>>(finalFilterExpression, parameter);
                            query = query.Where(lambda);
                        }
                    }

                    // Apply sorting
                    if (dataFilterModel.Sorts != null && dataFilterModel.Sorts.Any())
                    {
                        IOrderedQueryable<DummyItem>? orderedQuery = null;
                        foreach (var sortCriterion in dataFilterModel.Sorts)
                        {
                            var sortDirection = sortCriterion.SortDirection == "ASC" ? "ASC" : "DESC";
                            var sortExpression = $"{sortCriterion.FieldName} {sortDirection}";
                            orderedQuery = orderedQuery == null ? query.OrderBy(sortExpression) : orderedQuery.ThenBy(sortExpression);
                        }
                        query = orderedQuery ?? query;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying raw query in DummyItemService.GetPagedAsync: {RawQuery}", request.RawQuery);
                }
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((request.Page - 1) * request.PageSize)
                                 .Take(request.PageSize)
                                 .ToList();

            return Task.FromResult(new ServerDataResult<DummyItem>
            {
                Data = pagedData.AsQueryable(),
                TotalCount = totalCount
            });
        }
    }
}