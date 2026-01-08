using Microsoft.Extensions.DependencyInjection;
using RPK_BlazorClient.Models;
//using RPK_TestClient_CLI_CS_V0.Models; // Add this using directive for AppSettings
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPK_BlazorClient
{
    public static class MdbReader
    {
        // Mapping dictionary for MDB column names to standardized server-side names
        private static readonly Dictionary<string, string> ColumnMapping = new Dictionary<string, string>
        {
            { "GW Type", "gwType" },
            { "Finish time", "finishTime" },
            { "Program name", "programName" },
            { "Grinding time", "grindingTime" },
            { "Date start", "dateStart" },
            { "Upper GW start", "upperGwStart" },
            { "Lower GW start", "lowerGwStart" },
            { "Date", "clDate" },
            { "Object", "objectName" },
            { "Event ID", "eventId" },
            { "Lotto", "lotto" },
            { "Nr", "nr" },
            { "ID", "ID" }
        };

        [SupportedOSPlatform("windows")]
        public static async Task PrintConfiguredTables(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            var lastSentClientDbIds = LoadLastSentClientDbIds(settings.LastSentClientDbIdsFilePath);
            try
            {
                var tableConfigs = LoadTableConfigs(settings.TableConfigFilePath);
                using var connection = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={settings.MdbFilePath};");
                connection.Open();
                foreach (var tableConfig in tableConfigs)
                {
                    if (TableExists(connection, tableConfig.TableName))
                    {
                        Console.WriteLine($"\n--- Processing table: {tableConfig.TableName} ---");
                        int lastSentClientDbId = lastSentClientDbIds.ContainsKey(tableConfig.TableName) ? lastSentClientDbIds[tableConfig.TableName] : 0;
                        await ReadAndPrintConfiguredTable(tableConfig, connection, lastSentClientDbId, settings.BatchSize);
                    }
                    else
                    {
                        Console.WriteLine($"Table '{tableConfig.TableName}' not found in the MDB file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing configured tables: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows")]
        public static async Task ReadAndSendConfiguredTable(TableConfig tableConfig, OleDbConnection connection, IServiceProvider serviceProvider, string clientId)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            var lastSentClientDbIds = LoadLastSentClientDbIds(settings.LastSentClientDbIdsFilePath);
            // Use a separate variable for the query parameter that gets updated in the loop
            int currentQueryMinId = lastSentClientDbIds.ContainsKey(tableConfig.TableName) ? lastSentClientDbIds[tableConfig.TableName] : 0;
            int overallMaxIdForSaving = currentQueryMinId; // Track the max ID to save at the end

            try
            {
                if (tableConfig.Columns == null || tableConfig.Columns.Count == 0)
                {
                    Console.WriteLine($"Warning: No columns specified for table '{tableConfig.TableName}'. Skipping table.");
                    return;
                }

                // Build the SELECT statement with aliased column names
                List<string> selectColumns = new List<string>();
                foreach (string column in tableConfig.Columns)
                {
                    string formattedColumn;
                    string alias = column; // Default alias is the column name itself

                    // Use the mapping dictionary to get the standardized name
                    if (ColumnMapping.ContainsKey(column))
                    {
                        alias = ColumnMapping[column];
                    }

                    formattedColumn = $"[{column}] AS [{alias}]";
                    selectColumns.Add(formattedColumn);
                }

                // Remove duplicate column names
                selectColumns = selectColumns.Distinct().ToList();

                // Modify the base query to filter by ClientDbId
                // string baseQuery = $"SELECT {string.Join(", ", selectColumns)} FROM [{tableConfig.TableName}] WHERE [{tableConfig.ClientDbIdColumn}] > ?";
                // Count is less critical now, we can loop until no more rows are returned
                // string countQuery = $"SELECT COUNT(*) FROM [{tableConfig.TableName}] WHERE [{tableConfig.ClientDbIdColumn}] > ?";
                // using var countCommand = new OleDbCommand(countQuery, connection);
                // countCommand.Parameters.AddWithValue("?", currentQueryMinId);
                // object countResult = countCommand.ExecuteScalar();
                // int totalRecords = countResult != null && countResult != DBNull.Value ? Convert.ToInt32(countResult) : 0;

                // int offset = 0; // Offset less relevant now
                bool moreData = true;
                int batchNumber = 1;

                while(moreData) // Loop until a batch returns no data
                {
                    string batchQuery;
                    batchQuery = $"SELECT TOP {settings.BatchSize} {string.Join(", ", selectColumns)} FROM [{tableConfig.TableName}] WHERE [{tableConfig.ClientDbIdColumn}] > ? ORDER BY [{tableConfig.ClientDbIdColumn}]";
                    if (tableConfig.TableName == "Allarm")
                    {
                        batchQuery = batchQuery.Replace("[alarmDate]", "[Date]");
                    }
                    if (tableConfig.TableName == "Grinding")
                    {
                        batchQuery = batchQuery.Replace("[dateStart]", "[Date start]");
                    }
                    if (tableConfig.TableName == "Operational")
                    {
                        batchQuery = batchQuery.Replace("[objectName]", "[Object]");
                        batchQuery = batchQuery.Replace("[clDate]", "[Date]");
                    }

                    Console.WriteLine($"Executing SQL: {batchQuery}");

                    using var adapter = new OleDbDataAdapter(batchQuery, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("?", currentQueryMinId); // Use the updated ID
                    var dataTable = new DataTable();
                    try
                    {
                        adapter.Fill(dataTable);
                    }
                    catch (OleDbException ex)
                    {
                        Console.WriteLine($"Error reading table data: {ex.Message}");
                        if (ex.Errors.Count > 0)
                        {
                            Console.WriteLine($"Error details: {ex.Errors[0].Message}");
                        }
                        return;
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        Console.WriteLine($"Sending data from table '{tableConfig.TableName}' (Batch {batchNumber}):");
                        List<Dictionary<string, object>> batchData = ConvertDataTableToDictionaryList(dataTable, tableConfig.TableName, clientId);
                        // Check for null or empty values and set default values
                        foreach (var item in batchData)
                        {
                            if (item.ContainsKey("lotto") && string.IsNullOrEmpty(item["lotto"]?.ToString()))
                            {
                                item["lotto"] = "DefaultLotto";
                            }
                            if (item.ContainsKey("gwType") && string.IsNullOrEmpty(item["gwType"]?.ToString()))
                            {
                                item["gwType"] = "DefaultGwType";
                            }
                            if (item.ContainsKey("finishTime") && string.IsNullOrEmpty(item["finishTime"]?.ToString()))
                            {
                                item["finishTime"] = "00:00:00";
                            }
                            if (item.ContainsKey("programName") && string.IsNullOrEmpty(item["programName"]?.ToString()))
                            {
                                item["programName"] = "DefaultProgramName";
                            }
                            if (item.ContainsKey("grindingTime") && string.IsNullOrEmpty(item["grindingTime"]?.ToString()))
                            {
                                item["grindingTime"] = "00:00:00";
                            }
                            if (item.ContainsKey("objectName") && string.IsNullOrWhiteSpace(item["objectName"]?.ToString()))
                            {
                                item["objectName"] = null;
                            }
                            if (item.ContainsKey("ID") && item["ID"] == null)
                            {
                                item["ID"] = 0;
                            }
                        }
                        Console.WriteLine($"[DEBUG] JSON to send: {JsonSerializer.Serialize(batchData, new JsonSerializerOptions { WriteIndented = true })}");

                        // Send data and check success
                        bool success = await ApiConnector.PostBatchData(serviceProvider, JsonSerializer.Serialize(batchData, new JsonSerializerOptions { WriteIndented = true }), clientId, tableConfig.TableName);

                        if (success)
                        {
                            // Update the last sent ClientDbId ONLY if post was successful
                            int batchMaxClientDbId = dataTable.AsEnumerable().Max(row => row.Field<int>(tableConfig.ClientDbIdColumn));
                            overallMaxIdForSaving = Math.Max(overallMaxIdForSaving, batchMaxClientDbId); // Update overall max
                            currentQueryMinId = batchMaxClientDbId; // IMPORTANT: Update the ID for the next query

                            Console.WriteLine($"Waiting for {settings.SuspendTimeSeconds} seconds...");
                            await Task.Delay(TimeSpan.FromSeconds(settings.SuspendTimeSeconds));
                            batchNumber++; // Increment batch number for logging
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Batch failed to send. Stopping processing for table '{tableConfig.TableName}'.");
                            moreData = false; // Stop processing this table on failure
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No more new data found for table '{tableConfig.TableName}'.");
                        moreData = false; // No more data in this batch, exit loop
                    }
                }
                // Save the overall maximum ID processed for this table run
                int initialLastSentId = lastSentClientDbIds.ContainsKey(tableConfig.TableName) ? lastSentClientDbIds[tableConfig.TableName] : 0;
                if (overallMaxIdForSaving > initialLastSentId)
                {
                    lastSentClientDbIds[tableConfig.TableName] = overallMaxIdForSaving;
                    SaveLastSentClientDbIds(settings.LastSentClientDbIdsFilePath, lastSentClientDbIds);
                    Console.WriteLine($"Last sent ClientDbId for table '{tableConfig.TableName}' updated to: {overallMaxIdForSaving}");
                }
            }
            catch (OleDbException ex)
            {
                Console.WriteLine($"Error reading table data: {ex.Message}");
                if (ex.Errors.Count > 0)
                {
                    Console.WriteLine($"Error details: {ex.Errors[0].Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        [SupportedOSPlatform("windows")]
        private static async Task ReadAndPrintConfiguredTable(TableConfig tableConfig, OleDbConnection connection, int lastSentClientDbId, int batchSize)
        {
            try
            {
                if (tableConfig.Columns == null || tableConfig.Columns.Count == 0)
                {
                    Console.WriteLine($"Warning: No columns specified for table '{tableConfig.TableName}'. Skipping table.");
                    return;
                }
                List<string> formattedColumns = new List<string>();
                List<string> columns = new List<string>(tableConfig.Columns);
                foreach (string column in columns)
                {
                    formattedColumns.Add($"[{column}]");
                }
                // Remove duplicate column names
                formattedColumns = formattedColumns.Distinct().ToList();

                // Modify the query to filter by timestamp
                string baseQuery = $"SELECT {string.Join(", ", formattedColumns)} FROM [{tableConfig.TableName}] WHERE [{tableConfig.ClientDbIdColumn}] > ?";
                string countQuery = $"SELECT COUNT(*) FROM [{tableConfig.TableName}] WHERE [{tableConfig.ClientDbIdColumn}] > ?";

                using var countCommand = new OleDbCommand(countQuery, connection);
                countCommand.Parameters.AddWithValue("?", lastSentClientDbId);
                object countResult = countCommand.ExecuteScalar();
                int totalRecords = countResult != null && countResult != DBNull.Value ? Convert.ToInt32(countResult) : 0;

                int offset = 0;

                while (offset < totalRecords)
                {
                    string batchQuery;
                    batchQuery = $"SELECT TOP {batchSize} {string.Join(", ", formattedColumns)} FROM [{tableConfig.TableName}] WHERE [{tableConfig.ClientDbIdColumn}] > ? ORDER BY [{tableConfig.ClientDbIdColumn}]";
                    if (tableConfig.TableName == "Allarm")
                    {
                        batchQuery = batchQuery.Replace("[clDate]", "[Date]");
                    }
                    if (tableConfig.TableName == "Grinding")
                    {
                        batchQuery = batchQuery.Replace("[dateStart]", "[Date start]");
                    }
                    if (tableConfig.TableName == "Operational")
                    {
                        batchQuery = batchQuery.Replace("[objectName]", "[Object]");
                        batchQuery = batchQuery.Replace("[clDate]", "[Date]");
                    }

                    using var adapter = new OleDbDataAdapter(batchQuery, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("?", lastSentClientDbId);
                    var dataTable = new DataTable();
                    try
                    {
                        adapter.Fill(dataTable);
                    }
                    catch (OleDbException ex)
                    {
                        Console.WriteLine($"Error reading table data: {ex.Message}");
                        if (ex.Errors.Count > 0)
                        {
                            Console.WriteLine($"Error details: {ex.Errors[0].Message}");
                        }
                        return;
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        Console.WriteLine($"Printing data from table '{tableConfig.TableName}' (Batch {offset / batchSize + 1}):");
                        PrintTableData(dataTable, tableConfig);
                    }
                    else
                    {
                        Console.WriteLine($"Table '{tableConfig.TableName}' is empty.");
                    }
                    offset += batchSize;
                }
            }
            catch (OleDbException ex)
            {
                Console.WriteLine($"Error reading table data: {ex.Message}");
                if (ex.Errors.Count > 0)
                {
                    Console.WriteLine($"Error details: {ex.Errors[0].Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private static void PrintTableData(DataTable dataTable, TableConfig tableConfig)
        {
            // Print header
            foreach (DataColumn column in dataTable.Columns)
            {
                Console.Write($"{column.ColumnName}\t");
            }
            Console.WriteLine();

            // Print rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    // Format DateTime values
                    if (column.DataType == typeof(DateTime))
                    {
                        Console.Write($"{((DateTime)row[column]).ToString("dd.MM.yyyy HH:mm:ss")}\t");
                    }
                    else
                    {
                        Console.Write($"{row[column]}\t");
                    }
                }
                Console.WriteLine();
            }
        }

        public static List<TableConfig> LoadTableConfigs(string tableConfigFilePath)
        {
            if (!File.Exists(tableConfigFilePath))
            {
                throw new FileNotFoundException($"Table configuration file not found: {tableConfigFilePath}");
            }
            string json = File.ReadAllText(tableConfigFilePath);
            var tableConfigs = JsonSerializer.Deserialize<List<TableConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TableConfig>();
            return tableConfigs.Where(config => !string.IsNullOrWhiteSpace(config.TableName) && !string.IsNullOrWhiteSpace(config.ClientDbIdColumn)).ToList();
        }

        [SupportedOSPlatform("windows")]
        public static bool TableExists(OleDbConnection connection, string tableName)
        {
            var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object?[] { null, null, tableName, "TABLE" });
            return schemaTable?.Rows.Count > 0;
        }

        public static Dictionary<string, int> LoadLastSentClientDbIds(string lastSentClientDbIdsFilePath)
        {
            if (!File.Exists(lastSentClientDbIdsFilePath))
            {
                return new Dictionary<string, int>();
            }
            string json = File.ReadAllText(lastSentClientDbIdsFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
        }

        public static void SaveLastSentClientDbIds(string lastSentClientDbIdsFilePath, Dictionary<string, int> lastSentClientDbIds)
        {
            string json = JsonSerializer.Serialize(lastSentClientDbIds, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(lastSentClientDbIdsFilePath, json);
        }

        [SupportedOSPlatform("windows")]
        public static async Task ResetClientDbIds(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            try
            {
                var lastSentClientDbIds = LoadLastSentClientDbIds(settings.LastSentClientDbIdsFilePath);
                var tableConfigs = LoadTableConfigs(settings.TableConfigFilePath);

                foreach (var tableConfig in tableConfigs)
                {
                    if (settings.PredefinedClientDbIds.ContainsKey(tableConfig.TableName))
                    {
                        lastSentClientDbIds[tableConfig.TableName] = settings.PredefinedClientDbIds[tableConfig.TableName];
                    }
                    else
                    {
                        lastSentClientDbIds[tableConfig.TableName] = 0;
                    }
                }

                SaveLastSentClientDbIds(settings.LastSentClientDbIdsFilePath, lastSentClientDbIds);
                Console.WriteLine($"ClientDbIds reset to predefined values in last_sent_clientdbids.json.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting ClientDbIds: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows")]
        public static async Task PrintMdbStructure(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            try
            {
                using var connection = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={settings.MdbFilePath};");
                connection.Open();
                DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                if (schema != null && schema.Rows.Count > 0)
                {
                    Console.WriteLine("MDB File Structure:");
                    List<TableConfig> tableStructures = new List<TableConfig>();
                    foreach (DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        List<string> columnNames = GetColumnNames(connection, tableName);
                        TableConfig tableConfig = new TableConfig { TableName = tableName, Columns = columnNames };
                        tableStructures.Add(tableConfig);
                    }
                    Console.WriteLine(JsonSerializer.Serialize(tableStructures, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    Console.WriteLine("No tables found in the MDB file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing MDB structure: {ex.Message}");
            }
        }
        [SupportedOSPlatform("windows")]
        private static List<string> GetColumnNames(OleDbConnection connection, string tableName)
        {
            List<string> columnNames = new List<string>();
            try
            {
                using (OleDbCommand command = new OleDbCommand($"SELECT * FROM [{tableName}]", connection))
                {
                    using (OleDbDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        DataTable schemaTable = reader.GetSchemaTable();
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            columnNames.Add(row["ColumnName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting column names for table '{tableName}': {ex.Message}");
            }
            return columnNames;
        }

        private static List<Dictionary<string, object>> ConvertDataTableToDictionaryList(DataTable dataTable, string tableName, string clientId)
        {
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                rowData["TableName"] = tableName;
                rowData["ClientId"] = clientId;
                foreach (DataColumn column in dataTable.Columns)
                {
                    // Use the standardized name from the mapping
                    string columnName = column.ColumnName;
                    if (ColumnMapping.ContainsKey(column.ColumnName))
                    {
                        columnName = ColumnMapping[column.ColumnName];
                    }
                    else
                    {
                        columnName = column.ColumnName;
                    }

                    object value = row[column];

                    //Convert empty string to null
                    if (value is string strValue && string.IsNullOrEmpty(strValue))
                    {
                        value = null;
                    }
                    // Format values
                    if (column.DataType == typeof(DateTime))
                    {
                        if (columnName == "grindingTime" || columnName == "finishTime")
                        {
                            // Check if the value is a valid DateTime
                            if (value is DateTime dateTimeValue)
                            {
                                value = dateTimeValue.ToString("HH:mm:ss"); // Extract time portion only
                            }
                            else
                            {
                                value = "00:00:00"; // Default value if not a valid DateTime
                            }
                        }
                        else if (columnName == "dateStart" || columnName == "clDate")
                        {
                            if (value is DateTime dateTimeValue)
                            {
                                value = dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss"); // ISO 8601 format
                            }
                            else
                            {
                                value = DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:ss");
                            }
                        }
                        else
                        {
                            value = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss"); // ISO 8601 format
                        }
                    }
                    else if (column.DataType == typeof(TimeSpan))
                    {
                        value = ((TimeSpan)value).ToString("c"); // TimeSpan constant format
                    }
                    else if (column.DataType == typeof(byte[])) // Handle byte[] specifically
                    {
                        value = Convert.ToBase64String((byte[])value); // Encode byte[] to Base64 string
                    }
                    rowData[columnName] = value;
                    // Check for null or empty values and set default values
                    if (tableName == "Allarm")
                    {
                        if (columnName == "nr" && value == null)
                        {
                            value = 0; // Set a default value
                        }
                    }
                    if (tableName == "Operational")
                    {
                        if (columnName == "objectName" && string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            value = null; // Set a default value
                        }
                    }
                    if (columnName == "ID" && value == null)
                    {
                        value = 0;
                    }
                }
                data.Add(rowData);
            }
            return data;
        }
    }
}
