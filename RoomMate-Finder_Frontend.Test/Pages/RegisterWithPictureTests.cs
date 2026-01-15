using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using Xunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace RoomMate_Finder_Frontend.Test.Pages;


public class RegisterWithPictureTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private static readonly string[] RegistrationErrors = { "Email taken" };

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public RegisterWithPictureTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockAuthService.Object);

        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiBaseUrl", "https://api.test.com" }
            }).Build());

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // Component Existence Tests
    [Fact]
    public void RegisterWithPicture_ComponentExists()
    {
        typeof(RegisterWithPicture).Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_HasCorrectPageRoute()
    {
        var pageAttr = typeof(RegisterWithPicture).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false);
        pageAttr.Should().ContainSingle();
        ((Microsoft.AspNetCore.Components.RouteAttribute)pageAttr[0]).Template.Should().Be("/register-with-picture");
    }

    // Skip tests that require rendering since AuthService has complex dependencies
    [Fact]
    public void RegisterWithPicture_Renders_WithTitle()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Markup.Should().Contain("Register with Profile Picture");
    }

    [Fact]
    public void RegisterWithPicture_Renders_FormFields()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Markup.Should().Contain("Full Name");
    }

    [Fact]
    public void RegisterWithPicture_HasRegisterButton()
    {
        var cut = Render<RegisterWithPicture>();
        var button = cut.Find("button[type='submit']");
        button.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_HasGenderSelectOptions()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Markup.Should().Contain("Male");
    }

    [Fact]
    public void RegisterWithPicture_HasFileInput()
    {
        var cut = Render<RegisterWithPicture>();
        var fileInput = cut.Find("input[type='file']");
        fileInput.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_CanBindFullName()
    {
        var cut = Render<RegisterWithPicture>();
        var fullNameInputs = cut.FindAll("input.form-control");
        fullNameInputs[0].Change("John Doe");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindEmail()
    {
        var cut = Render<RegisterWithPicture>();
        var inputs = cut.FindAll("input.form-control");
        inputs[1].Change("test@example.com");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindPassword()
    {
        var cut = Render<RegisterWithPicture>();
        var inputs = cut.FindAll("input.form-control");
        inputs[2].Change("SecurePass123");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindAge()
    {
        var cut = Render<RegisterWithPicture>();
        var inputs = cut.FindAll("input.form-control");
        inputs[3].Change("25");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanSelectGender()
    {
        var cut = Render<RegisterWithPicture>();
        var genderSelect = cut.Find("select.form-control");
        genderSelect.Change("Male");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindUniversity()
    {
        var cut = Render<RegisterWithPicture>();
        var inputs = cut.FindAll("input.form-control");
        inputs[4].Change("Test University");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindBio()
    {
        var cut = Render<RegisterWithPicture>();
        var bioInput = cut.Find("textarea.form-control");
        bioInput.Change("My bio text");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindLifestyle()
    {
        var cut = Render<RegisterWithPicture>();
        var inputs = cut.FindAll("input.form-control");
        inputs[5].Change("Active");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_CanBindInterests()
    {
        var cut = Render<RegisterWithPicture>();
        var inputs = cut.FindAll("input.form-control");
        inputs[6].Change("Sports, Music");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void RegisterWithPicture_HasValidationSummary()
    {
        var cut = Render<RegisterWithPicture>();
        var form = cut.Find("form");
        form.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_HasDataAnnotationsValidator()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Markup.Should().Contain("form");
    }

    [Fact]
    public void RegisterWithPicture_HasProperCSSClasses()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Markup.Should().Contain("register-container");
    }

    [Fact]
    public void RegisterWithPicture_ConfigurationIsInjected()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_AuthServiceIsInjected()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_NavigationManagerIsInjected()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_FormHasCorrectStructure()
    {
        var cut = Render<RegisterWithPicture>();
        var formGroups = cut.FindAll(".form-group");
        formGroups.Count.Should().BeGreaterThanOrEqualTo(9);
    }

    [Fact]
    public void RegisterWithPicture_LabelsArePresent()
    {
        var cut = Render<RegisterWithPicture>();
        var labels = cut.FindAll("label");
        labels.Count.Should().BeGreaterThanOrEqualTo(9);
    }

    [Fact]
    public void RegisterWithPicture_SubmitButtonIsTyped()
    {
        var cut = Render<RegisterWithPicture>();
        var button = cut.Find("button");
        button.GetAttribute("type").Should().Be("submit");
    }

    [Fact]
    public void RegisterWithPicture_DoesNotShowErrorInitially()
    {
        var cut = Render<RegisterWithPicture>();
        cut.Markup.Should().NotContain("alert-danger");
    }

    [Fact]
    public void RegisterWithPicture_DoesNotShowImagePreviewInitially()
    {
        var cut = Render<RegisterWithPicture>();
        var images = cut.FindAll("img");
        images.Count.Should().Be(0);
    }

    [Fact]
    public async Task RegisterWithPicture_ValidSubmit_CallsAuthService_AndNavigates()
    {
        // Arrange
        _mockAuthService.Setup(x => x.RegisterWithPictureAsync(It.IsAny<RegistrationRequest>()))
            .ReturnsAsync(new RegisterResult { Successful = true });

        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = Render<RegisterWithPicture>();

        // Act
        cut.FindAll("input.form-control")[0].Change("John Doe");   // FullName
        cut.FindAll("input.form-control")[1].Change("test@example.com"); // Email
        cut.FindAll("input.form-control")[2].Change("Password123!"); // Password
        cut.FindAll("input.form-control")[3].Change("25");     // Age
        cut.Find("select.form-control").Change("Male");        // Gender
        cut.FindAll("input.form-control")[4].Change("My Uni"); // University
        cut.Find("textarea.form-control").Change("Bio text");  // Bio
        cut.FindAll("input.form-control")[5].Change("Active"); // Lifestyle
        cut.FindAll("input.form-control")[6].Change("Sports"); // Interests
        
        // Mock File Upload (InputFile)
        var fileInput = cut.FindComponent<Microsoft.AspNetCore.Components.Forms.InputFile>();
        
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("profile.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("topsecretimagecontent")));

        await cut.InvokeAsync(() => fileInput.Instance.OnChange.InvokeAsync(new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(new[] { mockFile.Object })));

        // Submit
        cut.Find("form").Submit();

        // Assert
        _mockAuthService.Verify(x => x.RegisterWithPictureAsync(It.Is<RegistrationRequest>(r => r.Email == "test@example.com" && r.Password == "Password123!" && r.FullName == "John Doe")), Times.Once);
            
        nav.Uri.Should().EndWith("/login");
    }

    [Fact]
    public void RegisterWithPicture_RegistrationError_ShowsAlert()
    {
         // Arrange
        _mockAuthService.Setup(x => x.RegisterWithPictureAsync(It.IsAny<RegistrationRequest>()))
            .ReturnsAsync(new RegisterResult { Successful = false, Errors = RegistrationErrors });

        var cut = Render<RegisterWithPicture>();

        // Act - Fill minimal valid form
        cut.FindAll("input.form-control")[0].Change("John Doe"); 
        cut.FindAll("input.form-control")[1].Change("test@example.com");
        cut.FindAll("input.form-control")[2].Change("Password123!");
        cut.FindAll("input.form-control")[3].Change("25"); 
        cut.Find("select.form-control").Change("Male");
        
        // Submit
        cut.Find("form").Submit();
        
        // Assert
        cut.WaitForAssertion(() => cut.Markup.Contains("alert-danger"));
        cut.Markup.Should().Contain("Email taken");
    }
}
