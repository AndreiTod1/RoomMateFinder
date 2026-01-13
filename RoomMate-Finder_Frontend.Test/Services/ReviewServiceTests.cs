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

public class ReviewServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        _reviewService = new ReviewService(_httpClient);
    }

    [Fact]
    public async Task GetMatchesForReview_ShouldReturnMatches_WhenResponseIsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "test@test.com",
                "Match Name",
                22,
                "Male",
                "Uni",
                "Bio",
                "Lifestyle",
                "Interests",
                DateTime.Now,
                true,
                null
            ),
             new UserMatchDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "test2@test.com",
                "Match Name 2",
                23,
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
        var result = await _reviewService.GetMatchesForReview(userId);

        // Assert
        result.Should().BeEquivalentTo(matches);
    }

    [Fact]
    public async Task GetMatchesForReview_ShouldReturnEmptyList_WhenResponseIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
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
                Content = new StringContent("null")
            });

        // Act
        var result = await _reviewService.GetMatchesForReview(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task LeaveReviewAsync_ShouldSucceed_WhenResponseIsSuccess()
    {
        // Arrange
        var reviewedUserId = "test-user-id";
        var rating = 5;
        var comment = "Great roommate!";
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri.ToString().Contains($"/profiles/{reviewedUserId}/reviews")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _reviewService.LeaveReviewAsync(reviewedUserId, rating, comment);

        // Assert
        // Verify that the request was sent with correct content
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.Content != null),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LeaveReviewAsync_ShouldThrowException_WhenResponseIsFailure()
    {
        // Arrange
        var reviewedUserId = "test-user-id";
        var rating = 5;
        var comment = "Great roommate!";
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var act = async () => await _reviewService.LeaveReviewAsync(reviewedUserId, rating, comment);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
