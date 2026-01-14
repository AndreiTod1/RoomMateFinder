using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Services.Internals;
using System.Reflection;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Services.Internals;

public class SignalRHubConnectionTests
{
    // HubConnectionFactory Tests
    [Fact]
    public void HubConnectionFactory_CreateConnection_ReturnsSignalRHubConnection()
    {
        // Arrange
        var factory = new HubConnectionFactory();
        var url = "http://localhost/hub";
        var token = "token";

        // Act
        var result = factory.CreateConnection(url, token);

        // Assert
        result.Should().BeOfType<SignalRHubConnection>();
        result.Should().NotBeNull();
    }

    // SignalRHubConnection Tests
    // Since HubConnection is hard to mock, we might only be able to test basic things check constructor.
    // However, if we can mock HubConnection, we can verify delegation.
    // HubConnection methods are virtual in recent versions? Let's try.
    // If this fails compile or runtime, we will know.
    
    // NOTE: HubConnection constructor is complex. We might not be able to instantiate a Mock<HubConnection> easily.
    // We will skip deep mocking if it defeats the purpose. 
    // Just testing the factory covers 50% of the 0% files.

    [Fact]
    public void SignalRHubConnection_State_ReturnsUnderlyingState()
    {
        // We cannot easily create a HubConnection without a Builder.
        // And even then, it's a real object.
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/dummy")
            .Build();

        var wrapper = new SignalRHubConnection(hubConnection);

        // Assert initial state
        wrapper.State.Should().Be(HubConnectionState.Disconnected);
    }
}
