using FluentAssertions;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using System.Net;
using System.Text.Json;

namespace RoomMate_Finder_Frontend.Test.Services;

public class RoommateServiceTests
{
    #region SendRoommateRequestAsync Tests

    [Fact]
    public async Task Given_ValidRequest_When_SendRoommateRequestAsyncIsCalled_Then_ReturnsResponse()
    {
        // Arrange
        var responseDto = new SendRoommateRequestResponse(Guid.NewGuid(), "Sent");
        var json = JsonSerializer.Serialize(responseDto);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be("Sent");
    }

    [Fact]
    public async Task Given_ErrorWithErrorProperty_When_SendRoommateRequestAsyncIsCalled_Then_ThrowsWithErrorMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"error\": \"Failed\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed");
    }

    [Fact]
    public async Task Given_ErrorWithMessageProperty_When_SendRoommateRequestAsyncIsCalled_Then_ThrowsWithMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"message\": \"Request failed\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Request failed");
    }

    [Fact]
    public async Task Given_ErrorWithTitleProperty_When_SendRoommateRequestAsyncIsCalled_Then_ThrowsWithTitle()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"title\": \"Bad Request\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Bad Request");
    }

    [Fact]
    public async Task Given_EmptyErrorResponse_When_SendRoommateRequestAsyncIsCalled_Then_ThrowsDefaultMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*BadRequest*");
    }

    [Fact]
    public async Task Given_InvalidJson_When_SendRoommateRequestAsyncIsCalled_Then_ThrowsDefaultMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("not valid json", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*BadRequest*");
    }

    [Fact]
    public async Task Given_JsonWithoutKnownProperties_When_SendRoommateRequestAsyncIsCalled_Then_ReturnsRawContent()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"unknown\": \"value\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.SendRoommateRequestAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("{\"unknown\": \"value\"}");
    }

    #endregion

    #region GetMyRequestsAsync Tests

    [Fact]
    public async Task When_GetMyRequestsAsyncIsCalled_Then_ReturnsRequests()
    {
        // Arrange
        var responseDto = new MyRoommateRequestsResponse(
            new List<MyRequestDto>(),
            new List<MyRequestDto>(),
            new List<MyRoommateDto>()
        );
        var json = JsonSerializer.Serialize(responseDto);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetMyRequestsAsync();

        // Assert
        result.Should().NotBeNull();
        result!.SentRequests.Should().BeEmpty();
    }

    #endregion

    #region GetUserRoommateAsync Tests

    [Fact]
    public async Task Given_ExistingRoommate_When_GetUserRoommateAsyncIsCalled_Then_ReturnsRoommate()
    {
        // Arrange
        var roommate = new UserRoommateDto(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Test", 
            "test@test.com", 
            null, 
            21, 
            "Uni", 
            DateTime.Now
        );
        var json = JsonSerializer.Serialize(roommate);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetUserRoommateAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.RoommateName.Should().Be("Test");
    }

    [Fact]
    public async Task Given_NotFoundResponse_When_GetUserRoommateAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.NotFound);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetUserRoommateAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_NetworkError_When_GetUserRoommateAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetUserRoommateAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPendingRequestsAsync Tests

    [Fact]
    public async Task When_GetPendingRequestsAsyncIsCalled_Then_ReturnsList()
    {
        // Arrange
        var list = new List<PendingRoommateRequestDto> 
        { 
            new PendingRoommateRequestDto(
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                "Requester", 
                "req@test.com", 
                Guid.NewGuid(), 
                "Target", 
                "target@test.com", 
                "Msg", 
                DateTime.Now
            ) 
        };
        var json = JsonSerializer.Serialize(list);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetPendingRequestsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_NullResponse_When_GetPendingRequestsAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetPendingRequestsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region ApproveRequestAsync Tests
    
    [Fact]
    public async Task Given_ValidApproval_When_ApproveRequestAsyncIsCalled_Then_ReturnsResponse()
    {
        // Arrange
        var responseDto = new ApproveRequestResponse(Guid.NewGuid(), "Approved");
        var json = JsonSerializer.Serialize(responseDto);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.ApproveRequestAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be("Approved");
    }

    [Fact]
    public async Task Given_Error_When_ApproveRequestAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"error\": \"Cannot approve\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.ApproveRequestAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot approve");
    }

    #endregion

    #region RejectRequestAsync Tests

    [Fact]
    public async Task Given_ValidRejection_When_RejectRequestAsyncIsCalled_Then_ReturnsResponse()
    {
        // Arrange
        var responseDto = new RejectRequestResponse("Rejected");
        var json = JsonSerializer.Serialize(responseDto);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.RejectRequestAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be("Rejected");
    }

    [Fact]
    public async Task Given_Error_When_RejectRequestAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"error\": \"Cannot reject\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.RejectRequestAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot reject");
    }

    #endregion

    #region GetRelationshipsAsync Tests

    [Fact]
    public async Task When_GetRelationshipsAsyncIsCalled_Then_ReturnsList()
    {
        // Arrange
        var list = new List<RoommateRelationshipDto> 
        { 
            new RoommateRelationshipDto(
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                "User1", 
                "user1@test.com", 
                Guid.NewGuid(), 
                "User2", 
                "user2@test.com", 
                "Admin",
                DateTime.Now,
                true
            ) 
        };
        var json = JsonSerializer.Serialize(list);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetRelationshipsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_NullResponse_When_GetRelationshipsAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.GetRelationshipsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region DeleteRelationshipAsync Tests

    [Fact]
    public async Task Given_ValidDeletion_When_DeleteRelationshipAsyncIsCalled_Then_ReturnsResponse()
    {
        // Arrange
        var responseDto = new DeleteRelationshipResponse("Deleted");
        var json = JsonSerializer.Serialize(responseDto);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        var result = await service.DeleteRelationshipAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be("Deleted");
    }

    [Fact]
    public async Task Given_Error_When_DeleteRelationshipAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("{\"error\": \"Cannot delete\"}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new RoommateService(httpClient);

        // Act
        Func<Task> act = () => service.DeleteRelationshipAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete");
    }

    #endregion
}

