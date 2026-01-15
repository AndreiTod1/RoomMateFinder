using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using System.Net;
using System.Text.Json;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ProfileServiceTests
{
    private static ProfileDto CreateTestProfile()
    {
        return new ProfileDto(
            Guid.NewGuid(), "test@test.com", "Test User", 25, "Male", "Uni", "Bio", "Quiet", "Music",
            DateTime.UtcNow, "/img/pic.jpg", "User"
        );
    }

    #region GetAdminsAsync Tests

    [Fact]
    public async Task Given_ValidRequest_When_GetAdminsAsyncIsCalled_Then_ReturnsListOfAdmins()
    {
        // Arrange
        var admins = new List<ProfileDto> { CreateTestProfile() };
        var json = JsonSerializer.Serialize(admins);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAdminsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task Given_NullResponse_When_GetAdminsAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAdminsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task Given_ProfilesExist_When_GetAllAsyncIsCalled_Then_ReturnsProfiles()
    {
        // Arrange
        var profiles = new List<ProfileDto> { CreateTestProfile(), CreateTestProfile() };
        var json = JsonSerializer.Serialize(profiles);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_NullResponse_When_GetAllAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task Given_ValidId_When_GetByIdAsyncIsCalled_Then_ReturnsProfile()
    {
        // Arrange
        var profile = CreateTestProfile();
        var json = JsonSerializer.Serialize(profile);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetByIdAsync(profile.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profile.Id);
    }

    [Fact]
    public async Task Given_NotFoundId_When_GetByIdAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.NotFound);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_Unauthorized_When_GetByIdAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_ServerError_When_GetByIdAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Server error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Server error");
    }

    [Fact]
    public async Task Given_EmptyErrorMessage_When_GetByIdAsyncFails_Then_ThrowsDefaultMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to get profile");
    }

    [Fact]
    public async Task Given_NetworkError_When_GetByIdAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCurrentAsync Tests

    [Fact]
    public async Task Given_ValidUser_When_GetCurrentAsyncIsCalled_Then_ReturnsProfile()
    {
        // Arrange
        var profile = CreateTestProfile();
        var json = JsonSerializer.Serialize(profile);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetCurrentAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Given_Unauthorized_When_GetCurrentAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.GetCurrentAsync();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_NotFound_When_GetCurrentAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.NotFound);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetCurrentAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ServerError_When_GetCurrentAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error message", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.GetCurrentAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Error message");
    }

    [Fact]
    public async Task Given_NetworkError_When_GetCurrentAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetCurrentAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task Given_ValidRequest_When_GetAllUsersAsyncIsCalled_Then_ReturnsPaginatedResponse()
    {
        // Arrange
        var users = new List<UserDto> { new UserDto(Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", null, DateTime.UtcNow, "User") };
        var response = new PaginatedUsersResponse(users, 1, 1, 10);
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAllUsersAsync(1, 10, null);

        // Assert
        result.Users.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_SearchQuery_When_GetAllUsersAsyncIsCalled_Then_IncludesSearchInUrl()
    {
        // Arrange
        var response = new PaginatedUsersResponse(new List<UserDto>(), 0, 1, 10);
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAllUsersAsync(1, 10, "test");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_NullResponse_When_GetAllUsersAsyncIsCalled_Then_ReturnsEmptyResponse()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetAllUsersAsync(1, 10, null);

        // Assert
        result.Users.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region DeleteProfileAsync Tests

    [Fact]
    public async Task Given_ValidId_When_DeleteProfileAsyncIsCalled_Then_Succeeds()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        // Act
        var act = async () => await service.DeleteProfileAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_DeleteProfile_When_ApiFails_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Delete failed", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.DeleteProfileAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Delete failed*");
    }

    #endregion

    #region UpdateRoleAsync Tests

    [Fact]
    public async Task Given_ValidRole_When_UpdateRoleAsyncIsCalled_Then_Succeeds()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        // Act
        var act = async () => await service.UpdateRoleAsync(Guid.NewGuid(), "Admin");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_FailedUpdate_When_UpdateRoleAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Update role failed", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        // Act
        Func<Task> act = () => service.UpdateRoleAsync(Guid.NewGuid(), "Admin");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Update role failed*");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task Given_ValidUpdate_When_UpdateAsyncIsCalled_Then_ReturnsUpdatedProfile()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var updatedProfile = CreateTestProfile();
        var json = JsonSerializer.Serialize(updatedProfile);
        
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((request) => 
        {
            request.Content.Should().BeOfType<MultipartFormDataContent>();
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });
        
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        var updateDto = new UpdateProfileRequestDto("New Name", 26, "F", "Uni", "Bio", "Social", "Sports");

        // Act
        var result = await service.UpdateAsync(profileId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task Given_Unauthorized_When_UpdateAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        var updateDto = new UpdateProfileRequestDto("New Name", null, null, null, null, null, null);

        // Act
        Func<Task> act = () => service.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_Forbidden_When_UpdateAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Forbidden);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        var updateDto = new UpdateProfileRequestDto("New Name", null, null, null, null, null, null);

        // Act
        Func<Task> act = () => service.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Forbidden");
    }

    [Fact]
    public async Task Given_SuccessWithEmptyBody_When_UpdateAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        var updateDto = new UpdateProfileRequestDto("New Name", null, null, null, null, null, null);

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_BadRequest_When_UpdateAsyncIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Validation error", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        var updateDto = new UpdateProfileRequestDto("New Name", null, null, null, null, null, null);

        // Act
        Func<Task> act = () => service.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Validation error");
    }

    [Fact]
    public async Task Given_EmptyBody_When_UpdateAsyncFails_Then_ThrowsDefaultMessage()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
        var service = new ProfileService(httpClient);

        var updateDto = new UpdateProfileRequestDto("New Name", null, null, null, null, null, null);

        // Act
        Func<Task> act = () => service.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Update failed");
    }

    #endregion

    #region GetUserReviews Tests

    [Fact]
    public async Task Given_ValidUserId_When_GetUserReviewsIsCalled_Then_ReturnsReviews()
    {
        // Arrange
        var reviews = new List<Review> 
        { 
            new Review { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "Reviewer", Rating = 5, Comment = "Great", CreatedAt = DateTime.UtcNow }
        };
        var response = new GetUserReviewsResponse { Reviews = reviews };
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetUserReviews(Guid.NewGuid());

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_NullResponse_When_GetUserReviewsIsCalled_Then_ReturnsEmpty()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("null");
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetUserReviews(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_Exception_When_GetUserReviewsIsCalled_Then_ReturnsEmpty()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new Exception());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ProfileService(httpClient);

        // Act
        var result = await service.GetUserReviews(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}

