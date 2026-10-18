# Motorvehicle & Weight Category Manager
Created for CreditWorks Software Engineer role tech task.

Technical programming assignment submission for CreditWorks. This web application manages motor vehicles and dynamic vehicle weight categories using a C# / ASP.NET Core backend, Entity Framework Core, Microsoft SQL Server, an xUnit automated test suite, and a React + TypeScript (Vite) frontend.

1. Setup Instructions

1.1 Required Software & Database Requirements

.NET SDK: .NET 8.0 SDK (or .NET 9.0 SDK)

Database: Microsoft SQL Server 2019 or later (SQL Server Express or SQL Server Express LocalDB)

Node.js: Node.js 18+ (LTS) and npm

1.2 Configuration Steps

Clone the repository:

git clone <https://github.com/DobbyCollective/CW_tech_task/>
cd CW_tech_task


Verify the SQL Server connection string in src/Motorvehicle.Api/appsettings.json:

SQL Server Express (default instance SQLEXPRESS):

"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MotorvehicleDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}


SQL Server Express LocalDB (alternative):
Replace Server=.\\SQLEXPRESS with Server=(localdb)\\mssqllocaldb.

Verify the API proxy target in client/vite.config.ts matches the HTTP port in src/Motorvehicle.Api/Properties/launchSettings.json (e.g., http://localhost:5000).

1.3 Database Creation & Migration Instructions

The solution includes Entity Framework Core migrations (src/Motorvehicle.App/Migrations/) and seed data in AppDbContext.OnModelCreating.

Automatic Initialisation on Startup: Starting Motorvehicle.Api automatically runs db.Database.Migrate(), which creates MotorvehicleDb, establishes the [config] and [data] schemas, creates all tables and constraints, and seeds the initial manufacturers (Mazda, Mercedes, Honda, Ferrari, Toyota) and initial weight categories (Light: 0–500 kg, Medium: 500–2500 kg, Heavy: 2500 kg+).

Manual CLI Migration (Optional):

dotnet ef database update --project src/Motorvehicle.App --startup-project src/Motorvehicle.Api


1.4 How to Build the Application

From the repository root:

# Build the .NET solution
dotnet build

# Install dependencies and build the React frontend
cd client
npm install
npm run build
cd ..


1.5 How to Run the Application

Start the ASP.NET Core Web API (Terminal 1, from the repository root):

dotnet run --project src/Motorvehicle.Api


Start the React Frontend (Terminal 2):

cd client
npm run dev


Open http://localhost:5173 in your browser.

1.6 How to Execute Automated Tests

Run the xUnit test suite from the repository root:

dotnet test


2. Design & Development Notes

2.1 Overall Solution Architecture

The solution avoids unnecessary enterprise ceremony while maintaining a clear separation between API presentation, business rules, data access, and domain models:

src/Motorvehicle.App (Compositions, Data, Tools):

Motorvehicle.App.Compositions: Domain entities (Manufacturer, Motorvehicle, WeightCategory), including WeightCategory.IsWithinRange(decimal inputWeight).

Motorvehicle.App.Data: AppDbContext with EF Core schema mappings, check constraints, foreign keys, and initial seed data.

Motorvehicle.App.Tools: Pure domain validation (WeightCategoryValidator) and application services (CategoryTools, VehicleTools).

src/Motorvehicle.Api (Controllers): Thin REST controllers (VehiclesController, CategoriesController, ManufacturersController), CORS configuration, and global ProblemDetails exception handling (app.UseExceptionHandler()).

tests/Motorvehicle.Tests: Automated xUnit tests verifying boundary resolution, gap/overlap prevention, dynamic recategorisation when categories change, neighbour boundary healing during individual category CRUD operations, and vehicle validation.

client/: Modular React + TypeScript application divided into section components (HeaderSection, StatusBannerSection, AddVehicleSection, VehicleListSection, CreateCategorySection, CategoryListSection) styled with responsive CSS Grid and Flexbox.

2.2 Database Design ([config] vs. [data] Schemas)

All data is persisted in a single relational database (MotorvehicleDb) separated into two SQL Server schemas to support least-privilege database security while preserving cross-table foreign key integrity:

[config].[config_Manufacturers]: Stores predefined manufacturers (ManufacturerKey, ManufacturerName with a unique index). Modelling manufacturers as a dedicated lookup table rather than an enum or hardcoded list allows new manufacturers to be added in the future without code changes or redeployment.

[config].[config_WeightCategories]: Stores weight category definitions (WeightKey, CategoryName, MinimumKg as decimal(10,2), nullable MaximumKg as decimal(10,2), and IconIdentifier).

[data].[data_Vehicles]: Stores registered vehicles (VehicleIdentifier, OwnerFirstName, OwnerLastName, OwnerOrganisation, ManufacturerKey, ManufacturedYear, and VehicleWeight as decimal(10,2)) with a foreign key (DeleteBehavior.Restrict) referencing [config].[config_Manufacturers].

2.3 Category Calculation Approach (Calculated vs. Stored)

A vehicle's weight category is calculated dynamically on read rather than stored as a foreign key column on [data].[data_Vehicles]:

Why calculated: Requirement 6 states that whenever category definitions change, all existing vehicles must immediately reflect the updated categories without any possibility of becoming inconsistent. Storing a WeightKey foreign key on each vehicle row would introduce data denormalisation and require bulk-updating the vehicle table on every category edit.

How it works: VehicleTools.GetVehiclesAsync loads the ordered WeightCategory list and resolves each vehicle's category via WeightCategoryValidator.ResolveCategory(vehicle.VehicleWeight, categories).

2.4 Category Boundary Rules

Category weight ranges are evaluated as contiguous half-open intervals [MinimumKg, MaximumKg):

Minimum weight (MinimumKg) is inclusive (>=).

Maximum weight (MaximumKg) is exclusive (<), and MaximumKg = null represents an unbounded upper limit (∞, "and above").

Exact boundary example: With the initial categories (Light: 0–500 kg, Medium: 500–2500 kg, Heavy: 2500 kg+):

499.99 kg resolves to Light (0 <= 499.99 < 500).

500.00 kg resolves deterministically to Medium (500 <= 500.00 < 2500).

2500.00 kg resolves deterministically to Heavy (2500 <= 2500.00).

Contiguous coverage enforcement: WeightCategoryValidator.ValidateWeightCategories enforces that the lowest category starts at 0.00 kg, every subsequent category's MinimumKg equals the previous category's MaximumKg (prev.MaximumKg == current.MinimumKg), and only the highest category has MaximumKg = null.

2.5 Significant Design Decisions & Trade-Offs

Individual Category CRUD with Automatic Neighbour Boundary Healing:

Problem: If a user edits a single category's maximum weight (e.g., changing Medium from 500–2500 kg to 500–2000 kg) in isolation, a gap (2000–2500 kg) would normally occur unless the adjacent Heavy category is also updated at the same time.

Decision: CategoryTools supports individual REST operations (POST, PUT, DELETE) by automatically adjusting the shared boundary of the adjacent neighbour inside the same unit of work (Heavy.MinimumKg is automatically updated to 2000 kg), validating the entire resulting set via WeightCategoryValidator before calling SaveChangesAsync(). Similarly, deleting a category automatically merges its range into its neighbour, and deleting the final remaining category is blocked.

Category Icon Representation:

Icons are persisted in the database as lightweight semantic keys ("car", "suv", "van", "truck", "heavy-truck") and rendered as inline SVG components in React (CategoryIcon.tsx), avoiding external icon library bloat or broken image URLs.

2.6 Important Assumptions

Individual vs. Organisation Vehicle Owners:

In real-world credit and fleet management scenarios, a vehicle may be owned either by a private individual or by a company/organisation.

We modelled [data].[data_Vehicles] with OwnerFirstName, OwnerLastName, and OwnerOrganisation (up to 255 characters). If OwnerOrganisation is provided, OwnerFirstName and OwnerLastName are optional; if OwnerOrganisation is blank, both OwnerFirstName and OwnerLastName are mandatory. This rule is enforced both in VehicleTools.ValidateVehicleInput and by the database check constraint CK_data_Vehicles_Owner_Requirement.

Sensible Year of Manufacture Range:

ManufacturedYear is validated to be between 1886 (the year of the first modern petrol automobile) and CurrentYear + 1 (to allow next-model-year vehicles registered late in the calendar year).

Positive Vehicle Weight vs. 0 kg Lower Category Bound:

The lowest category starts at 0.00 kg so weights between 0.01 kg and 500.00 kg are covered, while VehicleTools.ValidateVehicleInput and the SQL check constraint require every vehicle's VehicleWeight to be strictly greater than 0 (>= 0.01 kg) with at most 2 decimal places.

2.7 Security Considerations & Known Limitations

Implemented Security Practices:

Server-Side Validation: All vehicle and category rules are enforced on the server in VehicleTools, CategoryTools, and WeightCategoryValidator, backed by SQL Server CHECK, UNIQUE, and FOREIGN KEY constraints.

SQL Injection Prevention: All database queries use parameterised LINQ-to-Entities queries via Entity Framework Core.

Safe Error Responses: app.UseExceptionHandler() with AddProblemDetails() prevents internal stack traces or database implementation details from leaking to the client.

No Committed Secrets: appsettings.json uses Windows Integrated Security (Trusted_Connection=True) for local development so no passwords or API keys exist in source control.

Deliberate Omissions & Known Limitations:

Authentication & Authorisation: Omitted as permitted; in production, Category Administration endpoints (POST/PUT/DELETE /api/categories) would require role-based authorisation (e.g., JWT/OIDC with an Admin policy).

In-Memory Sorting & Pagination: VehicleTools.GetVehiclesAsync loads vehicles into memory before sorting. This is fast and simple for small-to-medium datasets, but a high-volume production system would push sorting and keyset/offset pagination (Skip/Take) down to SQL Server.

Concurrency Control: Simultaneous category edits by multiple administrators are not currently guarded by optimistic concurrency tokens ([Timestamp] / rowversion).

