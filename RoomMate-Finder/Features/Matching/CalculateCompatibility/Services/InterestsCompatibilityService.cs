namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface IInterestsCompatibilityService
{
    double CalculateScore(string interests1, string interests2);
    string GetDescription(double score);
}

public class InterestsCompatibilityService : IInterestsCompatibilityService
{
    public double CalculateScore(string interests1, string interests2)
    {
        if (string.IsNullOrEmpty(interests1) || string.IsNullOrEmpty(interests2))
            return 50.0;

        var interestsList1 = ParseInterests(interests1);
        var interestsList2 = ParseInterests(interests2);

        if (!interestsList1.Any() || !interestsList2.Any())
            return 50.0;

        return CalculateSimilarity(interestsList1, interestsList2);
    }

    public string GetDescription(double score)
    {
        return score switch
        {
            >= 70 => "Many shared interests - great for bonding",
            >= 40 => "Some common interests - good foundation",
            _ => "Different interests - opportunity to learn from each other"
        };
    }

    private static HashSet<string> ParseInterests(string interests)
    {
        return interests.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim().ToLower())
            .ToHashSet();
    }

    private static double CalculateSimilarity(HashSet<string> interests1, HashSet<string> interests2)
    {
        var commonInterests = interests1.Intersect(interests2).Count();
        
        if (commonInterests == 0)
            return 20.0;
        
        var similarity = (double)commonInterests / Math.Max(interests1.Count, interests2.Count);
        return Math.Round(similarity * 100, 2);
    }
}
