using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface ICompatibilityDescriptionService
{
    CompatibilityDetails CreateDetails(Profile user1, Profile user2, CompatibilityResult result);
}

public class CompatibilityDescriptionService : ICompatibilityDescriptionService
{
    private readonly IAgeCompatibilityService _ageService;
    private readonly IGenderCompatibilityService _genderService;
    private readonly IUniversityCompatibilityService _universityService;
    private readonly ILifestyleCompatibilityService _lifestyleService;
    private readonly IInterestsCompatibilityService _interestsService;

    public CompatibilityDescriptionService(
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

    public CompatibilityDetails CreateDetails(Profile user1, Profile user2, CompatibilityResult result)
    {
        return new CompatibilityDetails(
            result.AgeScore,
            result.GenderScore,
            result.UniversityScore,
            result.LifestyleScore,
            result.InterestsScore,
            _ageService.GetDescription(user1.Age, user2.Age),
            _genderService.GetDescription(user1.Gender, user2.Gender),
            _universityService.GetDescription(user1.University, user2.University),
            _lifestyleService.GetDescription(user1.Lifestyle, user2.Lifestyle, result.LifestyleScore),
            _interestsService.GetDescription(result.InterestsScore)
        );
    }
}
