using FluentAssertions;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Test.Helpers;
using System.Net;
using System.Text.Json;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ConversationServiceTests
{
    #region GetConversationsAsync Tests

    [Fact]
    public async Task Given_ValidRequest_When_GetConversationsAsyncIsCalled_Then_ReturnsConversations()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User1", "/pic.jpg", "User", DateTime.UtcNow)
        };
        var response = new { Conversations = conversations };
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetConversationsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_Unauthorized_When_GetConversationsAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        Func<Task> act = () => service.GetConversationsAsync();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_ServerError_When_GetConversationsAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetConversationsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_NetworkError_When_GetConversationsAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetConversationsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMessagesAsync Tests

    [Fact]
    public async Task Given_ValidConversation_When_GetMessagesAsyncIsCalled_Then_ReturnsMessages()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new MessageDto(Guid.NewGuid(), Guid.NewGuid(), "Sender", "User", "Hello", DateTime.UtcNow, false)
        };
        var response = new { Messages = messages };
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetMessagesAsync(Guid.NewGuid());

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_Unauthorized_When_GetMessagesAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        Func<Task> act = () => service.GetMessagesAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_ServerError_When_GetMessagesAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetMessagesAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_NetworkError_When_GetMessagesAsyncIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetMessagesAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region StartConversationAsync Tests

    [Fact]
    public async Task Given_ValidUser_When_StartConversationAsyncIsCalled_Then_ReturnsConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var response = new { ConversationId = conversationId };
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.StartConversationAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(conversationId);
    }

    [Fact]
    public async Task Given_Unauthorized_When_StartConversationAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        Func<Task> act = () => service.StartConversationAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_ServerError_When_StartConversationAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.StartConversationAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_NetworkError_When_StartConversationAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.StartConversationAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SendMessageAsync Tests

    [Fact]
    public async Task Given_ValidMessage_When_SendMessageAsyncIsCalled_Then_ReturnsMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var response = new { MessageId = messageId };
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.SendMessageAsync(Guid.NewGuid(), "Hello");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(messageId);
    }

    [Fact]
    public async Task Given_Unauthorized_When_SendMessageAsyncIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        Func<Task> act = () => service.SendMessageAsync(Guid.NewGuid(), "Hello");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_ServerError_When_SendMessageAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.SendMessageAsync(Guid.NewGuid(), "Hello");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_NetworkError_When_SendMessageAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.SendMessageAsync(Guid.NewGuid(), "Hello");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region MarkMessagesAsReadAsync Tests

    [Fact]
    public async Task Given_ValidConversation_When_MarkMessagesAsReadAsyncIsCalled_Then_Completes()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var act = async () => await service.MarkMessagesAsReadAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_NetworkError_When_MarkMessagesAsReadAsyncIsCalled_Then_SilentlyFails()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var act = async () => await service.MarkMessagesAsReadAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetUnreadConversationsAsync Tests

    [Fact]
    public async Task Given_ValidRequest_When_GetUnreadConversationsAsyncIsCalled_Then_ReturnsResponse()
    {
        // Arrange
        var unreadConvos = new List<UnreadConversationDto>
        {
            new UnreadConversationDto(Guid.NewGuid(), 5)
        };
        var response = new UnreadConversationsResponse(unreadConvos, 5);
        var json = JsonSerializer.Serialize(response);
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler(json);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetUnreadConversationsAsync();

        // Assert
        result.Should().NotBeNull();
        result!.TotalUnreadMessages.Should().Be(5);
    }

    [Fact]
    public async Task Given_Unauthorized_When_GetUnreadConversationsAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetUnreadConversationsAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ServerError_When_GetUnreadConversationsAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetUnreadConversationsAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_NetworkError_When_GetUnreadConversationsAsyncIsCalled_Then_ReturnsNull()
    {
        // Arrange
        var httpHandler = TestHelpers.CreateMockHttpMessageHandler((_) => throw new HttpRequestException());
        var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };

        var service = new ConversationService(httpClient);

        // Act
        var result = await service.GetUnreadConversationsAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
