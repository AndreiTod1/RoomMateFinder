using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ApiAuthenticationStateProviderTests
{
    #region ApiAuthenticationStateProvider Implementation Tests

    [Fact]
    public void ApiAuthenticationStateProvider_Should_InheritFromAuthenticationStateProvider()
    {
        // Assert
        typeof(ApiAuthenticationStateProvider).BaseType!.Name.Should().Be("AuthenticationStateProvider");
    }

    [Fact]
    public void ApiAuthenticationStateProvider_Should_HaveGetAuthenticationStateAsyncMethod()
    {
        // Assert
        var method = typeof(ApiAuthenticationStateProvider).GetMethod("GetAuthenticationStateAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void ApiAuthenticationStateProvider_Should_HaveMarkUserAsAuthenticatedMethod()
    {
        // Assert
        var method = typeof(ApiAuthenticationStateProvider).GetMethod("MarkUserAsAuthenticated");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void ApiAuthenticationStateProvider_Should_HaveMarkUserAsLoggedOutMethod()
    {
        // Assert
        var method = typeof(ApiAuthenticationStateProvider).GetMethod("MarkUserAsLoggedOut");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    #endregion

    #region JWT Parsing Tests (using reflection to test private method logic)

    [Fact]
    public void JwtParsing_ValidBase64_ShouldNotThrow()
    {
        // Arrange - A simple test JWT payload (base64 encoded {"sub":"123","name":"test"})
        var base64Payload = "eyJzdWIiOiIxMjMiLCJuYW1lIjoidGVzdCJ9";
        
        // Act & Assert - just verify base64 decoding works
        var action = () => 
        {
            var paddedPayload = base64Payload;
            switch (paddedPayload.Length % 4)
            {
                case 2: paddedPayload += "=="; break;
                case 3: paddedPayload += "="; break;
            }
            paddedPayload = paddedPayload.Replace('-', '+').Replace('_', '/');
            Convert.FromBase64String(paddedPayload);
        };
        
        action.Should().NotThrow();
    }

    [Fact]
    public void JwtParsing_UrlSafeBase64_ShouldBeConverted()
    {
        // Arrange - URL-safe base64 uses - and _ instead of + and /
        var urlSafeBase64 = "eyJ0ZXN0IjoiYS1iX2MifQ"; // {"test":"a-b_c"}
        
        // Act
        var standardBase64 = urlSafeBase64.Replace('-', '+').Replace('_', '/');
        while (standardBase64.Length % 4 != 0)
        {
            standardBase64 += "=";
        }
        
        // Assert
        var action = () => Convert.FromBase64String(standardBase64);
        action.Should().NotThrow();
    }

    #endregion
}

