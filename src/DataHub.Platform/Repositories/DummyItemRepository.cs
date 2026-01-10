using DataHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories
{
    public class DummyItemRepository : IDummyItemRepository
    {
        private static readonly List<DummyItem> _dummyItems = new();
        private static int _nextId = 1;
        private static readonly object _lock = new();

        // Static constructor to seed in-memory data
        static DummyItemRepository()
        {
            SeedData();
        }

        private static void SeedData()
        {
            lock (_lock)
            {
                if (_dummyItems.Any()) return; // Seed only once

                var random = new Random(42); // Fixed seed for consistency

                for (int i = 1; i <= 250; i++)
                {
                    _dummyItems.Add(new DummyItem
                    {
                        Id = _nextId++,
                        Name = $"Dummy Item {i}",
                        Description = $"Repository test item #{i}",
                        CategoryId = random.Next(1, 6), // Random category 1-5
                        StatusId = random.Next(1, 7), // Random status 1-6
                        CreatedDate = DateTime.UtcNow.AddDays(-random.Next(365)),
                        IsActive = random.Next(10) > 2, // ~80% active
                        Value = (decimal)(random.NextDouble() * 10000)
                    });
                }
                Console.WriteLine($"Seeded {_dummyItems.Count} dummy items into repository.");
            }
        }

        public IQueryable<DummyItem> GetDummyItems()
        {
            return _dummyItems.AsQueryable();
        }

        public Task<DummyItem?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                var item = _dummyItems.FirstOrDefault(i => i.Id == id);
                return Task.FromResult(item);
            }
        }

        public Task<DummyItem> AddAsync(DummyItem item)
        {
            lock (_lock)
            {
                item.Id = _nextId++;
                item.CreatedDate = DateTime.UtcNow;
                _dummyItems.Add(item);
                return Task.FromResult(item);
            }
        }

        public Task<DummyItem?> UpdateAsync(DummyItem item)
        {
            lock (_lock)
            {
                var existingItem = _dummyItems.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.Name = item.Name;
                    existingItem.Description = item.Description;
                    existingItem.CategoryId = item.CategoryId;
                    existingItem.StatusId = item.StatusId;
                    existingItem.IsActive = item.IsActive;
                    existingItem.Value = item.Value;
                    existingItem.CreatedDate = item.CreatedDate;
                    return Task.FromResult<DummyItem?>(existingItem);
                }
                return Task.FromResult<DummyItem?>(null);
            }
        }

        public Task<bool> DeleteAsync(int id)
        {
            lock (_lock)
            {
                var itemToRemove = _dummyItems.FirstOrDefault(i => i.Id == id);
                if (itemToRemove != null)
                {
                    _dummyItems.Remove(itemToRemove);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }
    }
}
