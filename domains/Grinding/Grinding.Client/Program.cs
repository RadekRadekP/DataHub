using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RPK_BlazorClient.Models;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RPK_BlazorClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("CLI client for interacting with the RPK Test Server");
            var interactiveCommand = new Command("interactive", "Starts an interactive session with the server");
            var clientIdOption = new Option<string>("--clientid", "The unique identifier for this client.") { IsRequired = true };
            interactiveCommand.AddOption(clientIdOption);
            interactiveCommand.SetHandler(async (string clientId) => await RunInteractiveSession(ConfigureServices(clientId), clientId), clientIdOption);
            rootCommand.AddCommand(interactiveCommand);
            var loginCommand = new Command("login", "Logs in to the server");
            loginCommand.SetHandler(async () => await ApiConnector.Login(ConfigureServices("")));
            rootCommand.AddCommand(loginCommand);
            var printMdbStructureCommand = new Command("printmdbstructure", "Prints the structure of all tables in the MDB file");
            printMdbStructureCommand.SetHandler(async () => await MdbReader.PrintMdbStructure(ConfigureServices("")));
            rootCommand.AddCommand(printMdbStructureCommand);
            var resetClientDbIdsCommand = new Command("resetclientdbids", "Resets all ClientDbIds to the predefined values");
            resetClientDbIdsCommand.SetHandler(async () => await MdbReader.ResetClientDbIds(ConfigureServices("")));
            rootCommand.AddCommand(resetClientDbIdsCommand);
            var printConfigTablesCommand = new Command("printconfigtables", "Prints the configured tables");
            printConfigTablesCommand.SetHandler(async () => await MdbReader.PrintConfiguredTables(ConfigureServices("")));
            rootCommand.AddCommand(printConfigTablesCommand);

            // New command to send MDB data
            var sendMdbDataCommand = new Command("sendmdbdata", "Sends data from the MDB to the server");
            sendMdbDataCommand.AddOption(clientIdOption);
            sendMdbDataCommand.SetHandler(async (string clientId) => await SendMdbDataToServer(ConfigureServices(clientId), clientId), clientIdOption);
            rootCommand.AddCommand(sendMdbDataCommand);

            var getDataCommand = new Command("getdata", "Gets data from the server");
            var tableNameArgument = new Argument<string>("tableName", "The name of the table to get data from.");
            getDataCommand.AddArgument(tableNameArgument);
            getDataCommand.SetHandler(async (string tableName) => await GetDataFromServer(ConfigureServices(""), tableName), tableNameArgument);
            rootCommand.AddCommand(getDataCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static IServiceProvider ConfigureServices(string clientId)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var appSettings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(appSettings);

            // Correctly bind PredefinedClientDbIds
            var predefinedClientDbIds = new Dictionary<string, int>();
            configuration.GetSection("PredefinedClientDbIds").Bind(predefinedClientDbIds);
            appSettings.PredefinedClientDbIds = predefinedClientDbIds;

            appSettings.ClientId = clientId; // Add this line

            return new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<AppSettings>(appSettings)
                .AddHttpClient("InsecureClient")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true, AllowAutoRedirect = true, PreAuthenticate = true })
                .Services
                .BuildServiceProvider();
        }

        static async Task RunInteractiveSession(IServiceProvider serviceProvider, string clientId)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            Console.WriteLine($"Starting interactive session for client: {clientId}. Type 'login', 'printmdbr', 'printmdbstructure', 'resetclientdbids', 'sendmdbdata', 'getdata' or 'exit'.");
            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Exiting interactive session.");
                    break;
                }
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();
                try
                {
                    switch (command)
                    {
                        case "login":
                            await ApiConnector.Login(serviceProvider);
                            break;
                        case "printmdbr":
                            if (OperatingSystem.IsWindows())
                            {
                                await MdbReader.PrintConfiguredTables(serviceProvider);
                            }
                            else
                            {
                                Console.WriteLine("Error: Reading MDB files is only supported on Windows.");
                            }
                            break;
                        case "printmdbstructure":
                            if (OperatingSystem.IsWindows())
                            {
                                await MdbReader.PrintMdbStructure(serviceProvider);
                            }
                            else
                            {
                                Console.WriteLine("Error: Reading MDB files is only supported on Windows.");
                            }
                            break;
                        case "resetclientdbids":
                            if (OperatingSystem.IsWindows())
                            {
                                await MdbReader.ResetClientDbIds(serviceProvider);
                            }
                            else
                            {
                                Console.WriteLine("Error: Reading MDB files is only supported on Windows.");
                            }
                            break;
                        case "sendmdbdata":
                            if (OperatingSystem.IsWindows())
                            {
                                await SendMdbDataToServer(serviceProvider, clientId);
                            }
                            else
                            {
                                Console.WriteLine("Error: Reading MDB files is only supported on Windows.");
                            }
                            break;
                        case "getdata":
                            if (parts.Length > 1)
                            {
                                string tableName = parts[1];
                                await ApiConnector.GetDataFromServer(serviceProvider, tableName);
                            }
                            else
                            {
                                Console.WriteLine("Usage: getdata <tableName>");
                            }
                            break;
                        case "printconfigtables":
                            if (OperatingSystem.IsWindows())
                            {
                                await MdbReader.PrintConfiguredTables(serviceProvider);
                            }
                            else
                            {
                                Console.WriteLine("Error: Reading MDB files is only supported on Windows.");
                            }
                            break;
                        default:
                            Console.WriteLine($"Unknown command: {command}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
        // New method to send MDB data to the server
        static async Task SendMdbDataToServer(IServiceProvider serviceProvider, string clientId)
        {
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("Error: Reading MDB files is only supported on Windows.");
                return;
            }

            try
            {
                var appSettings = serviceProvider.GetRequiredService<AppSettings>();
                var tableConfigs = MdbReader.LoadTableConfigs(appSettings.TableConfigFilePath);

                Console.WriteLine("Starting to send MDB data...");
                using var connection = new System.Data.OleDb.OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={appSettings.MdbFilePath};");
                connection.Open();
                foreach (var tableConfig in tableConfigs)
                {
                    if (MdbReader.TableExists(connection, tableConfig.TableName))
                    {
                        Console.WriteLine($"\n--- Processing table: {tableConfig.TableName} ---");
                        await MdbReader.ReadAndSendConfiguredTable(tableConfig, connection, serviceProvider, clientId);
                    }
                    else
                    {
                        Console.WriteLine($"Table '{tableConfig.TableName}' not found in the MDB file.");
                    }
                }
                Console.WriteLine("All MDB data sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending MDB data: {ex.Message}");
            }
        }

        static async Task GetDataFromServer(IServiceProvider serviceProvider, string tableName)
        {
            await ApiConnector.GetDataFromServer(serviceProvider, tableName);
        }
    }
}
