using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataHub.Core.Models.Metadata;
using DataHub.Platform.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DataHub.Host.Services
{
    public class DynamicDataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _appContextFactory;
        private readonly IDbContextFactory<Eisod.Services.Data.EisodDbContext> _eisodContextFactory;
        private readonly IDbContextFactory<Grinding.Services.Data.GrindingDbContext> _grindingContextFactory;

        public DynamicDataService(
            IDbContextFactory<ApplicationDbContext> appContextFactory,
            IDbContextFactory<Eisod.Services.Data.EisodDbContext> eisodContextFactory,
            IDbContextFactory<Grinding.Services.Data.GrindingDbContext> grindingContextFactory)
        {
            _appContextFactory = appContextFactory;
            _eisodContextFactory = eisodContextFactory;
            _grindingContextFactory = grindingContextFactory;
        }

        public async Task<List<Dictionary<string, object>>> GetDataAsync(MetaEntity entity, List<MetaField> fields, int maxRows = 100)
        {
            // Determine which context to use based on DbContextName
            DbContext context = GetContextByName(entity.DbContextName);
            if (context == null) throw new InvalidOperationException($"Context {entity.DbContextName} not found.");

            // Construct SQL Query
            var tableName = $"[{entity.SchemaName}].[{entity.TableName}]";
            var columns = string.Join(", ", fields.Select(f => $"[{f.ColumnName}]"));
            
            if (string.IsNullOrEmpty(columns)) columns = "*";

            var sql = $"SELECT TOP {maxRows} {columns} FROM {tableName}";

            var results = new List<Dictionary<string, object>>();

            using (var connection = context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var colName = reader.GetName(i);
                                var field = fields.FirstOrDefault(f => f.ColumnName.Equals(colName, StringComparison.OrdinalIgnoreCase));
                                var key = field?.FieldName ?? colName;
                                
                                row[key] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }

            return results;
        }
        
        public async Task<Dictionary<string, object>?> GetRecordByIdAsync(MetaEntity entity, List<MetaField> fields, object id)
        {
             DbContext context = GetContextByName(entity.DbContextName);
             if (context == null) throw new InvalidOperationException($"Context {entity.DbContextName} not found.");

             var tableName = $"[{entity.SchemaName}].[{entity.TableName}]";
             var columns = string.Join(", ", fields.Select(f => $"[{f.ColumnName}]"));
             
             var pkField = fields.FirstOrDefault(f => f.IsPrimaryKey);
             var pkColumn = pkField?.ColumnName ?? "Id";

             var sql = $"SELECT TOP 1 {columns} FROM {tableName} WHERE [{pkColumn}] = @Id";

             using (var connection = context.Database.GetDbConnection())
             {
                 await connection.OpenAsync();
                 using (var command = connection.CreateCommand())
                 {
                     command.CommandText = sql;
                     
                     var param = command.CreateParameter();
                     param.ParameterName = "@Id";
                     param.Value = id;
                     command.Parameters.Add(param);

                     using (var reader = await command.ExecuteReaderAsync())
                     {
                         if (await reader.ReadAsync())
                         {
                             var row = new Dictionary<string, object>();
                             for (int i = 0; i < reader.FieldCount; i++)
                             {
                                 var colName = reader.GetName(i);
                                 var field = fields.FirstOrDefault(f => f.ColumnName.Equals(colName, StringComparison.OrdinalIgnoreCase));
                                 var key = field?.FieldName ?? colName;
                                 row[key] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                             }
                             return row;
                         }
                     }
                 }
             }
             return null;
        }

        private DbContext GetContextByName(string contextName)
        {
            switch (contextName)
            {
                case "ApplicationDbContext": return _appContextFactory.CreateDbContext();
                case "EisodDbContext": return _eisodContextFactory.CreateDbContext();
                case "GrindingDbContext": return _grindingContextFactory.CreateDbContext();
                default: 
                    return _appContextFactory.CreateDbContext(); 
            }
        }
    }
}
