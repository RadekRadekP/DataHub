using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RPK_BlazorClient.Models;

namespace RPK_BlazorClient
{
    public static class ApiConnector
    {
        public static async Task Login(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            using var client = CreateHttpClient(serviceProvider);
            var credentials = $"{settings.Username}:{settings.Password}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            var response = await client.PostAsync($"{settings.ServerUrl}/api/data/Login", null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Login successful!");
            }
            else
            {
                Console.WriteLine($"Login failed: {(int)response.StatusCode} {response.ReasonPhrase}");
            }
        }

        // Changed return type to Task<bool>
        public static async Task<bool> PostBatchData(IServiceProvider serviceProvider, string value, string clientId, string tableName)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            using var client = CreateHttpClient(serviceProvider);
            client.DefaultRequestHeaders.Authorization = GetBasicAuthHeader(settings.Username, settings.Password);
            Console.WriteLine($"[AUTH] Using Basic Authentication: {client.DefaultRequestHeaders.Authorization.Parameter}");
            
            // The 'value' parameter is already a JSON string representing a list of items.
            // We need to wrap this list in an object that matches the server's DTO structure.
            // e.g., for Allarms, the server expects {"Allarms": [item1, item2,...]}
            string wrappedValue = "";
            string endpointPath = "";

            switch (tableName)
            {
                case "Grinding":
                    // Assuming GrindingListRestPostRequestDTO has a "Grindings" property
                    wrappedValue = $"{{\"Grindings\": {value}}}"; 
                    endpointPath = "grinding/batch";
                    break;
                case "Allarm":
                    // Assuming AllarmListRestPostRequestDTO has an "Allarms" property
                    wrappedValue = $"{{\"Allarms\": {value}}}"; 
                    endpointPath = "allarm/batch";
                    break;
                case "Operational":
                    // Assuming OperationalListRestPostRequestDTO has an "Operationals" property
                    wrappedValue = $"{{\"Operationals\": {value}}}"; 
                    endpointPath = "operational/batch";
                    break;
                default:
                    Console.WriteLine($"[POST BATCH] Error posting data: Unknown table name: {tableName}");
                    return false; // Indicate failure
            }

            var content = new StringContent(wrappedValue, Encoding.UTF8, "application/json");
            Console.WriteLine($"[POST BATCH] Sending data: {wrappedValue}");
            try
            {
                // Construct the correct URL based on server controller routes
                string url = $"{settings.ServerUrl}/api/{endpointPath}";
                Console.WriteLine($"[POST BATCH] Target URL: {url}");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[POST BATCH] Data posted successfully. Status Code: {response.StatusCode}");
                    return true; // Indicate success
                }
                else
                {
                    Console.WriteLine($"[POST BATCH] Error posting data: Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[POST BATCH] Response content: {responseContent}");
                    return false; // Indicate failure
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[POST BATCH] Error posting data: {ex.Message}");
                return false; // Indicate failure
            }
        }

        public static async Task GetDataFromServer(IServiceProvider serviceProvider, string tableName)
        {
            var settings = serviceProvider.GetRequiredService<AppSettings>();
            using var client = CreateHttpClient(serviceProvider);
            client.DefaultRequestHeaders.Authorization = GetBasicAuthHeader(settings.Username, settings.Password);
            
            string endpointPath = "";
            switch (tableName.ToLowerInvariant()) // Use ToLowerInvariant for case-insensitive matching
            {
                case "grinding":
                    endpointPath = "grinding";
                    break;
                case "allarm":
                    endpointPath = "allarm";
                    break;
                case "operational":
                    endpointPath = "operational";
                    break;
                default:
                    Console.WriteLine($"[GET DATA] Error: Unknown table name for GET request: {tableName}");
                    return;
            }
            try
            {
                var response = await client.GetAsync($"{settings.ServerUrl}/api/{endpointPath}");
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Data for table '{tableName}':");
                    Console.WriteLine(responseContent);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"No data found for table '{tableName}'.");
                }
                else
                {
                    Console.WriteLine($"Error getting data: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static AuthenticationHeaderValue GetBasicAuthHeader(string username, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public static HttpClient CreateHttpClient(IServiceProvider serviceProvider)
        {
            var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return clientFactory.CreateClient("InsecureClient");
        }
    }
}
