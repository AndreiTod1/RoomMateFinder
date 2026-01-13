using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class AuthServiceDtoTests
{
    #region IAuthService Interface Contract Tests

    [Fact]
    public void IAuthService_Interface_ShouldExist()
    {
        // Assert
        typeof(IAuthService).Should().NotBeNull();
        typeof(IAuthService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IAuthService_Should_HaveLoginAsyncMethod()
    {
        // Assert
        var method = typeof(IAuthService).GetMethod("LoginAsync");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void IAuthService_Should_HaveLogoutAsyncMethod()
    {
        // Assert
        var method = typeof(IAuthService).GetMethod("LogoutAsync");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void IAuthService_Should_HaveGetTokenAsyncMethod()
    {
        // Assert
        var method = typeof(IAuthService).GetMethod("GetTokenAsync");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<string?>));
    }

    #endregion
}

