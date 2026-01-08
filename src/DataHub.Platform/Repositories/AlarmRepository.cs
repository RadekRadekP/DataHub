using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DataHub.Platform.Data;
using DataHub.Core.Models;
using DataHub.Core.Models.UI;
using DataHub.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using DataHub.Platform.Repositories.Generic;
using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories
{
    public class AlarmRepository : Repository<Alarm, ApplicationDbContext>, IAlarmRepository
    {
        private readonly ILogger<AlarmRepository> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public AlarmRepository(ILogger<AlarmRepository> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Stream> ExportToExcelAsync(DataFilterModel filter)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var query = GetFilteredQuery(context, filter); // Pass the new context
            var alarms = await query.AsNoTracking().ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Alarms");
                worksheet.Cell(1, 1).InsertTable(alarms);
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }

        private IQueryable<Alarm> GetFilteredQuery(ApplicationDbContext context, DataFilterModel filter) // Accept DbContext as parameter
        {
            _logger.LogInformation("Building filtered query with filter: {@Filter}", filter);

            IQueryable<Alarm> query = context.Alarms;

            if (filter.Criteria != null && filter.Criteria.Any())
            {
                try
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Alarm), "a");
                    var combinedExpression = ExpressionBuilder.BuildCombinedExpression<Alarm>(filter.Criteria, parameter);
                    if (combinedExpression != null)
                    {
                        var lambda = System.Linq.Expressions.Expression.Lambda<Func<Alarm, bool>>(combinedExpression, parameter);
                        query = query.Where(lambda);
                        _logger.LogInformation("Applying filter criteria: {@Criteria}", filter.Criteria);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error building expression for Alarm filter.");
                }
            }

            // Apply Sorting
            if (filter.Sorts != null && filter.Sorts.Any())
            {
                IOrderedQueryable<Alarm>? orderedQuery = null;
                foreach (var sortCriterion in filter.Sorts)
                {
                    _logger.LogInformation("Applying sort column: {SortColumn}, direction: {SortDirection}", sortCriterion.FieldName, sortCriterion.SortDirection);
                    var sortExpression = $"{sortCriterion.FieldName} {(sortCriterion.SortDirection == "asc" ? "ASC" : "DESC")}";
                    if (orderedQuery == null)
                    {
                        orderedQuery = query.OrderBy(sortExpression);
                    }
                    else
                    {
                        orderedQuery = orderedQuery.ThenBy(sortExpression);
                    }
                }
                query = orderedQuery ?? query; // Use orderedQuery if available, else original query
            }

            var sqlQuery = query.ToQueryString();
            _logger.LogInformation("Final SQL query: {SqlQuery}", sqlQuery);

            return query;
        }

        public async Task<int> GetCountAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Alarms.CountAsync();
        }
    }
}