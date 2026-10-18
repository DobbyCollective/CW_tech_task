using Motorvehicle.App.Compositions;
using Microsoft.EntityFrameworkCore;

namespace Motorvehicle.App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<WeightCategory> WeightCategories => Set<WeightCategory>();
    public DbSet<Compositions.Motorvehicle> Vehicles => Set<Compositions.Motorvehicle>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Manufacturer>(entity =>
        {
            entity.ToTable("config_Manufacturers", schema: "config");
            entity.HasKey(m => m.ManufacturerKey);
            entity.Property(m => m.ManufacturerName).IsRequired().HasMaxLength(50);
            entity.HasIndex(m => m.ManufacturerName).IsUnique();
            entity.HasData(
                new Manufacturer { ManufacturerKey = 1, ManufacturerName = "Mazda" },
                new Manufacturer { ManufacturerKey = 2, ManufacturerName = "Mercedes" },
                new Manufacturer { ManufacturerKey = 3, ManufacturerName = "Honda" },
                new Manufacturer { ManufacturerKey = 4, ManufacturerName = "Ferrari" },
                new Manufacturer { ManufacturerKey = 5, ManufacturerName = "Toyota" }
            );
        });

        mb.Entity<WeightCategory>(entity =>
        {
            entity.ToTable("config_WeightCategories", schema: "config");
            entity.HasKey(c => c.WeightKey);
            entity.Property(c => c.CategoryName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.IconIdentifier).IsRequired().HasMaxLength(50);
            entity.Property(c => c.MinimumKg).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(c => c.MaximumKg).HasColumnType("decimal(10,2)");
            entity.HasData(
                new WeightCategory { WeightKey = 1, CategoryName = "Light", MinimumKg = 0m, MaximumKg = 500m, IconIdentifier = "car" },
                new WeightCategory { WeightKey = 2, CategoryName = "Medium", MinimumKg = 500m, MaximumKg = 2500m, IconIdentifier = "suv" },
                new WeightCategory { WeightKey = 3, CategoryName = "Heavy", MinimumKg = 2500m, MaximumKg = null, IconIdentifier = "truck" }
            );
        });

         mb.Entity<Compositions.Motorvehicle>(entity =>
        {
            // Either both First/Last names are present (Individual), or Organisation is present.
            entity.ToTable("data_Vehicles", schema: "data", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_data_Vehicles_Owner_Requirement",
                    "([OwnerOrganisation] <> N'' OR ([OwnerFirstName] <> N'' AND [OwnerLastName] <> N''))"
                );
            });

            entity.HasKey(v => v.VehicleIdentifier);

            // Max length for names set to 255 as is current practice at IRD.
            entity.Property(v => v.OwnerFirstName).HasMaxLength(255);
            entity.Property(v => v.OwnerLastName).HasMaxLength(255);
            entity.Property(v => v.OwnerOrganisation).HasMaxLength(255);

            entity.Property(v => v.ManufacturedYear).IsRequired();
            entity.Property(v => v.VehicleWeight).HasColumnType("decimal(10,2)").IsRequired();

            entity.HasOne(v => v.Manufacturer)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ManufacturerKey)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}