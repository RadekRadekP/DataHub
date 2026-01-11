using DataHub.Core.Models;
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Grinding.Services
{
    /// <summary>
    /// In-memory service for DummyMeta entities - clean implementation for metadata-driven development
    /// </summary>
    public class DummyMetaService : IDummyMetaService
    {
        private readonly ILogger<DummyMetaService> _logger;
        private readonly QueryParserService _queryParserService;
        private readonly List<DummyMeta> _data;
        private int _nextId = 1;

        public DummyMetaService(
            ILogger<DummyMetaService> logger,
            QueryParserService queryParserService)
        {
            _logger = logger;
            _queryParserService = queryParserService;
            _data = new List<DummyMeta>();
            
            // Initialize with sample data
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            var random = new Random(42); // Seed for consistent data
            var names = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi" };
            var descriptions = new[] 
            { 
                "High priority item", 
                "Standard operation", 
                "Routine task", 
                "Critical path", 
                "Optional enhancement",
                "Urgent maintenance required",
                "Scheduled for review",
                "Pending approval",
                "In progress",
                "Awaiting resources",
                "Completed successfully",
                "Requires investigation"
            };
            var tags = new[] { "TAG-A", "TAG-B", "TAG-C", "TAG-D", "TAG-E", "TAG-F", "META-01", "META-02", "PROD", "DEV", null };

            // Generate 500 sample records
            for (int i = 1; i <= 500; i++)
            {
                _data.Add(new DummyMeta
                {
                    Id = _nextId++,
                    Name = $"{names[random.Next(names.Length)]} Meta {i:D4}",
                    Description = descriptions[random.Next(descriptions.Length)],
                    CustomTag = tags[random.Next(tags.Length)],
                    CategoryId = random.Next(1, 6), // Assuming 5 categories
                    StatusId = random.Next(1, 4),   // Assuming 3 statuses
                    CreatedDate = DateTime.UtcNow.AddDays(-random.Next(0, 730)), // Up to 2 years
                    IsActive = random.Next(0, 10) > 2, // 80% active
                    Value = random.Next(0, 10) > 1 ? (decimal?)(random.Next(100, 50000) / 100.0m) : null, // Wider value range
                    ServerTimestamp = DateTime.UtcNow,
                    ChangeCounter = random.Next(1, 10) // Simulate different change counts
                });
            }

            _logger.LogInformation("DummyMetaService initialized with {Count} sample records", _data.Count);
        }

        public async Task<ServerDataResult<DummyMeta>> GetPagedAsync(ServerDataRequest request)
        {
            await Task.CompletedTask; // Make async

            IQueryable<DummyMeta> query = _data.AsQueryable();

            // TODO: Add advanced filtering support later if needed
            // For now, this is a clean implementation without complex query parsing

            var totalCount = query.Count();

            // Apply sorting
            if (request.Sorts != null && request.Sorts.Any())
            {
                var firstSort = request.Sorts.First();
                bool descending = firstSort.SortDirection?.ToLower() == "desc";
                query = ApplySort(query, firstSort.FieldName, descending);

                foreach (var sort in request.Sorts.Skip(1))
                {
                    bool sortDescending = sort.SortDirection?.ToLower() == "desc";
                    query = ApplyThenSort(query, sort.FieldName, sortDescending);
                }
            }
            else
            {
                // Default sort by Id
                query = query.OrderBy(x => x.Id);
            }

            // Apply pagination (unless GetAll is true)
            if (!request.GetAll)
            {
                var skip = (request.Page - 1) * request.PageSize;
                query = query.Skip(skip).Take(request.PageSize);
            }

            var data = query.ToList();

            return new ServerDataResult<DummyMeta>
            {
                Data = data.AsQueryable(),
                TotalCount = totalCount
            };
        }

        public async Task<DummyMeta?> GetByIdAsync(int id)
        {
            await Task.CompletedTask;
            return _data.FirstOrDefault(x => x.Id == id);
        }

        public async Task<DummyMeta> AddAsync(DummyMeta item)
        {
            await Task.CompletedTask;
            item.Id = _nextId++;
            item.CreatedDate = DateTime.UtcNow;
            item.ServerTimestamp = DateTime.UtcNow;
            item.ChangeCounter = 1;
            _data.Add(item);
            _logger.LogInformation("Added DummyMeta with Id: {Id}", item.Id);
            return item;
        }

        public async Task<DummyMeta> UpdateAsync(DummyMeta item)
        {
            await Task.CompletedTask;
            var existing = _data.FirstOrDefault(x => x.Id == item.Id);
            if (existing != null)
            {
                _data.Remove(existing);
                item.ServerTimestamp = DateTime.UtcNow;
                item.ChangeCounter++;
                _data.Add(item);
                _logger.LogInformation("Updated DummyMeta with Id: {Id}", item.Id);
            }
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            await Task.CompletedTask;
            var item = _data.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                _data.Remove(item);
                _logger.LogInformation("Deleted DummyMeta with Id: {Id}", id);
            }
        }

        private IQueryable<DummyMeta> ApplySort(IQueryable<DummyMeta> query, string fieldName, bool descending)
        {
            return fieldName switch
            {
                nameof(DummyMeta.Id) => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                nameof(DummyMeta.Name) => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                nameof(DummyMeta.Description) => descending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                nameof(DummyMeta.CustomTag) => descending ? query.OrderByDescending(x => x.CustomTag) : query.OrderBy(x => x.CustomTag),
                nameof(DummyMeta.CategoryId) => descending ? query.OrderByDescending(x => x.CategoryId) : query.OrderBy(x => x.CategoryId),
                nameof(DummyMeta.StatusId) => descending ? query.OrderByDescending(x => x.StatusId) : query.OrderBy(x => x.StatusId),
                nameof(DummyMeta.CreatedDate) => descending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                nameof(DummyMeta.IsActive) => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                nameof(DummyMeta.Value) => descending ? query.OrderByDescending(x => x.Value) : query.OrderBy(x => x.Value),
                _ => query.OrderBy(x => x.Id)
            };
        }

        private IQueryable<DummyMeta> ApplyThenSort(IQueryable<DummyMeta> query, string fieldName, bool descending)
        {
            var orderedQuery = (IOrderedQueryable<DummyMeta>)query;
            return fieldName switch
            {
                nameof(DummyMeta.Id) => descending ? orderedQuery.ThenByDescending(x => x.Id) : orderedQuery.ThenBy(x => x.Id),
                nameof(DummyMeta.Name) => descending ? orderedQuery.ThenByDescending(x => x.Name) : orderedQuery.ThenBy(x => x.Name),
                nameof(DummyMeta.Description) => descending ? orderedQuery.ThenByDescending(x => x.Description) : orderedQuery.ThenBy(x => x.Description),
                nameof(DummyMeta.CustomTag) => descending ? orderedQuery.ThenByDescending(x => x.CustomTag) : orderedQuery.ThenBy(x => x.CustomTag),
                nameof(DummyMeta.CategoryId) => descending ? orderedQuery.ThenByDescending(x => x.CategoryId) : orderedQuery.ThenBy(x => x.CategoryId),
                nameof(DummyMeta.StatusId) => descending ? orderedQuery.ThenByDescending(x => x.StatusId) : orderedQuery.ThenBy(x => x.StatusId),
                nameof(DummyMeta.CreatedDate) => descending ? orderedQuery.ThenByDescending(x => x.CreatedDate) : orderedQuery.ThenBy(x => x.CreatedDate),
                nameof(DummyMeta.IsActive) => descending ? orderedQuery.ThenByDescending(x => x.IsActive) : orderedQuery.ThenBy(x => x.IsActive),
                nameof(DummyMeta.Value) => descending ? orderedQuery.ThenByDescending(x => x.Value) : orderedQuery.ThenBy(x => x.Value),
                _ => orderedQuery
            };
        }
    }
}
