using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Services.Internals;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Services.Internals;

public class SignalRHubConnectionTests
{
    private SignalRHubConnection CreateConnection()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/dummy")
            .Build();
        return new SignalRHubConnection(hubConnection);
    }

    #region Factory Tests

    [Fact]
    public void HubConnectionFactory_CreateConnection_ReturnsSignalRHubConnection()
    {
        // Arrange
        var factory = new HubConnectionFactory();

        // Act
        var result = factory.CreateConnection("http://localhost/hub", "token");

        // Assert
        result.Should().BeOfType<SignalRHubConnection>();
        result.Should().NotBeNull();
    }

    [Fact]
    public void HubConnectionFactory_CreateConnection_WithDifferentUrl_ReturnsNewInstance()
    {
        // Arrange
        var factory = new HubConnectionFactory();

        // Act
        var conn1 = factory.CreateConnection("http://localhost/hub1", "token1");
        var conn2 = factory.CreateConnection("http://localhost/hub2", "token2");

        // Assert
        conn1.Should().NotBeSameAs(conn2);
    }

    #endregion

    #region State Tests

    [Fact]
    public void SignalRHubConnection_State_ReturnsDisconnected_Initially()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Assert
        wrapper.State.Should().Be(HubConnectionState.Disconnected);
    }

    [Fact]
    public void SignalRHubConnection_State_CanBeAccessed()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act & Assert
        var state = wrapper.State;
        state.Should().BeOneOf(
            HubConnectionState.Disconnected,
            HubConnectionState.Connected,
            HubConnectionState.Connecting,
            HubConnectionState.Reconnecting
        );
    }

    #endregion

    #region StartAsync Tests

    [Fact]
    public async Task SignalRHubConnection_StartAsync_ThrowsWhenNoServer()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        Func<Task> act = () => wrapper.StartAsync(CancellationToken.None);

        // Assert - Can't connect to dummy URL, but the method is called
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SignalRHubConnection_StartAsync_WithCancellation_ThrowsWhenCancelled()
    {
        // Arrange
        var wrapper = CreateConnection();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = () => wrapper.StartAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region StopAsync Tests

    [Fact]
    public async Task SignalRHubConnection_StopAsync_CanBeCalled()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act - Stop on disconnected connection should work
        await wrapper.StopAsync();

        // Assert
        wrapper.State.Should().Be(HubConnectionState.Disconnected);
    }

    [Fact]
    public async Task SignalRHubConnection_StopAsync_WithCancellation_Works()
    {
        // Arrange
        var wrapper = CreateConnection();
        var cts = new CancellationTokenSource();

        // Act
        await wrapper.StopAsync(cts.Token);

        // Assert
        wrapper.State.Should().Be(HubConnectionState.Disconnected);
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task SignalRHubConnection_InvokeAsync_NoArgs_ThrowsWhenDisconnected()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        Func<Task> act = () => wrapper.InvokeAsync("TestMethod");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SignalRHubConnection_InvokeAsync_OneArg_ThrowsWhenDisconnected()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        Func<Task> act = () => wrapper.InvokeAsync("TestMethod", "arg1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SignalRHubConnection_InvokeAsync_TwoArgs_ThrowsWhenDisconnected()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        Func<Task> act = () => wrapper.InvokeAsync("TestMethod", "arg1", "arg2");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region On Handler Tests

    [Fact]
    public void SignalRHubConnection_On_SingleParam_RegistersHandler()
    {
        // Arrange
        var wrapper = CreateConnection();
        string received = "";

        // Act
        var subscription = wrapper.On<string>("TestEvent", msg => received = msg);

        // Assert
        subscription.Should().NotBeNull();
    }

    [Fact]
    public void SignalRHubConnection_On_TwoParams_RegistersHandler()
    {
        // Arrange
        var wrapper = CreateConnection();
        string msg1 = "", msg2 = "";

        // Act
        var subscription = wrapper.On<string, string>("TestEvent", (a, b) => { msg1 = a; msg2 = b; });

        // Assert
        subscription.Should().NotBeNull();
    }

    [Fact]
    public void SignalRHubConnection_On_ReturnsDisposable()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        var subscription = wrapper.On<string>("Event", _ => { });

        // Assert - Can dispose without error
        subscription.Dispose();
    }

    [Fact]
    public void SignalRHubConnection_On_MultipleHandlers_AllRegistered()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        var sub1 = wrapper.On<string>("Event1", _ => { });
        var sub2 = wrapper.On<string>("Event2", _ => { });
        var sub3 = wrapper.On<int, string>("Event3", (_, _) => { });

        // Assert
        sub1.Should().NotBeNull();
        sub2.Should().NotBeNull();
        sub3.Should().NotBeNull();
    }

    #endregion

    #region DisposeAsync Tests

    [Fact]
    public async Task SignalRHubConnection_DisposeAsync_CanBeCalled()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act & Assert - Should not throw
        await wrapper.DisposeAsync();
    }

    [Fact]
    public async Task SignalRHubConnection_DisposeAsync_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var wrapper = CreateConnection();

        // Act
        await wrapper.DisposeAsync();
        await wrapper.DisposeAsync();

        // Assert - No exception
    }

    #endregion
}

