using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DataHub.Core.Services
{
    public class ChangeSetService
    {
        private ConcurrentDictionary<string, UserChangeSet> _userChangeSets = new();

        public UserChangeSet GetChangeSetForUser(string userId)
        {
            return _userChangeSets.GetOrAdd(userId, _ => new UserChangeSet());
        }

        public void ClearChangeSetForUser(string userId)
        {
            if (_userChangeSets.TryGetValue(userId, out var changeSet))
            {
                changeSet.Clear();
            }
        }
    }

    public class UserChangeSet
    {
        public Dictionary<object, object> UpdatedItems { get; set; } = new();
        public HashSet<object> ItemsToDelete { get; set; } = new();

        public bool HasChanges => UpdatedItems.Any() || ItemsToDelete.Any();

        public void AddOrUpdateItem(object id, object item)
        {
            if (ItemsToDelete.Contains(id))
            {
                ItemsToDelete.Remove(id);
            }
            UpdatedItems[id] = item;
        }

        public void MarkForDeletion(object id)
        {
            if (UpdatedItems.ContainsKey(id))
            {
                UpdatedItems.Remove(id);
            }
            ItemsToDelete.Add(id);
        }

        public void RemoveFromChangeSet(object id)
        {
            UpdatedItems.Remove(id);
            ItemsToDelete.Remove(id);
        }

        public void Clear()
        {
            UpdatedItems.Clear();
            ItemsToDelete.Clear();
        }
    }
}
