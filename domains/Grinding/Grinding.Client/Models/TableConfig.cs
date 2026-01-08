using System.Collections.Generic;

namespace RPK_BlazorClient.Models
{
    public class TableConfig
    {
        public string TableName { get; set; }
        public List<string> Columns { get; set; }
        public string ClientDbIdColumn { get; set; } // Add this property
    }
}
