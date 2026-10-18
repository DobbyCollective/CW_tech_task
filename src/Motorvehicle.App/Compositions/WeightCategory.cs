namespace Motorvehicle.App.Compositions;

public class WeightCategory
{
    #region Compositions
    public int WeightKey {get; set;}
    public string CategoryName {get; set;} = string.Empty;
    public decimal MinimumKg {get; set;} 
    public decimal? MaximumKg {get; set;}
    public string IconIdentifier {get; set;} = string.Empty;
    #endregion

    #region Public methods
    //Returns boolean - if inputWeight is within MinimumKg and MaximumKg, true.
    public bool IsWithinRange(decimal inputWeight)
    {
        bool isHigherThanMinimum = inputWeight >= MinimumKg;
        bool isLowerThanMaximum = !MaximumKg.HasValue || inputWeight < MaximumKg.Value;

        return isHigherThanMinimum && isLowerThanMaximum;
    }


    #endregion
    
}