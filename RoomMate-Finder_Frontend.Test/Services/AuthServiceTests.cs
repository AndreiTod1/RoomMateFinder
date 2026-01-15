using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using System.Net;

namespace RoomMate_Finder_Frontend.Test.Services;

public class AuthServiceTests
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly Mock<ApiAuthenticationStateProvider> _authProviderMock;

    public AuthServiceTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _authProviderMock = new Mock<ApiAuthenticationStateProvider>(
            _jsMock.Object, 
            new HttpClient() // Dummy, not used in virtual method calls usually, but passed to ctor
        );
    }

    [Fact]
    public async Task Given_ValidCredentials_When_LoginAsyncIsCalled_Then_SetsTokenAndAuthenticates()
    {
        // Arrange
        var token = "valid_token_123";
        var responseContent = $$"""{"token": "{{token}}"}""";
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        await service.LoginAsync("test@test.com", "password");

        // Assert
        // 1. Verify token saved to LocalStorage
        _jsMock.Verify(x => x.InvokeAsync<object>(
            "localStorage.setItem", 
            It.Is<object[]>(args => 
                args.Length == 2 && 
                (string)args[0] == "authToken" && 
                (string)args[1] == token)
            ), Times.Once);

        // 2. Verify Authorization header set
        httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        httpClient.DefaultRequestHeaders.Authorization!.Parameter.Should().Be(token);

        // 3. Verify AuthProvider notified
        _authProviderMock.Verify(x => x.MarkUserAsAuthenticated(token), Times.Once);
    }

    [Fact]
    public async Task Given_InvalidCredentials_When_LoginAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Invalid credentials", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        Func<Task> act = () => service.LoginAsync("test@test.com", "wrong");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials");
        
        _jsMock.Verify(x => x.InvokeAsync<object>("localStorage.setItem", It.IsAny<object[]>()), Times.Never);
        _authProviderMock.Verify(x => x.MarkUserAsAuthenticated(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Given_MissingToken_When_LoginAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var responseContent = "{}"; // No token
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(responseContent);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        Func<Task> act = () => service.LoginAsync("test@test.com", "password");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Token missing*");
    }

    [Fact]
    public async Task When_LogoutAsyncIsCalled_Then_ClearsTokenAndNotifiesProvider()
    {
        // Arrange
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost") };
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "old_token");

        var service = new AuthService(httpClient, _jsMock.Object, _authProviderMock.Object);

        // Act
        await service.LogoutAsync();

        // Assert
        // 1. Verify LocalStorage removed
        _jsMock.Verify(x => x.InvokeAsync<object>(
            "localStorage.removeItem", 
            It.Is<object[]>(args => (string)args[0] == "authToken")
        ), Times.Once);

        // 2. Verify Header cleared
        httpClient.DefaultRequestHeaders.Authorization.Should().BeNull();

        // 3. Verify AuthProvider notified
        _authProviderMock.Verify(x => x.MarkUserAsLoggedOut(), Times.Once);
    }

    [Fact]
    public async Task When_GetTokenAsyncIsCalled_Then_ReturnsTokenFromStorage()
    {
        // Arrange
        var expectedToken = "stored_token";
        _jsMock.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.Is<object[]>(args => (string)args[0] == "authToken")))
               .ReturnsAsync(expectedToken);

        var service = new AuthService(null!, _jsMock.Object, null!); // Http/Auth not needed

        // Act
        var token = await service.GetTokenAsync();

        // Assert
        token.Should().Be(expectedToken);
    }
}
