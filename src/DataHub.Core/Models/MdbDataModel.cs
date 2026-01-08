using System;
using System.Collections.Generic;

namespace DataHub.Core.Models
{
    public class MdbDataModel
    {
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>(); // Initialize with default
    }
    
}
