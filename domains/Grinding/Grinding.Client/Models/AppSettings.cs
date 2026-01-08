// c:\NON_Install_Programs\REST\RPK_TestServer_V2_900\Models\AppSettings.cs
using System;
using System.Collections.Generic;
 
namespace RPK_BlazorClient.Models
{
    public class AppSettings
    {
        public string TableConfigFilePath { get; set; }
        public string LastSentClientDbIdsFilePath { get; set; }
        public Dictionary<string, int> PredefinedClientDbIds { get; set; } = new Dictionary<string, int>();
        public string MdbFilePath { get; set; }
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int BatchSize { get; set; }
        public int SuspendTimeSeconds { get; set; }
        public string ClientId { get; set; }
    }
}
