using System;
using System.Collections.Generic;
using System.Linq;
using Motorvehicle.App.Compositions;

namespace Motorvehicle.App.Tools;

/// <summary>
/// Validates weight category configurations and resolves vehicle weight categories.
/// </summary>
public static class WeightCategoryValidator
{
    #region Constants
    private const string noCategoryErr = "No recorded weight categories. Please check weight categories.";
    private const string missingValuesErr = "Missing configuration for categories.";
    private const string minWeightErr = "Minimum weight cannot be negative.";
    private const string decimalError = "Numbers with up to 2 decimal places are accepted.";
    private const string maxWeightErr = "Maximum weight must be strictly greater than minimum weight.";
    private const string dupNameErr = "Duplicate categories exist.";
    private const string zeroFloorErr = "The lowest weight category must start at 0 kg to ensure complete weight coverage.";
    private const string unboundedCeilingErr = "The highest weight category must not have a maximum weight limit.";
    private const string middleUnboundedErr = "Only the highest weight category can have an open-ended maximum weight.";
    private const string gapErr = "A weight gap exists between adjacent categories.";
    private const string overlapErr = "An overlapping weight range exists between adjacent categories.";
    private const string nonPositiveVehicleWeightErr = "Vehicle weight must be a positive value greater than 0 kg.";
    #endregion

    #region Public Methods
    /// <summary>
    /// Validates the full set of weight categories to ensure all properties are valid,
    /// names are unique, and ranges cover 0 kg to infinity with no gaps or overlaps.
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateWeightCategories(IEnumerable<WeightCategory> categories)
    {
        if (categories == null)
        {
            return (isValid: false, errorMessage: noCategoryErr);
        }

        List<WeightCategory> categoryList = categories
            .OrderBy(c => c.MinimumKg)
            .ThenBy(c => c.MaximumKg ?? decimal.MaxValue)
            .ToList();

        if (categoryList.Count == 0)
        {
            return (isValid: false, errorMessage: noCategoryErr);
        }

        foreach (WeightCategory category in categoryList)
        {
            if (!ValidateParameters(category, out string error))
            {
                return (isValid: false, errorMessage: error);
            }
        }

        if (!CheckDuplicateName(categoryList, out string duplicateCategoryName))
        {
            return (isValid: false, errorMessage: $"{dupNameErr} Name: {duplicateCategoryName}.");
        }

        if (!ValidateRangeCoverage(categoryList, out string coverageError))
        {
            return (isValid: false, errorMessage: coverageError);
        }

        return (isValid: true, errorMessage: null);
    }

    /// <summary>
    /// Identify the matching weight category for a given vehicle weight.
    /// </summary>
    public static WeightCategory GetMatchingCategory(decimal weightKg, IEnumerable<WeightCategory> categories)
    {
        if (weightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), nonPositiveVehicleWeightErr);
        }

        WeightCategory? matchedCategory = categories.FirstOrDefault(c => MatchesWeight(c, weightKg));
        if (matchedCategory == null)
        {
            throw new InvalidOperationException($"No weight category matched vehicle weight {weightKg} kg.");
        }

        return matchedCategory;
    }

    /// <summary>
    /// Returns true if a number has more than two decimal places.
    /// </summary>
    public static bool IsOverTwoDecimalPlaces(decimal number)
    {
        return decimal.Round(number, 2) != number;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Checks for duplicate category names (case-insensitive).
    /// Returns false if a duplicate exists, outputting the duplicate name.
    /// </summary>
    private static bool CheckDuplicateName(List<WeightCategory> categoryList, out string name)
    {
        IGrouping<string, WeightCategory>? duplicateName = categoryList
            .GroupBy(c => c.CategoryName.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateName != null)
        {
            name = duplicateName.Key;
            return false;
        }

        name = string.Empty;
        return true;
    }

    /// <summary>
    /// Basic validation of individual category object properties.
    /// Returns false if validation failed, with out param error message.
    /// </summary>
    private static bool ValidateParameters(WeightCategory category, out string error)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName) || string.IsNullOrWhiteSpace(category.IconIdentifier))
        {
            error = $"{missingValuesErr} Category name: '{category.CategoryName}'. Assigned icon: '{category.IconIdentifier}'.";
            return false;
        }

        string errorAppendix = $"For category name: {category.CategoryName}.";

        if (category.MinimumKg < 0)
        {
            error = $"{minWeightErr} {errorAppendix}";
            return false;
        }

        if (IsOverTwoDecimalPlaces(category.MinimumKg) ||
            (category.MaximumKg.HasValue && IsOverTwoDecimalPlaces(category.MaximumKg.Value)))
        {
            error = $"{decimalError} {errorAppendix}";
            return false;
        }

        if (category.MaximumKg.HasValue && category.MaximumKg.Value <= category.MinimumKg)
        {
            error = $"{maxWeightErr} {errorAppendix}";
            return false;
        }

        error = string.Empty;
        return true;
    }

    /// <summary>
    /// Checks that the sorted category list starts at 0 kg, has no gaps or overlaps between adjacent categories,
    /// and has an unbounded highest category.
    /// </summary>
    private static bool ValidateRangeCoverage(List<WeightCategory> categoryList, out string error)
    {
        // Rule 1: Lowest category starts at 0kg.
        if (categoryList[0].MinimumKg != 0)
        {
            error = $"{zeroFloorErr} Current lowest category '{categoryList[0].CategoryName}' starts at {categoryList[0].MinimumKg} kg.";
            return false;
        }

        // Rule 2: Highest category must be unbounded.
        if (categoryList[^1].MaximumKg.HasValue)
        {
            error = $"{unboundedCeilingErr} Current highest category '{categoryList[^1].CategoryName}' ends at {categoryList[^1].MaximumKg} kg.";
            return false;
        }

        // Rule 3: No gaps and overlaps between adjacent categories.
        for (int i = 0; i < categoryList.Count - 1; i++)
        {
            WeightCategory current = categoryList[i];
            WeightCategory next = categoryList[i + 1];

            if (!current.MaximumKg.HasValue)
            {
                error = $"{middleUnboundedErr} Category: {current.CategoryName}.";
                return false;
            }

            if (current.MaximumKg.Value < next.MinimumKg)
            {
                error = $"{gapErr} '{current.CategoryName}' ends at {current.MaximumKg} kg, but '{next.CategoryName}' starts at {next.MinimumKg} kg.";
                return false;
            }

            if (current.MaximumKg.Value > next.MinimumKg)
            {
                error = $"{overlapErr} '{current.CategoryName}' ends at {current.MaximumKg} kg, which overlaps '{next.CategoryName}' starting at {next.MinimumKg} kg.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    /// <summary>
    /// Evaluates whether a given weight falls within a category's range.
    /// </summary>
    private static bool MatchesWeight(WeightCategory category, decimal weightKg)
    {
        bool meetsLowerBound = category.MinimumKg == 0
            ? weightKg > 0
            : weightKg >= category.MinimumKg;

        bool meetsUpperBound = !category.MaximumKg.HasValue || weightKg < category.MaximumKg.Value;

        return meetsLowerBound && meetsUpperBound;
    }
    #endregion
}