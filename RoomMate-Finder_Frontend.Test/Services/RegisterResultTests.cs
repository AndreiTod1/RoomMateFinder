using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

/// <summary>
/// Tests for RegisterResult to increase coverage.
/// </summary>
public class RegisterResultTests
{
    [Fact]
    public void Given_NewRegisterResult_When_Successful_Then_PropertiesAreCorrect()
    {
        // Arrange & Act
        var result = new RegisterResult { Successful = true };

        // Assert
        result.Successful.Should().BeTrue();
        result.Errors.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Given_NewRegisterResult_When_Failed_Then_ErrorsAreStored()
    {
        // Arrange & Act
        var result = new RegisterResult 
        { 
            Successful = false, 
            Errors = new[] { "Error 1", "Error 2" } 
        };

        // Assert
        result.Successful.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void Given_RegisterResult_When_EmptyErrors_Then_ArrayIsEmpty()
    {
        // Arrange & Act
        var result = new RegisterResult 
        { 
            Successful = false, 
            Errors = Array.Empty<string>() 
        };

        // Assert
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_RegisterResult_When_NullErrors_Then_ReturnsNull()
    {
        // Arrange & Act
        var result = new RegisterResult 
        { 
            Successful = true, 
            Errors = null 
        };

        // Assert
        result.Errors.Should().BeNull();
    }
}
