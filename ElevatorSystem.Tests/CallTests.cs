using ElevatorSystem.Models;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for HallCall and CabCall models.
/// </summary>
public class CallTests
{
    [Fact]
    public void HallCall_ValidFloorAndDirection_CreatesSuccessfully()
    {
        // Act
        var call = new HallCall(5, ElevatorDirection.Up);

        // Assert
        Assert.Equal(5, call.Floor);
        Assert.Equal(ElevatorDirection.Up, call.Direction);
        Assert.True(call.IsValid);
        Assert.Null(call.AssignedElevatorId);
        Assert.False(call.IsServiced);
    }

    [Fact]
    public void HallCall_InvalidFloor_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new HallCall(0, ElevatorDirection.Up));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HallCall(11, ElevatorDirection.Up));
    }

    [Fact]
    public void HallCall_IdleDirection_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HallCall(5, ElevatorDirection.Idle));
    }

    [Fact]
    public void HallCall_BottomFloorDown_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HallCall(1, ElevatorDirection.Down));
    }

    [Fact]
    public void HallCall_TopFloorUp_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HallCall(10, ElevatorDirection.Up));
    }

    [Fact]
    public void HallCall_AssignTo_SetsElevatorId()
    {
        // Arrange
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        call.AssignTo(2);

        // Assert
        Assert.Equal(2, call.AssignedElevatorId);
    }

    [Fact]
    public void HallCall_MarkServiced_SetsFlag()
    {
        // Arrange
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        call.MarkServiced();

        // Assert
        Assert.True(call.IsServiced);
    }

    [Fact]
    public void CabCall_ValidFloor_CreatesSuccessfully()
    {
        // Act
        var call = new CabCall(5);

        // Assert
        Assert.Equal(5, call.DestinationFloor);
        Assert.False(call.IsServiced);
    }

    [Fact]
    public void CabCall_InvalidFloor_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new CabCall(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CabCall(11));
    }

    [Fact]
    public void CabCall_MarkServiced_SetsFlag()
    {
        // Arrange
        var call = new CabCall(5);

        // Act
        call.MarkServiced();

        // Assert
        Assert.True(call.IsServiced);
    }
}
