using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Motorvehicle.App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "config_Manufacturers",
                schema: "config",
                columns: table => new
                {
                    ManufacturerKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config_Manufacturers", x => x.ManufacturerKey);
                });

            migrationBuilder.CreateTable(
                name: "config_WeightCategories",
                schema: "config",
                columns: table => new
                {
                    WeightKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinimumKg = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaximumKg = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IconIdentifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config_WeightCategories", x => x.WeightKey);
                });

            migrationBuilder.CreateTable(
                name: "data_Vehicles",
                schema: "data",
                columns: table => new
                {
                    VehicleIdentifier = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerFirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OwnerLastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OwnerOrganisation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ManufacturerKey = table.Column<int>(type: "int", nullable: false),
                    ManufacturedYear = table.Column<int>(type: "int", nullable: false),
                    VehicleWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_Vehicles", x => x.VehicleIdentifier);
                    table.CheckConstraint("CK_data_Vehicles_Owner_Requirement", "([OwnerOrganisation] <> N'' OR ([OwnerFirstName] <> N'' AND [OwnerLastName] <> N''))");
                    table.ForeignKey(
                        name: "FK_data_Vehicles_config_Manufacturers_ManufacturerKey",
                        column: x => x.ManufacturerKey,
                        principalSchema: "config",
                        principalTable: "config_Manufacturers",
                        principalColumn: "ManufacturerKey",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "config",
                table: "config_Manufacturers",
                columns: new[] { "ManufacturerKey", "ManufacturerName" },
                values: new object[,]
                {
                    { 1, "Mazda" },
                    { 2, "Mercedes" },
                    { 3, "Honda" },
                    { 4, "Ferrari" },
                    { 5, "Toyota" }
                });

            migrationBuilder.InsertData(
                schema: "config",
                table: "config_WeightCategories",
                columns: new[] { "WeightKey", "CategoryName", "IconIdentifier", "MaximumKg", "MinimumKg" },
                values: new object[,]
                {
                    { 1, "Light", "car", 500m, 0m },
                    { 2, "Medium", "suv", 2500m, 500m },
                    { 3, "Heavy", "truck", null, 2500m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_config_Manufacturers_ManufacturerName",
                schema: "config",
                table: "config_Manufacturers",
                column: "ManufacturerName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_data_Vehicles_ManufacturerKey",
                schema: "data",
                table: "data_Vehicles",
                column: "ManufacturerKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "config_WeightCategories",
                schema: "config");

            migrationBuilder.DropTable(
                name: "data_Vehicles",
                schema: "data");

            migrationBuilder.DropTable(
                name: "config_Manufacturers",
                schema: "config");
        }
    }
}
