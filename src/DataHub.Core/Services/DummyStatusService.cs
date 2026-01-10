using DataHub.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataHub.Core.Services
{
    /// <summary>
    /// In-memory service for DummyStatus lookup table
    /// </summary>
    public class DummyStatusService
    {
        private readonly List<DummyStatus> _statuses;

        public DummyStatusService()
        {
            _statuses = new List<DummyStatus>
            {
                new DummyStatus { Id = 1, Name = "Draft", Description = "New item, not yet reviewed", DisplayOrder = 1, IsTerminal = false },
                new DummyStatus { Id = 2, Name = "Pending Review", Description = "Awaiting approval", DisplayOrder = 2, IsTerminal = false },
                new DummyStatus { Id = 3, Name = "Approved", Description = "Approved and active", DisplayOrder = 3, IsTerminal = false },
                new DummyStatus { Id = 4, Name = "In Progress", Description = "Currently being processed", DisplayOrder = 4, IsTerminal = false },
                new DummyStatus { Id = 5, Name = "Completed", Description = "Successfully completed", DisplayOrder = 5, IsTerminal = true },
                new DummyStatus { Id = 6, Name = "Cancelled", Description = "Cancelled or rejected", DisplayOrder = 6, IsTerminal = true }
            };
        }

        public List<DummyStatus> GetAll() => _statuses.ToList();
        public DummyStatus? GetById(int id) => _statuses.FirstOrDefault(s => s.Id == id);
    }
}
