using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Services;

public class AuthTokenHandlerTests
{
    private readonly Mock<IJSRuntime> _mockJs;
    private readonly AuthTokenHandler _handler;

    public AuthTokenHandlerTests()
    {
        _mockJs = new Mock<IJSRuntime>();
        _handler = new AuthTokenHandler(_mockJs.Object);
    }

    private class TestInnerHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task SendAsync_WithToken_AddsAuthorizationHeader()
    {
        // Arrange
        var token = "test-jwt-token";
        _mockJs.Setup(j => j.InvokeAsync<string>("localStorage.getItem", It.Is<object[]>(args => args[0].Equals("authToken"))))
               .ReturnsAsync(token);

        var innerHandler = new TestInnerHandler();
        _handler.InnerHandler = innerHandler;

        var invoker = new HttpMessageInvoker(_handler);

        // Act
        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com"), CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        innerHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        innerHandler.LastRequest.Headers.Authorization!.Parameter.Should().Be(token);
    }

    [Fact]
    public async Task SendAsync_NoToken_DoesNotAddHeader()
    {
        // Arrange
        _mockJs.Setup(j => j.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>())).ReturnsAsync((string?)null);

        var innerHandler = new TestInnerHandler();
        _handler.InnerHandler = innerHandler;
        var invoker = new HttpMessageInvoker(_handler);

        // Act
        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com"), CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_JsException_DoesNotAddHeaderAndProceeds()
    {
        // Arrange
        _mockJs.Setup(j => j.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
               .ThrowsAsync(new InvalidOperationException("JS Interop failed"));

        var innerHandler = new TestInnerHandler();
        _handler.InnerHandler = innerHandler;
        var invoker = new HttpMessageInvoker(_handler);

        // Act
        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com"), CancellationToken.None);

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }
}
