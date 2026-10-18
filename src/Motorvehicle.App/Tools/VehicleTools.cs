using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Motorvehicle.App.Compositions;
using Motorvehicle.App.Data;

namespace Motorvehicle.App.Tools;

public record VehicleRecord(
    int VehicleIdentifier,
    string OwnerFirstName,
    string OwnerLastName,
    string OwnerOrganisation,
    string OwnerDisplayName,
    int ManufacturerKey,
    string ManufacturerName,
    int ManufacturedYear,
    decimal VehicleWeight,
    string CategoryName,
    string IconIdentifier
);

/// <summary>
/// Manages vehicle validation, creation, dynamic categorisation, and sorting.
/// </summary>
public class VehicleTools
{
    #region Fields and Constants
    private readonly AppDbContext _db;

    private const string firstNameRequiredErr = "Owner first name is required when no organisation is specified.";
    private const string lastNameRequiredErr = "Owner last name is required when no organisation is specified.";
    private const string manufacturerRequiredErr = "Manufacturer is required.";
    private const string manufacturerNotFoundErr = "Selected manufacturer does not exist.";
    private const string positiveWeightErr = "Vehicle weight must be a positive value greater than 0 kg.";
    private const string decimalPlacesErr = "Vehicle weight supports a maximum of two decimal places.";
    private const int earliestManufactureYear = 1886;
    #endregion

    #region Constructors
    public VehicleTools(AppDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Validates vehicle input fields. If ownerOrganisation is provided,
    /// ownerFirstName and ownerLastName are not required.
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateVehicleInput(
        string? ownerFirstName,
        string? ownerLastName,
        string? ownerOrganisation,
        int manufacturerKey,
        int manufacturedYear,
        decimal vehicleWeight)
    {
        if (!ValidateOwnerDetails(ownerFirstName, ownerLastName, ownerOrganisation, out string ownerError))
        {
            return (isValid: false, errorMessage: ownerError);
        }

        if (manufacturerKey <= 0)
        {
            return (isValid: false, errorMessage: manufacturerRequiredErr);
        }

        int maxAllowedYear = DateTime.UtcNow.Year + 1;
        if (manufacturedYear < earliestManufactureYear || manufacturedYear > maxAllowedYear)
        {
            return (
                isValid: false,
                errorMessage: $"Manufactured year must be between {earliestManufactureYear} and {maxAllowedYear}."
            );
        }

        if (vehicleWeight <= 0m)
        {
            return (isValid: false, errorMessage: positiveWeightErr);
        }

        if (WeightCategoryValidator.IsOverTwoDecimalPlaces(vehicleWeight))
        {
            return (isValid: false, errorMessage: decimalPlacesErr);
        }

        return (isValid: true, errorMessage: null);
    }

    public async Task<List<VehicleRecord>> GetVehiclesAsync(string? sortBy = "ownerName", string? sortDirection = "asc")
    {
        List<WeightCategory> categories = await _db.WeightCategories
            .AsNoTracking()
            .OrderBy(c => c.MinimumKg)
            .ToListAsync();

        List<Compositions.Motorvehicle> vehicles = await _db.Vehicles
            .AsNoTracking()
            .Include(v => v.Manufacturer)
            .ToListAsync();

        IEnumerable<VehicleRecord> records = vehicles.Select((Compositions.Motorvehicle v) =>
        {
            WeightCategory retrievedCategory = WeightCategoryValidator.GetMatchingCategory(v.VehicleWeight, categories);
            string displayName = FormatOwnerDisplayName(v.OwnerFirstName, v.OwnerLastName, v.OwnerOrganisation);

            return new VehicleRecord(
                v.VehicleIdentifier,
                v.OwnerFirstName,
                v.OwnerLastName,
                v.OwnerOrganisation,
                displayName,
                v.ManufacturerKey,
                v.Manufacturer?.ManufacturerName ?? "Unknown",
                v.ManufacturedYear,
                v.VehicleWeight,
                retrievedCategory.CategoryName,
                retrievedCategory.IconIdentifier
            );
        });

        bool isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        string normalisedSortBy = sortBy?.ToLowerInvariant() ?? "ownername";

        records = normalisedSortBy switch
        {
            "manufacturer" => isDescending
                ? records.OrderByDescending(r => r.ManufacturerName).ThenBy(r => r.OwnerDisplayName)
                : records.OrderBy(r => r.ManufacturerName).ThenBy(r => r.OwnerDisplayName),
            "year" or "manufacturedyear" => isDescending
                ? records.OrderByDescending(r => r.ManufacturedYear).ThenBy(r => r.OwnerDisplayName)
                : records.OrderBy(r => r.ManufacturedYear).ThenBy(r => r.OwnerDisplayName),
            "weight" or "vehicleweight" => isDescending
                ? records.OrderByDescending(r => r.VehicleWeight).ThenBy(r => r.OwnerDisplayName)
                : records.OrderBy(r => r.VehicleWeight).ThenBy(r => r.OwnerDisplayName),
            _ => isDescending
                ? records.OrderByDescending(r => r.OwnerDisplayName)
                : records.OrderBy(r => r.OwnerDisplayName)
        };

        return records.ToList();
    }

    public async Task<(bool success, string? error, VehicleRecord? created)> CreateVehicleAsync(
        string? ownerFirstName,
        string? ownerLastName,
        string? ownerOrganisation,
        int manufacturerKey,
        int manufacturedYear,
        decimal vehicleWeight)
    {
        (bool isValid, string? errorMessage) validation = ValidateVehicleInput(
            ownerFirstName,
            ownerLastName,
            ownerOrganisation,
            manufacturerKey,
            manufacturedYear,
            vehicleWeight);

        if (!validation.isValid)
        {
            return (success: false, error: validation.errorMessage, created: null);
        }

        Manufacturer? manufacturer = await _db.Manufacturers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ManufacturerKey == manufacturerKey);

        if (manufacturer == null)
        {
            return (success: false, error: manufacturerNotFoundErr, created: null);
        }

        List<WeightCategory> categories = await _db.WeightCategories
            .AsNoTracking()
            .OrderBy(c => c.MinimumKg)
            .ToListAsync();

        WeightCategory retrievedCategory = WeightCategoryValidator.GetMatchingCategory(vehicleWeight, categories);

        Compositions.Motorvehicle vehicle = new Compositions.Motorvehicle
        {
            OwnerFirstName = ownerFirstName?.Trim() ?? string.Empty,
            OwnerLastName = ownerLastName?.Trim() ?? string.Empty,
            OwnerOrganisation = ownerOrganisation?.Trim() ?? string.Empty,
            ManufacturerKey = manufacturerKey,
            ManufacturedYear = manufacturedYear,
            VehicleWeight = vehicleWeight
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        string displayName = FormatOwnerDisplayName(
            vehicle.OwnerFirstName,
            vehicle.OwnerLastName,
            vehicle.OwnerOrganisation);

        VehicleRecord createdRecord = new VehicleRecord(
            vehicle.VehicleIdentifier,
            vehicle.OwnerFirstName,
            vehicle.OwnerLastName,
            vehicle.OwnerOrganisation,
            displayName,
            vehicle.ManufacturerKey,
            manufacturer.ManufacturerName,
            vehicle.ManufacturedYear,
            vehicle.VehicleWeight,
            retrievedCategory.CategoryName,
            retrievedCategory.IconIdentifier
        );

        return (success: true, error: null, created: createdRecord);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates owner fields: if ownerOrganisation is populated, ownerFirstName and ownerLastName
    /// are not required; otherwise both ownerFirstName and ownerLastName are required.
    /// </summary>
    private static bool ValidateOwnerDetails(
        string? ownerFirstName,
        string? ownerLastName,
        string? ownerOrganisation,
        out string error)
    {
        bool isOrganisationOwner = !string.IsNullOrWhiteSpace(ownerOrganisation);
        if (isOrganisationOwner)
        {
            error = string.Empty;
            return true;
        }

        if (string.IsNullOrWhiteSpace(ownerFirstName))
        {
            error = firstNameRequiredErr;
            return false;
        }

        if (string.IsNullOrWhiteSpace(ownerLastName))
        {
            error = lastNameRequiredErr;
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string FormatOwnerDisplayName(
        string? ownerFirstName,
        string? ownerLastName,
        string? ownerOrganisation)
    {
        string trimmedOrg = ownerOrganisation?.Trim() ?? string.Empty;
        string fullName = $"{ownerFirstName?.Trim()} {ownerLastName?.Trim()}".Trim();

        if (!string.IsNullOrWhiteSpace(trimmedOrg) && !string.IsNullOrWhiteSpace(fullName))
        {
            return $"{trimmedOrg} ({fullName})";
        }

        if (!string.IsNullOrWhiteSpace(trimmedOrg))
        {
            return trimmedOrg;
        }

        return fullName;
    }
    #endregion
}
