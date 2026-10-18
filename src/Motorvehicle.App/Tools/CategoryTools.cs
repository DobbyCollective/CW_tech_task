using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Motorvehicle.App.Compositions;
using Motorvehicle.App.Data;

namespace Motorvehicle.App.Tools;

/// <summary>
/// Manages individual create/update/delete (CRUD) actions for weight categories while automatically
/// adjusting adjacent category boundaries to prevent gaps or overlaps.
/// </summary>
public class CategoryTools
{
    #region Fields and Constants
    private readonly AppDbContext _db;

    private const string lastCategoryDeleteErr = "Cannot delete the last remaining category because all vehicle weights must map to a category.";
    private const string lowestStartZeroErr = "The lowest category must always start at 0 kg.";
    private const string highestUnboundedErr = "The highest category must remain unbounded (no maximum weight).";
    private const string nonTopBoundedErr = "Only the highest category can have an empty maximum weight.";
    private const string boundedFitErr = "A new bounded category must fit within an existing category's range so it can cleanly split the boundary without overlapping multiple categories.";
    #endregion

    #region Constructors
    public CategoryTools(AppDbContext db)
    {
        _db = db;
    }
    #endregion
    
    #region Public Methods
    public async Task<List<WeightCategory>> GetAllOrderedAsync()
    {
        List<WeightCategory> categories = await _db.WeightCategories
            .AsNoTracking()
            .OrderBy(c => c.MinimumKg)
            .ToListAsync();

        return categories;
    }

    /// <summary>
    /// Creates a new category by splitting an existing category's range or appending a new top category,
    /// automatically adjusting adjacent boundaries so no gap or overlap can occur.
    /// </summary>
    public async Task<(bool success, string? error, WeightCategory? created)> CreateCategoryAsync(
        string categoryName,
        string iconIdentifier,
        decimal minimumKg,
        decimal? maximumKg)
    {
        List<WeightCategory> categories = await _db.WeightCategories
            .OrderBy(c => c.MinimumKg)
            .ToListAsync();

        WeightCategory newCategory = new WeightCategory
        {
            CategoryName = categoryName?.Trim() ?? string.Empty,
            IconIdentifier = iconIdentifier?.Trim() ?? string.Empty,
            MinimumKg = minimumKg,
            MaximumKg = maximumKg
        };

        if (!maximumKg.HasValue)
        {
            WeightCategory? currentTop = categories.LastOrDefault();
            if (currentTop == null)
            {
                newCategory.MinimumKg = 0m;
            }
            else
            {
                if (minimumKg <= currentTop.MinimumKg)
                {
                    return (
                        success: false,
                        error: $"New unbounded category must start above the current top category's minimum ({currentTop.MinimumKg} kg).",
                        created: null
                    );
                }

                currentTop.MaximumKg = minimumKg;
            }

            categories.Add(newCategory);
        }
        else
        {
            WeightCategory? hostCategory = categories.FirstOrDefault(c =>
                c.MinimumKg <= minimumKg &&
                (!c.MaximumKg.HasValue || c.MaximumKg.Value >= maximumKg.Value));

            if (hostCategory == null)
            {
                return (success: false, error: boundedFitErr, created: null);
            }

            if (hostCategory.MinimumKg == minimumKg && hostCategory.MaximumKg != maximumKg)
            {
                hostCategory.MinimumKg = maximumKg.Value;
                categories.Add(newCategory);
            }
            else if (hostCategory.MaximumKg == maximumKg && hostCategory.MinimumKg != minimumKg)
            {
                hostCategory.MaximumKg = minimumKg;
                categories.Add(newCategory);
            }
            else
            {
                string maxLabel = hostCategory.MaximumKg.HasValue ? hostCategory.MaximumKg.Value.ToString("0.##") : "∞";
                return (
                    success: false,
                    error: $"To insert '{newCategory.CategoryName}' without gaps, its minimum ({minimumKg} kg) or maximum ({maximumKg} kg) must align with an existing boundary of '{hostCategory.CategoryName}' ({hostCategory.MinimumKg}–{maxLabel} kg).",
                    created: null
                );
            }
        }

        (bool isValid, string? errorMessage) validation = WeightCategoryValidator.ValidateWeightCategories(categories);
        if (!validation.isValid)
        {
            return (success: false, error: validation.errorMessage, created: null);
        }

        _db.WeightCategories.Add(newCategory);
        await _db.SaveChangesAsync();

        return (success: true, error: null, created: newCategory);
    }

    /// <summary>
    /// Updates an existing category by WeightKey and automatically adjusts the shared boundary of the
    /// adjacent lower/upper neighbour so contiguous coverage is maintained.
    /// </summary>
    public async Task<(bool success, string? error, WeightCategory? updated)> UpdateCategoryAsync(
        int weightKey,
        string categoryName,
        string iconIdentifier,
        decimal minimumKg,
        decimal? maximumKg)
    {
        List<WeightCategory> categories = await _db.WeightCategories
            .OrderBy(c => c.MinimumKg)
            .ToListAsync();

        int index = categories.FindIndex(c => c.WeightKey == weightKey);
        if (index == -1)
        {
            return (success: false, error: $"Category with key {weightKey} was not found.", updated: null);
        }

        WeightCategory targetCategory = categories[index];
        targetCategory.CategoryName = categoryName?.Trim() ?? string.Empty;
        targetCategory.IconIdentifier = iconIdentifier?.Trim() ?? string.Empty;

        if (index == 0 && minimumKg != 0m)
        {
            return (success: false, error: lowestStartZeroErr, updated: null);
        }

        if (index == categories.Count - 1 && maximumKg.HasValue)
        {
            return (success: false, error: highestUnboundedErr, updated: null);
        }

        if (index > 0 && targetCategory.MinimumKg != minimumKg)
        {
            categories[index - 1].MaximumKg = minimumKg;
            targetCategory.MinimumKg = minimumKg;
        }

        if (index < categories.Count - 1 && targetCategory.MaximumKg != maximumKg)
        {
            if (!maximumKg.HasValue)
            {
                return (success: false, error: $"{nonTopBoundedErr} Category: '{targetCategory.CategoryName}'.", updated: null);
            }

            categories[index + 1].MinimumKg = maximumKg.Value;
            targetCategory.MaximumKg = maximumKg.Value;
        }

        (bool isValid, string? errorMessage) validation = WeightCategoryValidator.ValidateWeightCategories(categories);
        if (!validation.isValid)
        {
            return (success: false, error: validation.errorMessage, updated: null);
        }

        await _db.SaveChangesAsync();
        return (success: true, error: null, updated: targetCategory);
    }

    /// <summary>
    /// Deletes a category by WeightKey and merges its weight range into the adjacent neighbour
    /// so no weight gap is ever created.
    /// </summary>
    public async Task<(bool success, string? error)> DeleteCategoryAsync(int weightKey)
    {
        List<WeightCategory> categories = await _db.WeightCategories
            .OrderBy(c => c.MinimumKg)
            .ToListAsync();

        if (categories.Count <= 1)
        {
            return (success: false, error: lastCategoryDeleteErr);
        }

        int index = categories.FindIndex(c => c.WeightKey == weightKey);
        if (index == -1)
        {
            return (success: false, error: $"Category with key {weightKey} was not found.");
        }

        WeightCategory categoryToRemove = categories[index];

        if (index == 0)
        {
            categories[1].MinimumKg = 0m;
        }
        else
        {
            categories[index - 1].MaximumKg = categoryToRemove.MaximumKg;
        }

        categories.RemoveAt(index);

        (bool isValid, string? errorMessage) validation = WeightCategoryValidator.ValidateWeightCategories(categories);
        if (!validation.isValid)
        {
            return (success: false, error: validation.errorMessage);
        }

        _db.WeightCategories.Remove(categoryToRemove);
        await _db.SaveChangesAsync();

        return (success: true, error: null);
    }
    #endregion
}
