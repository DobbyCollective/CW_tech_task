namespace Motorvehicle.App.Compositions;

public class Motorvehicle
{
    public int VehicleIdentifier {get; set;}
    public string OwnerFirstName {get; set;} = string.Empty;
    public string OwnerLastName {get; set;} = string.Empty;
    public string OwnerOrganisation {get; set;} = string.Empty;
    public int ManufacturerKey {get; set;}
    public Manufacturer? Manufacturer {get; set;}
    public int ManufacturedYear {get; set;}
    public decimal VehicleWeight {get; set;}
}