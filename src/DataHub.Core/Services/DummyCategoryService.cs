using DataHub.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataHub.Core.Services
{
    /// <summary>
    /// In-memory service for DummyCategory lookup table
    /// </summary>
    public class DummyCategoryService
    {
        private readonly List<DummyCategory> _categories;

        public DummyCategoryService()
        {
            _categories = new List<DummyCategory>
            {
                new DummyCategory { Id = 1, Name = "Electronics", Description = "Electronic devices and components", ColorCode = "#3B82F6" },
                new DummyCategory { Id = 2, Name = "Office Supplies", Description = "Office equipment and stationery", ColorCode = "#10B981" },
                new DummyCategory { Id = 3, Name = "Furniture", Description = "Office and home furniture", ColorCode = "#F59E0B" },
                new DummyCategory { Id = 4, Name = "Tools", Description = "Hand and power tools", ColorCode = "#EF4444" },
                new DummyCategory { Id = 5, Name = "Software", Description = "Software licenses and subscriptions", ColorCode = "#8B 5CF6" }
            };
        }

        public List<DummyCategory> GetAll() => _categories.ToList();
        public DummyCategory? GetById(int id) => _categories.FirstOrDefault(c => c.Id == id);
    }
}
