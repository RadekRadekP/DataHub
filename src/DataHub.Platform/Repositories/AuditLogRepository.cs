using DataHub.Platform.Data;
using DataHub.Core.Models;
using DataHub.Platform.Repositories.Generic;

using ClosedXML.Excel;
using System.IO;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DataHub.Core.Services;
using DataHub.Core.Models.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories
{
    public class AuditLogRepository : Repository<AuditLog, ApplicationDbContext>, IAuditLogRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public AuditLogRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<System.IO.Stream> ExportToExcelAsync(DataFilterModel filter)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var query = context.AuditLogs.AsQueryable();

            if (filter.Criteria != null && filter.Criteria.Any())
            {
                var parameter = Expression.Parameter(typeof(AuditLog), "a");
                var combinedExpression = ExpressionBuilder.BuildCombinedExpression<AuditLog>(filter.Criteria, parameter);
                if (combinedExpression != null)
                {
                    var lambda = Expression.Lambda<Func<AuditLog, bool>>(combinedExpression, parameter);
                    query = query.Where(lambda);
                }
            }

            // Apply Sorting
            if (filter.Sorts != null && filter.Sorts.Any())
            {
                IOrderedQueryable<AuditLog>? orderedQuery = null;
                foreach (var sortCriterion in filter.Sorts)
                {
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
            else
            {
                query = query.OrderByDescending(a => a.Timestamp);
            }

            var auditLogs = await query.AsNoTracking().ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("AuditLogs");
                worksheet.Cell(1, 1).InsertTable(auditLogs);
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                var stream = new System.IO.MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }
    }
}