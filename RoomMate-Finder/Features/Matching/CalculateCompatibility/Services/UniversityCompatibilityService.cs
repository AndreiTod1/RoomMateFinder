namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface IUniversityCompatibilityService
{
    double CalculateScore(string university1, string university2);
    string GetDescription(string university1, string university2);
}

public class UniversityCompatibilityService : IUniversityCompatibilityService
{
    public double CalculateScore(string university1, string university2)
    {
        if (string.Equals(university1, university2, StringComparison.OrdinalIgnoreCase))
        {
            return 100.0; 
        }
        else
        {
            return 40.0;
        }
    }

    public string GetDescription(string university1, string university2)
    {
        return string.Equals(university1, university2, StringComparison.OrdinalIgnoreCase)
            ? "Same university - great for commuting together"
            : "Different universities - manageable";
    }
}
