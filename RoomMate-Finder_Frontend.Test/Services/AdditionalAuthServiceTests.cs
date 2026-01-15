using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using System.Net;

namespace RoomMate_Finder_Frontend.Test.Services;

/// <summary>
/// Additional AuthService tests to increase coverage.
/// </summary>
public class AdditionalAuthServiceTests
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly Mock<ApiAuthenticationStateProvider> _authProviderMock;

    public AdditionalAuthServiceTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _authProviderMock = new Mock<ApiAuthenticationStateProvider>(
            _jsMock.Object, 
            new HttpClient()
        );
    }

    #region RegisterWithPictureAsync Tests

    [Fact]
    public async Task Given_ValidData_When_RegisterWithPictureAsyncIsCalled_Then_ReturnsSuccessfulResult()
    {
        // Arrange
        var responseContent = """{"id": "123", "email": "test@test.com"}""";
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent, HttpStatusCode.Created);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        var result = await service.RegisterWithPictureAsync(
            "test@test.com", "Password123!", "Test User", 25, "Male",
            "MIT", "Developer", "Active", "Coding", "http://picture.url/pic.jpg"
        );

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Errors.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Given_FailedResponse_When_RegisterWithPictureAsyncIsCalled_Then_ReturnsUnsuccessfulResult()
    {
        // Arrange
        var errorMessage = "Email already exists";
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(errorMessage, HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        var result = await service.RegisterWithPictureAsync(
            "existing@test.com", "Password123!", "Test User", 25, "Male",
            "MIT", "Developer", "Active", "Coding", null
        );

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.Errors.Should().Contain(errorMessage);
    }

    [Fact]
    public async Task Given_NullProfilePicture_When_RegisterWithPictureAsyncIsCalled_Then_StillWorks()
    {
        // Arrange
        var responseContent = """{"id": "456"}""";
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent, HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        var result = await service.RegisterWithPictureAsync(
            "new@test.com", "Pass123!", "New User", 30, "Female",
            "Harvard", "Student", "Quiet", "Reading", null
        );

        // Assert
        result.Successful.Should().BeTrue();
    }

    [Fact]
    public async Task Given_ServerError_When_RegisterWithPictureAsyncIsCalled_Then_ReturnsError()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Internal Server Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        var result = await service.RegisterWithPictureAsync(
            "test@test.com", "Pass123!", "User", 25, "Male",
            "Uni", "Bio", "Lifestyle", "Interests", null
        );

        // Assert
        result.Successful.Should().BeFalse();
    }

    #endregion

    #region GetTokenAsync Edge Cases

    [Fact]
    public async Task Given_JsRuntimeThrows_When_GetTokenAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
               .ThrowsAsync(new JSException("JS Error"));

        var service = new AuthService(null!, _jsMock.Object, null!);

        // Act
        var token = await service.GetTokenAsync();

        // Assert
        token.Should().BeNull();
    }

    [Fact]
    public async Task Given_NoTokenInStorage_When_GetTokenAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
               .ReturnsAsync((string?)null);

        var service = new AuthService(null!, _jsMock.Object, null!);

        // Act
        var token = await service.GetTokenAsync();

        // Assert
        token.Should().BeNull();
    }

    #endregion

    #region LoginAsync Edge Cases

    [Fact]
    public async Task Given_UppercaseTokenProperty_When_LoginAsyncIsCalled_Then_ParsesCorrectly()
    {
        // Arrange
        var token = "uppercase_token_property";
        var responseContent = $$"""{"Token": "{{token}}"}"""; // Note: uppercase Token
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        await service.LoginAsync("test@test.com", "password");

        // Assert
        _jsMock.Verify(x => x.InvokeAsync<object>(
            "localStorage.setItem", 
            It.Is<object[]>(args => (string)args[1] == token)
        ), Times.Once);
    }

    [Fact]
    public async Task Given_EmptyToken_When_LoginAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var responseContent = """{"token": ""}""";
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        Func<Task> act = () => service.LoginAsync("test@test.com", "password");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Token empty*");
    }

    [Fact]
    public async Task Given_WhitespaceToken_When_LoginAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var responseContent = """{"token": "   "}""";
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        Func<Task> act = () => service.LoginAsync("test@test.com", "password");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Token empty*");
    }

    [Fact]
    public async Task Given_EmptyErrorMessage_When_LoginAsyncFails_Then_ShowsDefaultMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        Func<Task> act = () => service.LoginAsync("test@test.com", "wrong");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Login failed");
    }

    #endregion
}
