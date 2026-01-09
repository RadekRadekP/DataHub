using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DataHub.Core.Data;
using DataHub.Core.Models;
using DataHub.Core.Models.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DataHub.Core.Services
{
    public class UserPreferenceService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserPreferenceService> _logger; // Inject logger

        public UserPreferenceService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IHttpContextAccessor httpContextAccessor, ILogger<UserPreferenceService> logger) // Add logger to constructor
        {
            _dbContextFactory = dbContextFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger; // Assign logger
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
        }

        public async Task<List<string>> ListSavedCriteriaNamesAsync(string tableName)
        {
            var userId = GetCurrentUserId();
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.UserSavedCriteria
                                .Where(c => c.UserId == userId && c.TableName == tableName)
                                .Select(c => c.CriteriaName)
                                .OrderBy(name => name)
                                .ToListAsync();
        }

        public async Task<SavedCriteria?> LoadCriteriaAsync(string tableName, string criteriaName)
        {
            var userId = GetCurrentUserId();
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var savedCriteriaEntity = await context.UserSavedCriteria
                                             .FirstOrDefaultAsync(c => c.UserId == userId && c.TableName == tableName && c.CriteriaName == criteriaName);

            if (savedCriteriaEntity == null)
            {
                return null;
            }

            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var loadedCriteria = JsonSerializer.Deserialize<SavedCriteria>(savedCriteriaEntity.CriteriaJson, jsonSerializerOptions);
            if (loadedCriteria != null)
            {
                loadedCriteria.RawQuery = savedCriteriaEntity.RawQuery ?? string.Empty;
            }
            return loadedCriteria;
        }

        public async Task<bool> SaveCriteriaAsync(string tableName, string criteriaName, List<FilterCriterion> filters, List<SortCriterion> sorts, string? rawQuery = null)
        {
            _logger.LogInformation("DEBUG: UserPreferenceService.SaveCriteriaAsync - TableName: {TableName}, CriteriaName: {CriteriaName}, RawQuery: {RawQuery}", tableName, criteriaName, rawQuery);
            _logger.LogInformation("DEBUG: UserPreferenceService.SaveCriteriaAsync - Filters count: {FiltersCount}, Sorts count: {SortsCount}", filters?.Count ?? 0, sorts?.Count ?? 0);

            var userId = GetCurrentUserId();
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var existingCriteria = await context.UserSavedCriteria
                                                .FirstOrDefaultAsync(c => c.UserId == userId && c.TableName == tableName && c.CriteriaName == criteriaName);

            var criteriaToSerialize = new SavedCriteria
            {
                Name = criteriaName,
                Filters = filters ?? new(),
                Sorts = sorts ?? new()
            };

            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = false };
            string criteriaJson = JsonSerializer.Serialize(criteriaToSerialize, jsonSerializerOptions);

            if (existingCriteria != null)
            {
                // Update existing
                existingCriteria.CriteriaJson = criteriaJson;
                existingCriteria.RawQuery = rawQuery; // Update raw query
                context.UserSavedCriteria.Update(existingCriteria);
            }
            else
            {
                // Create new
                var newCriteria = new UserSavedCriteria
                {
                    UserId = userId,
                    TableName = tableName,
                    CriteriaName = criteriaName,
                    CriteriaJson = criteriaJson,
                };
                if (rawQuery is not null) newCriteria.RawQuery = rawQuery;
            }

            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("DEBUG: UserPreferenceService.SaveCriteriaAsync - Successfully saved criteria {CriteriaName}.", criteriaName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR: UserPreferenceService.SaveCriteriaAsync - Failed to save criteria {CriteriaName}.", criteriaName);
                return false;
            }
        }

        public async Task<bool> DeleteCriteriaAsync(string tableName, string criteriaName)
        {
            var userId = GetCurrentUserId();
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var criteriaToDelete = await context.UserSavedCriteria
                                                .FirstOrDefaultAsync(c => c.UserId == userId && c.TableName == tableName && c.CriteriaName == criteriaName);

            if (criteriaToDelete != null)
            {
                context.UserSavedCriteria.Remove(criteriaToDelete);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}