using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Motorvehicle.App.Compositions;
using Motorvehicle.App.Data;
using Motorvehicle.App.Tools;
using Xunit;

namespace Motorvehicle.Tests;

public class VehicleAndCategoryTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        AppDbContext db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Theory]
    [InlineData(0.01, "Light")]
    [InlineData(499.99, "Light")]
    [InlineData(500.00, "Medium")]
    [InlineData(1850.75, "Medium")]
    [InlineData(2499.99, "Medium")]
    [InlineData(2500.00, "Heavy")]
    [InlineData(9500.00, "Heavy")]
    public void GetMatchingCategory_HandlesBoundariesCorrectly(decimal vehicleWeight, string expectedCategory)
    {
        using AppDbContext db = CreateInMemoryDb();
        List<WeightCategory> categories = db.WeightCategories.OrderBy(c => c.MinimumKg).ToList();

        WeightCategory retrievedCategory = WeightCategoryValidator.GetMatchingCategory(vehicleWeight, categories);

        Assert.Equal(expectedCategory, retrievedCategory.CategoryName);
    }

    [Fact]
    public void ValidateWeightCategories_RejectsGapsBetweenCategories()
    {
        List<WeightCategory> categoriesWithGap = new List<WeightCategory>
        {
            new WeightCategory { CategoryName = "Light", MinimumKg = 0m, MaximumKg = 500m, IconIdentifier = "car" },
            new WeightCategory { CategoryName = "Medium", MinimumKg = 600m, MaximumKg = 2500m, IconIdentifier = "suv" },
            new WeightCategory { CategoryName = "Heavy", MinimumKg = 2500m, MaximumKg = null, IconIdentifier = "truck" }
        };

        (bool isValid, string? errorMessage) result = WeightCategoryValidator.ValidateWeightCategories(categoriesWithGap);

        Assert.False(result.isValid);
        Assert.Contains("gap", result.errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateWeightCategories_RejectsOverlappingCategories()
    {
        List<WeightCategory> categoriesWithOverlap = new List<WeightCategory>
        {
            new WeightCategory { CategoryName = "Light", MinimumKg = 0m, MaximumKg = 600m, IconIdentifier = "car" },
            new WeightCategory { CategoryName = "Medium", MinimumKg = 500m, MaximumKg = 2500m, IconIdentifier = "suv" },
            new WeightCategory { CategoryName = "Heavy", MinimumKg = 2500m, MaximumKg = null, IconIdentifier = "truck" }
        };

        (bool isValid, string? errorMessage) result = WeightCategoryValidator.ValidateWeightCategories(categoriesWithOverlap);

        Assert.False(result.isValid);
        Assert.Contains("overlapping", result.errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CategoryUpdate_AutomaticallyUpdatesExistingVehicleCategory_AndAdjustsNeighbour()
    {
        using AppDbContext db = CreateInMemoryDb();
        VehicleTools vehicleTools = new VehicleTools(db);
        CategoryTools categoryTools = new CategoryTools(db);

        await vehicleTools.CreateVehicleAsync(
            ownerFirstName: "Jane",
            ownerLastName: "Turei",
            ownerOrganisation: "",
            manufacturerKey: 2,
            manufacturedYear: 2015,
            vehicleWeight: 2200m);

        List<VehicleRecord> initialList = await vehicleTools.GetVehiclesAsync();
        Assert.Equal("Medium", initialList.Single().CategoryName);

        (bool success, string? error, WeightCategory? updated) updateResult =
            await categoryTools.UpdateCategoryAsync(2, "Medium", "suv", 500m, 2000m);

        Assert.True(updateResult.success, updateResult.error);

        List<VehicleRecord> updatedList = await vehicleTools.GetVehiclesAsync();
        Assert.Equal("Heavy", updatedList.Single().CategoryName);
    }

    [Fact]
    public async Task IndividualCategoryDelete_HealsGapWithNeighbour_AndPreventsDeletingLastCategory()
    {
        using AppDbContext db = CreateInMemoryDb();
        CategoryTools categoryTools = new CategoryTools(db);

        (bool success, string? error) deleteMediumResult = await categoryTools.DeleteCategoryAsync(2);
        Assert.True(deleteMediumResult.success, deleteMediumResult.error);

        List<WeightCategory> remaining = await categoryTools.GetAllOrderedAsync();
        Assert.Equal(2, remaining.Count);
        Assert.Equal(2500m, remaining[0].MaximumKg);
        Assert.Equal(2500m, remaining[1].MinimumKg);

        await categoryTools.DeleteCategoryAsync(1);

        (bool success, string? error) deleteLastResult = await categoryTools.DeleteCategoryAsync(3);
        Assert.False(deleteLastResult.success);
        Assert.Contains("Cannot delete the last remaining category", deleteLastResult.error);
    }

    [Fact]
    public void ValidateVehicleInput_AllowsBlankFirstAndLastName_WhenOwnerOrganisationIsProvided()
    {
        (bool isValid, string? errorMessage) orgResult = VehicleTools.ValidateVehicleInput(
            ownerFirstName: "",
            ownerLastName: "",
            ownerOrganisation: "CreditWorks NZ Ltd",
            manufacturerKey: 1,
            manufacturedYear: 2022,
            vehicleWeight: 1500.50m);

        Assert.True(orgResult.isValid, orgResult.errorMessage);

        (bool isValid, string? errorMessage) individualMissingLastResult = VehicleTools.ValidateVehicleInput(
            ownerFirstName: "John",
            ownerLastName: "",
            ownerOrganisation: "",
            manufacturerKey: 1,
            manufacturedYear: 2022,
            vehicleWeight: 1500.50m);

        Assert.False(individualMissingLastResult.isValid);
        Assert.Contains("last name is required", individualMissingLastResult.errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-150.5)]
    [InlineData(1850.755)]
    public void ValidateVehicleInput_RejectsInvalidWeights(decimal invalidWeight)
    {
        (bool isValid, string? errorMessage) result = VehicleTools.ValidateVehicleInput(
            ownerFirstName: "John",
            ownerLastName: "Smith",
            ownerOrganisation: "",
            manufacturerKey: 1,
            manufacturedYear: 2020,
            vehicleWeight: invalidWeight);

        Assert.False(result.isValid);
    }
}
