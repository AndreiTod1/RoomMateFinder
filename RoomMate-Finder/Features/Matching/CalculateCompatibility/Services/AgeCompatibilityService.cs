namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface IAgeCompatibilityService
{
    double CalculateScore(int age1, int age2);
    string GetDescription(int age1, int age2);
}

public class AgeCompatibilityService : IAgeCompatibilityService
{
    public double CalculateScore(int age1, int age2)
    {
        var ageDifference = Math.Abs(age1 - age2);
        
        return ageDifference switch
        {
            0 => 100.0,           // Same age - perfect match
            1 => 95.0,            // 1 year difference - excellent
            2 => 85.0,            // 2 years difference - very good
            3 => 75.0,            // 3 years difference - good
            4 => 65.0,            // 4 years difference - decent
            5 => 50.0,            // 5 years difference - moderate
            _ => Math.Max(0, 50 - (ageDifference - 5) * 5) // Decreases further
        };
    }

    public string GetDescription(int age1, int age2)
    {
        var diff = Math.Abs(age1 - age2);
        
        if (diff == 0)
        {
            return "Same age - perfect match!";
        }
        
        if (diff <= 2)
        {
            return $"{diff} year(s) difference - very compatible";
        }
        
        return $"{diff} year(s) difference - some age gap";
    }
}
