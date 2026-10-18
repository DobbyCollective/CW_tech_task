namespace Motorvehicle.App.Compositions;

public class Manufacturer
{
    public int ManufacturerKey {get; set;}
    public string ManufacturerName {get; set;} = string.Empty;
    public ICollection<Motorvehicle> Vehicles {get; set;} = new List<Motorvehicle>();
}