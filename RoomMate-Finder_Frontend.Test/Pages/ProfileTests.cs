using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Models;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class ProfileTests : IAsyncLifetime
{
    private readonly TestContext _ctx = new();
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IRoommateService> _mockRoommateService;
    
    public ProfileTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _mockProfileService = new Mock<IProfileService>();
        _mockRoommateService = new Mock<IRoommateService>();

        _ctx.Services.AddSingleton(_mockProfileService.Object);
        _ctx.Services.AddSingleton(_mockRoommateService.Object);
        
        _mockRoommateService.Setup(x => x.GetUserRoommateAsync(It.IsAny<Guid>()))
            .ReturnsAsync((UserRoommateDto?)null);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    [Fact]
    public void Profile_LoadingState_RendersSpinner()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _mockProfileService.Setup(x => x.GetByIdAsync(profileId))
            .Returns(async () => { await Task.Delay(100); return null; });

        // Act
        // Render MudPopoverProvider heavily recommended for MudBlazor components
        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));

        // Assert
        profileComp.FindComponents<MudProgressCircular>().Should().NotBeEmpty();
        profileComp.Markup.Should().Contain("Se încarcă profilul");
    }

    [Fact]
    public void Profile_ErrorState_RendersErrorMessage()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _mockProfileService.Setup(x => x.GetByIdAsync(profileId))
            .ReturnsAsync((ProfileDto?)null);

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));

        // Assert
        profileComp.WaitForState(() => profileComp.FindAll(".mud-alert-message").Count > 0);
        profileComp.Markup.Should().Contain("Profilul nu a fost găsit");
    }

    [Fact]
    public void Profile_ViewMode_RendersDetails()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new ProfileDto(
            profileId,
            "test@test.com",
            "Test User",
            25,
            "Male",
            "Test Uni",
            "My Bio",
            "Active",
            "Coding, Gaming",
            DateTime.UtcNow,
            "/images/default.png",
            "User"
        );

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(new ProfileDto(Guid.NewGuid(), "other@test.com", "Other", 20, "F", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User")); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));

        // Assert
        profileComp.WaitForState(() => profileComp.FindAll("h3").Count > 0);
        profileComp.Find("h3").TextContent.Should().Contain("Test User");
        profileComp.Markup.Should().Contain("Test Uni");
    }

    [Fact]
    public void Profile_EditMode_ToggleWorks_ForCurrentUser()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new ProfileDto(
            profileId,
            "me@test.com",
            "My Profile",
            25,
            "Male",
            "Uni",
            "Bio",
            "Life",
            "Int",
            DateTime.UtcNow,
            null,
            "User"
        );

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());

        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));
        
        profileComp.WaitForState(() => profileComp.FindAll("button").Count > 0);

        // Act - Click Edit
        var editBtn = profileComp.FindComponents<MudButton>()
            .FirstOrDefault(b => b.Markup.Contains("Editează Profilul"));
        
        editBtn.Should().NotBeNull();
        editBtn!.Find("button").Click();

        // Assert - Check for Form
        profileComp.WaitForState(() => profileComp.FindComponents<MudForm>().Count > 0);
        profileComp.FindComponents<MudTextField<string>>().Should().NotBeEmpty();
    }

    [Fact]
    public void Profile_EditMode_Cancel_RevertsToView()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new ProfileDto(profileId, "me@test.com", "My Profile", 25, "Male", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());

        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));
        
        profileComp.WaitForState(() => profileComp.FindAll("button").Count > 0);

        // Enter Edit Mode
        profileComp.FindComponents<MudButton>().First(b => b.Markup.Contains("Editează Profilul")).Find("button").Click();
        profileComp.WaitForState(() => profileComp.FindComponents<MudForm>().Count > 0);

        // Act - Click Cancel
        profileComp.FindComponents<MudButton>().First(b => b.Markup.Contains("Anulează")).Find("button").Click();

        // Assert - Back to View Mode
        profileComp.WaitForState(() => profileComp.FindComponents<MudForm>().Count == 0);
        profileComp.Find("h3").TextContent.Should().Contain("My Profile");
    }

    [Fact]
    public async Task Profile_EditMode_Save_CallsUpdateService()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new ProfileDto(profileId, "me@test.com", "My Profile", 25, "Male", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());
        _mockProfileService.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateProfileRequestDto>(), It.IsAny<IBrowserFile?>()))
            .ReturnsAsync(profile with { FullName = "Updated Name" });

        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));
        
        profileComp.WaitForState(() => profileComp.FindAll("button").Count > 0);

        // Enter Edit Mode
        profileComp.FindComponents<MudButton>().First(b => b.Markup.Contains("Editează Profilul")).Find("button").Click();
        profileComp.WaitForState(() => profileComp.FindComponents<MudForm>().Count > 0);

        // Change Name
        var nameField = profileComp.FindComponents<MudTextField<string>>().First(x => x.Instance.Label == "Nume Complet");
        nameField.Find("input").Change("Updated Name");

        // Act - Click Save
        var saveBtn = profileComp.FindComponents<MudButton>().First(b => b.Markup.Contains("Salvează"));
        await profileComp.InvokeAsync(() => saveBtn.Find("button").Click());

        // Assert
        _mockProfileService.Verify(x => x.UpdateAsync(profileId, It.Is<UpdateProfileRequestDto>(d => d.FullName == "Updated Name"), null), Times.Once);
        
        // Should return to view mode with new name
        profileComp.WaitForState(() => profileComp.FindComponents<MudForm>().Count == 0);
        profileComp.Markup.Should().Contain("Updated Name");
    }

    [Fact]
    public void Profile_OtherUser_NoEditButton()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var profile = new ProfileDto(profileId, "other@test.com", "Other Profile", 25, "Male", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");
        var currentUser = new ProfileDto(currentUserId, "me@test.com", "Me", 25, "Male", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(currentUser); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));
        
        profileComp.WaitForState(() => profileComp.FindAll("h3").Count > 0);

        // Assert
        profileComp.Markup.Should().NotContain("Editează Profilul");
    }
    [Fact]
    public void Profile_DisplaysReviews_WhenNotEmpty()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new ProfileDto(profileId, "test@test.com", "Test User", 25, "Male", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null, "User");
        
        var reviews = new List<Review>
        {
            new Review { ReviewerFullName = "Reviewer 1", Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow },
            new Review { ReviewerFullName = "Reviewer 2", Rating = 4, Comment = "Good", CreatedAt = DateTime.UtcNow }
        };

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(reviews);

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));

        // Assert
        profileComp.WaitForAssertion(() =>
        {
            profileComp.Markup.Should().Contain("Reviewer 1");
            profileComp.Markup.Should().Contain("Great!");
            profileComp.Markup.Should().Contain("Reviewer 2");
        });
    }

    [Fact]
    public void Profile_DisplaysRoommate_WhenExists()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        var profile = new ProfileDto(profileId, "test@test.com", "Test User", 25, "Male", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null, "User");
        
        var roommate = new UserRoommateDto(
            Guid.NewGuid(), // RelationshipId
            roommateId,
            "Room Mate",
            "roommate@test.com",
            null, // ProfilePicturePath
            22,   // Age
            "Uni", // University
            DateTime.UtcNow // Since
        );

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());
        _mockRoommateService.Setup(x => x.GetUserRoommateAsync(profileId)).ReturnsAsync(roommate);

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));

        // Assert
        profileComp.WaitForAssertion(() =>
        {
            profileComp.Markup.Should().Contain("Roommate");
            profileComp.Markup.Should().Contain("Room Mate");
            profileComp.Markup.Should().Contain("RM"); // Initials expected for "Room Mate"
        });
    }

    [Fact]
    public async Task Profile_EditMode_HandlesFileUpload()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new ProfileDto(profileId, "me@test.com", "My Profile", 25, "Male", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");

        _mockProfileService.Setup(x => x.GetByIdAsync(profileId)).ReturnsAsync(profile);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile); 
        _mockProfileService.Setup(x => x.GetUserReviews(profileId)).ReturnsAsync(new List<Review>());
        
        // Mock JS crop
        _ctx.JSInterop.Setup<string>("cropImageToCircle", _ => true).SetResult("data:image/jpeg;base64,ZmFrZQ=="); // "fake" base64
        _ctx.JSInterop.SetupVoid("preventScrollOnElement", _ => true);

        var cut = _ctx.Render<MudPopoverProvider>();
        var profileComp = _ctx.Render<Profile>(parameters => parameters.Add(p => p.Id, profileId));
        
        // Enter Edit Mode
        profileComp.WaitForState(() => profileComp.FindAll("button").Count > 0);
        profileComp.FindComponents<MudButton>().First(b => b.Markup.Contains("Editează Profilul")).Find("button").Click();
        profileComp.WaitForState(() => profileComp.FindComponents<MudForm>().Count > 0);

        // Upload File
        var inputFile = profileComp.FindComponent<InputFile>();
        inputFile.UploadFiles(InputFileContent.CreateFromText("fake image content", "test.jpg"));

        // Save
        var saveBtn = profileComp.FindComponents<MudButton>().First(b => b.Markup.Contains("Salvează"));
        await profileComp.InvokeAsync(() => saveBtn.Find("button").Click());
        
        // Verify UpdateAsync calls with a file
        _mockProfileService.Verify(x => x.UpdateAsync(
            profileId, 
            It.IsAny<UpdateProfileRequestDto>(), 
            It.Is<IBrowserFile>(f => f != null)
        ), Times.Once); // Might be invoked asynchronously so this verify might happen before call completes if not awaited? 
                        // But invoke OnClick is sync here for the test part, the handler is async.
                        // BUnit wait needed?
    }
}
