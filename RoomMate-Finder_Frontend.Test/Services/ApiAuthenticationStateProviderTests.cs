using System.Security.Claims;
using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ApiAuthenticationStateProviderTests
{
    private readonly Mock<IJSRuntime> _mockJs;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly ApiAuthenticationStateProvider _provider;

    public ApiAuthenticationStateProviderTests()
    {
        _mockJs = new Mock<IJSRuntime>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        _provider = new ApiAuthenticationStateProvider(_mockJs.Object, _httpClient);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ReturnsAnonymous_WhenNoToken()
    {
        // Arrange
        _mockJs.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        // Act
        var state = await _provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity!.IsAuthenticated.Should().BeFalse();
        _httpClient.DefaultRequestHeaders.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ReturnsAuthenticated_WhenValidToken()
    {
        // Arrange
        var payload = new { sub = "123", name = "Test User", role = "User" };
        var token = CreateDummyJwt(payload);
        
        _mockJs.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(token);

        // Act
        var state = await _provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity!.IsAuthenticated.Should().BeTrue();
        state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("123");
        state.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be("Test User");
        state.User.FindFirst(ClaimTypes.Role)?.Value.Should().Be("User");
        
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(token);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ParsesArrayClaims_Correctly()
    {
        // Arrange
        var payload = new { sub = "123", role = new[] { "Admin", "User" } };
        var token = CreateDummyJwt(payload);
        
        _mockJs.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(token);

        // Act
        var state = await _provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity!.IsAuthenticated.Should().BeTrue();
        var roles = state.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        roles.Should().Contain(ExpectedRoles);
    }

    private static readonly string[] ExpectedRoles = { "Admin", "User" };

    [Fact]
    public async Task MarkUserAsAuthenticated_NotifyStateChanged()
    {
        // Arrange
        var token = CreateDummyJwt(new { sub = "456" });
        bool stateChanged = false;
        _provider.AuthenticationStateChanged += (task) => stateChanged = true;

        // Act
        await _provider.MarkUserAsAuthenticated(token);

        // Assert
        stateChanged.Should().BeTrue();
        var state = await _provider.GetAuthenticationStateAsync(); // Should reflect new state? 
        // Note: GetAuthenticationStateAsync reads from JS. MarkUserAsAuthenticated notifies explicitly.
        // But GetAuthenticationStateAsync implementation reads storage every time.
        // Does MarkUserAsAuthenticated WRITE to storage? 
        // Checking code: No, it just parsing and notifying.
        // Usually implementation should write too, or the caller writes.
        // Code check: MarkUserAsAuthenticated(string token) does NOT write to LocalStorage.
        // It assumes caller did it? Or maybe it should? 
    }

    [Fact]
    public async Task MarkUserAsLoggedOut_NotifyStateChanged()
    {
        // Arrange
        bool stateChanged = false;
        _provider.AuthenticationStateChanged += (task) => stateChanged = true;

        // Act
        await _provider.MarkUserAsLoggedOut();

        // Assert
        stateChanged.Should().BeTrue();
    }

    private static string CreateDummyJwt(object payload)
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var json = JsonSerializer.Serialize(payload);
        var body = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        return $"{header}.{body}.signature"; // Signature doesn't matter for this parser
    }
}
