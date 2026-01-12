namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface ILifestyleCompatibilityService
{
    double CalculateScore(string lifestyle1, string lifestyle2);
    string GetDescription(string lifestyle1, string lifestyle2, double score);
}

public class LifestyleCompatibilityService : ILifestyleCompatibilityService
{
    private readonly Dictionary<string, List<string>> _compatiblePairs;

    public LifestyleCompatibilityService()
    {
        _compatiblePairs = new Dictionary<string, List<string>>
        {
            { "quiet", new List<string> { "studious", "calm", "peaceful" } },
            { "social", new List<string> { "outgoing", "party", "active" } },
            { "studious", new List<string> { "quiet", "academic", "focused" } },
            { "active", new List<string> { "social", "sporty", "energetic" } },
            { "organized", new List<string> { "clean", "neat", "structured" } }
        };
    }

    public double CalculateScore(string lifestyle1, string lifestyle2)
    {
        var life1 = lifestyle1.ToLower();
        var life2 = lifestyle2.ToLower();

        if (life1 == life2)
        {
            return 100.0;
        }
        
        if (IsCompatible(life1, life2))
        {
            return 75.0;
        }

        return 30.0;
    }

    public string GetDescription(string lifestyle1, string lifestyle2, double score)
    {
        if (string.Equals(lifestyle1, lifestyle2, StringComparison.OrdinalIgnoreCase))
        {
            return "Same lifestyle - excellent compatibility";
        }
        
        if (score > 60)
        {
            return "Compatible lifestyles";
        }
        
        return "Different lifestyles - may need compromise";
    }

    private bool IsCompatible(string life1, string life2)
    {
        if (_compatiblePairs.ContainsKey(life1) && _compatiblePairs[life1].Contains(life2))
        {
            return true;
        }
        if (_compatiblePairs.ContainsKey(life2) && _compatiblePairs[life2].Contains(life1))
        {
            return true;
        }
        return false;
    }
}
