using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public class CompatibilityCalculatorService : ICompatibilityCalculatorService
{
    private readonly IAgeCompatibilityService _ageService;
    private readonly IGenderCompatibilityService _genderService;
    private readonly IUniversityCompatibilityService _universityService;
    private readonly ILifestyleCompatibilityService _lifestyleService;
    private readonly IInterestsCompatibilityService _interestsService;

    public CompatibilityCalculatorService(
        IAgeCompatibilityService ageService,
        IGenderCompatibilityService genderService,
        IUniversityCompatibilityService universityService,
        ILifestyleCompatibilityService lifestyleService,
        IInterestsCompatibilityService interestsService)
    {
        _ageService = ageService;
        _genderService = genderService;
        _universityService = universityService;
        _lifestyleService = lifestyleService;
        _interestsService = interestsService;
    }

    public CompatibilityResult CalculateCompatibility(Profile user1, Profile user2)
    {
        // Calculate individual scores
        var ageScore = _ageService.CalculateScore(user1.Age, user2.Age);
        var genderScore = _genderService.CalculateScore(user1.Gender, user2.Gender);
        var universityScore = _universityService.CalculateScore(user1.University, user2.University);
        var lifestyleScore = _lifestyleService.CalculateScore(user1.Lifestyle, user2.Lifestyle);
        var interestsScore = _interestsService.CalculateScore(user1.Interests, user2.Interests);

        // Calculate weighted overall score
        var overallScore = CalculateWeightedScore(ageScore, genderScore, universityScore, lifestyleScore, interestsScore);
        
        var compatibilityLevel = GetCompatibilityLevel(overallScore);

        return new CompatibilityResult(
            ageScore,
            genderScore,
            universityScore,
            lifestyleScore,
            interestsScore,
            overallScore,
            compatibilityLevel
        );
    }

    private static double CalculateWeightedScore(double age, double gender, double university, double lifestyle, double interests)
    {
        return (age * 0.2) + (gender * 0.15) + (university * 0.25) + (lifestyle * 0.25) + (interests * 0.15);
    }

    private static string GetCompatibilityLevel(double score)
    {
        return score switch
        {
            >= 85.0 => "Excellent Match",
            >= 70.0 => "Very Good Match",
            >= 55.0 => "Good Match",
            >= 40.0 => "Moderate Match",
            _ => "Low Compatibility"
        };
    }
}
