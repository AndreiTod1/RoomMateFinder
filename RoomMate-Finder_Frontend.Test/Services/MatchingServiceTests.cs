using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using Xunit;
using System.Text.Json;

namespace RoomMate_Finder_Frontend.Test.Services;

public class MatchingServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly MatchingService _matchingService;

    public MatchingServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        _matchingService = new MatchingService(_httpClient);
    }

    [Fact]
    public async Task GetDiscoverProfilesAsync_ShouldReturnProfiles_WhenResponseIsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profiles = new List<MatchProfileDto>
        {
            new MatchProfileDto(
                Guid.NewGuid(), 
                "test@test.com", 
                "John Doe", 
                20, 
                "Male", 
                "Uni", 
                "Bio", 
                "Lifestyle", 
                "Interests", 
                88.5, 
                "High", 
                DateTime.Now, 
                null
            )
        };

        var responseContent = JsonSerializer.Serialize(profiles);
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"/matching/matches/{userId}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _matchingService.GetDiscoverProfilesAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(profiles);
    }

    [Fact]
    public async Task GetDiscoverProfilesAsync_ShouldReturnEmptyList_WhenResponseIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"/matching/matches/{userId}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null")
            });

        // Act
        var result = await _matchingService.GetDiscoverProfilesAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateCompatibilityAsync_ShouldReturnCompatibility_WhenResponseIsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var details = new CompatibilityDetailsDto(10, 10, 10, 10, 10, "A", "G", "U", "L", "I");
        var compatibility = new CompatibilityDto(userId, otherUserId, 88.0, "High", details);

        var responseContent = JsonSerializer.Serialize(compatibility);
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"/matching/compatibility/{userId}/{otherUserId}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _matchingService.CalculateCompatibilityAsync(userId, otherUserId);

        // Assert
        result.Should().BeEquivalentTo(compatibility);
    }

    [Fact]
    public async Task GetMyMatchesAsync_ShouldReturnMatches_WhenResponseIsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "match@test.com",
                "Match Name",
                21,
                "Female",
                "Uni",
                "Bio",
                "Lifestyle",
                "Interests",
                DateTime.Now,
                true,
                null
            )
        };

        var responseContent = JsonSerializer.Serialize(matches);
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"/matching/my-matches/{userId}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _matchingService.GetMyMatchesAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(matches);
    }

    [Fact]
    public async Task LikeProfileAsync_ShouldReturnResponse_WhenSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var responseDto = new LikeResponseDto(true, "Match!");

        var responseContent = JsonSerializer.Serialize(responseDto);
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri.ToString().Contains("/matching/like")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _matchingService.LikeProfileAsync(userId, targetUserId);

        // Assert
        result.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task PassProfileAsync_ShouldReturnResponse_WhenSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var responseDto = new PassResponseDto(true, "Passed");

        var responseContent = JsonSerializer.Serialize(responseDto);
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri.ToString().Contains("/matching/pass")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _matchingService.PassProfileAsync(userId, targetUserId);

        // Assert
        result.Should().BeEquivalentTo(responseDto);
    }
}
