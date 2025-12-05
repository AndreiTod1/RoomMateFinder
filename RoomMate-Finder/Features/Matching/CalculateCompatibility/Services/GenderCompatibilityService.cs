namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface IGenderCompatibilityService
{
    double CalculateScore(string gender1, string gender2);
    string GetDescription(string gender1, string gender2);
}

public class GenderCompatibilityService : IGenderCompatibilityService
{
    public double CalculateScore(string gender1, string gender2)
    {
        if (string.Equals(gender1, gender2, StringComparison.OrdinalIgnoreCase))
        {
            return 80.0;
        }
        else
        {
            return 60.0;
        }
    }

    public string GetDescription(string gender1, string gender2)
    {
        return string.Equals(gender1, gender2, StringComparison.OrdinalIgnoreCase) 
            ? "Same gender - often preferred for roommates"
            : "Different genders - still compatible";
    }
}
