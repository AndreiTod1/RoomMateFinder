using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using System.Net;
using System.Text.Json;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ListingServiceTests
{
    private static ListingDto CreateTestListing()
    {
        return new ListingDto(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Title", 
            "Desc", 
            "City", 
            "Area", 
            500,
            DateTime.UtcNow, 
            new List<string> { "WiFi" }, 
            DateTime.UtcNow, 
            true, 
            null, 
            "Owner Name", 
            ListingApprovalStatus.Approved, 
            null
        );
    }

    #region SearchAsync Tests

    [Fact]
    public async Task Given_ValidSearch_When_SearchAsyncIsCalled_Then_ReturnsResults()
    {
        // Arrange
        var responseDto = new ListingsResponse(new List<ListingSummaryDto>(), 10, 1, 10);
        var json = JsonSerializer.Serialize(responseDto);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new ListingsSearchRequest();

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task Given_SearchFailure_When_SearchAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new ListingsSearchRequest();

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_NullResponse_When_SearchAsyncIsCalled_Then_ReturnsEmptyResult()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new ListingsSearchRequest();

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task Given_ValidId_When_GetByIdAsyncIsCalled_Then_ReturnsListing()
    {
        // Arrange
        var listing = CreateTestListing();
        var json = JsonSerializer.Serialize(listing);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.GetByIdAsync(listing.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listing.Id);
    }

    [Fact]
    public async Task Given_NotFoundId_When_GetByIdAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Not Found", HttpStatusCode.NotFound);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_NetworkError_When_GetByIdAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task Given_ValidCreate_When_CreateAsyncIsCalled_Then_ReturnsCreatedListing()
    {
        // Arrange
        var listing = CreateTestListing();
        var json = JsonSerializer.Serialize(listing);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new CreateListingRequest(
            "New Room",
            "Desc",
            "City",
            "Area",
            400,
            DateTime.UtcNow,
            new List<string> { "TV" },
            Guid.NewGuid(),
            null
        );

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Title");
    }
    
    [Fact]
    public async Task Given_CreateFailure_When_CreateAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Create failed", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new CreateListingRequest(
            "New Room",
            "Desc",
            "City",
            "Area",
            400,
            DateTime.UtcNow,
            new List<string> { "TV" },
            Guid.NewGuid(),
            null
        );

        // Act
        Func<Task> act = () => service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to create*");
    }

    [Fact]
    public async Task Given_CreateWithEmptyImages_When_CreateAsyncIsCalled_Then_SendsWithoutImages()
    {
        // Arrange
        var listing = CreateTestListing();
        var json = JsonSerializer.Serialize(listing);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new CreateListingRequest(
            "New Room",
            "Desc",
            "City",
            "Area",
            400,
            DateTime.UtcNow,
            new List<string>(),
            Guid.NewGuid(),
            new List<IBrowserFile>()
        );

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task Given_ValidUpdate_When_UpdateAsyncIsCalled_Then_ReturnsUpdatedListing()
    {
        // Arrange
        var listing = CreateTestListing();
        var json = JsonSerializer.Serialize(listing);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new UpdateListingRequest(
            "Updated Title",
            "Desc",
            "City",
            "Area",
            600,
            DateTime.UtcNow,
            new List<string> { "WiFi" },
            true
        );

        // Act
        var result = await service.UpdateAsync(listing.Id, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_UpdateFailure_When_UpdateAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Update failed", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);
        var request = new UpdateListingRequest(
            "Updated Title",
            "Desc",
            "City",
            "Area",
            600,
            DateTime.UtcNow,
            new List<string> { "WiFi" },
            true
        );

        // Act
        Func<Task> act = () => service.UpdateAsync(Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to update*");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task Given_ValidDelete_When_DeleteAsyncIsCalled_Then_Completes()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var act = async () => await service.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_DeleteFailure_When_DeleteAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Delete failed", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        Func<Task> act = () => service.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to delete*");
    }

    #endregion

    #region ApproveAsync Tests

    [Fact]
    public async Task Given_ValidApprove_When_ApproveAsyncIsCalled_Then_ReturnsSuccess()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.ApproveAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("approved");
    }

    [Fact]
    public async Task Given_ApproveFailure_When_ApproveAsyncIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Cannot approve", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.ApproveAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to approve");
    }

    #endregion

    #region RejectAsync Tests

    [Fact]
    public async Task Given_ValidReject_When_RejectAsyncIsCalled_Then_ReturnsSuccess()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.RejectAsync(Guid.NewGuid(), "reason");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("rejected");
    }

    [Fact]
    public async Task Given_RejectFailure_When_RejectAsyncIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Cannot reject", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ListingService(httpClient);

        // Act
        var result = await service.RejectAsync(Guid.NewGuid(), "reason");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to reject");
    }

    #endregion
}

