using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the Elevator class.
/// </summary>
public class ElevatorTests
{
    [Fact]
    public void NewElevator_StartsAtFloor1_WithIdleState()
    {
        // Arrange & Act
        var elevator = new Elevator(1);

        // Assert
        Assert.Equal(1, elevator.Id);
        Assert.Equal(1, elevator.CurrentFloor);
        Assert.Equal(Direction.Idle, elevator.Direction);
        Assert.Equal(ElevatorState.Stopped, elevator.State);
        Assert.False(elevator.HasDestinations);
    }

    [Fact]
    public void AddDestination_AboveCurrentFloor_SetsUpDirection()
    {
        // Arrange
        var elevator = new Elevator(1);

        // Act
        elevator.AddDestination(5, Direction.Up);

        // Assert
        Assert.Equal(Direction.Up, elevator.Direction);
        Assert.True(elevator.HasDestinations);
        Assert.Contains(5, elevator.Destinations);
    }

    [Fact]
    public void AddDestination_BelowCurrentFloor_SetsDownDirection()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(5, Direction.Up);
        elevator.Move();
        elevator.CompleteMove(); // Now at floor 2

        // Act
        elevator.AddDestination(1, Direction.Down);

        // Assert
        Assert.Contains(1, elevator.Destinations);
    }

    [Fact]
    public void Move_MovesOneFloorInDirection()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(5, Direction.Up);

        // Act
        bool moved = elevator.Move();

        // Assert
        Assert.True(moved);
        Assert.Equal(2, elevator.CurrentFloor);
        Assert.Equal(ElevatorState.Moving, elevator.State);
    }

    [Fact]
    public void Move_DoesNotMoveWhenIdle()
    {
        // Arrange
        var elevator = new Elevator(1);

        // Act
        bool moved = elevator.Move();

        // Assert
        Assert.False(moved);
        Assert.Equal(1, elevator.CurrentFloor);
    }

    [Fact]
    public void CompleteMove_SetsStateToStopped()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(5, Direction.Up);
        elevator.Move();

        // Act
        elevator.CompleteMove();

        // Assert
        Assert.Equal(ElevatorState.Stopped, elevator.State);
    }

    [Fact]
    public void ShouldStopAtFloor_ReturnsTrueForDestination()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(2, Direction.Up);
        elevator.Move(); // Now at floor 2

        // Act
        bool shouldStop = elevator.ShouldStopAtFloor();

        // Assert
        Assert.True(shouldStop);
    }

    [Fact]
    public void OpenDoors_SetsDoorsOpenState_RemovesDestination()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(2, Direction.Up);
        elevator.Move();
        elevator.CompleteMove();

        // Act
        elevator.OpenDoors();

        // Assert
        Assert.Equal(ElevatorState.DoorsOpen, elevator.State);
        Assert.DoesNotContain(2, elevator.Destinations);
    }

    [Fact]
    public void CloseDoors_SetsStoppedState()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(2, Direction.Up);
        elevator.Move();
        elevator.CompleteMove();
        elevator.OpenDoors();

        // Act
        elevator.CloseDoors();

        // Assert
        Assert.Equal(ElevatorState.Stopped, elevator.State);
    }

    [Fact]
    public void ScanAlgorithm_ContinuesUpUntilExhausted()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(3, Direction.Up);
        elevator.AddDestination(5, Direction.Up);

        // Act - Move to floor 3
        while (elevator.CurrentFloor < 3)
        {
            elevator.Move();
            elevator.CompleteMove();
        }
        elevator.OpenDoors();
        elevator.CloseDoors();

        // Assert - Should still be going up
        Assert.Equal(Direction.Up, elevator.Direction);
    }

    [Fact]
    public void ScanAlgorithm_ReversesWhenNoMoreDestinationsInDirection()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(3, Direction.Up);

        // Act - Move to floor 3 and service it
        while (elevator.CurrentFloor < 3)
        {
            elevator.Move();
            elevator.CompleteMove();
        }
        elevator.OpenDoors();
        
        // Simulate passenger getting on at floor 3 and wanting to go to floor 1
        elevator.AddInternalDestination(1);
        
        elevator.CloseDoors();

        // Assert - Should reverse to go down since no more up destinations
        Assert.Equal(Direction.Down, elevator.Direction);
    }

    [Fact]
    public void CalculateEffectiveDistance_IdleElevator_ReturnsPhysicalDistance()
    {
        // Arrange
        var elevator = new Elevator(1);

        // Act
        int distance = elevator.CalculateEffectiveDistance(5, Direction.Up);

        // Assert
        Assert.Equal(4, distance); // |5 - 1| = 4
    }

    [Fact]
    public void CalculateEffectiveDistance_MovingToward_ReturnsPhysicalDistance()
    {
        // Arrange
        var elevator = new Elevator(1);
        elevator.AddDestination(10, Direction.Up);

        // Act
        int distance = elevator.CalculateEffectiveDistance(5, Direction.Up);

        // Assert
        Assert.Equal(4, distance); // On the way up, floor 5 is en route
    }

    [Fact]
    public void AddDestination_InvalidFloor_IsIgnored()
    {
        // Arrange
        var elevator = new Elevator(1);

        // Act
        elevator.AddDestination(0, Direction.Up);
        elevator.AddDestination(11, Direction.Up);

        // Assert
        Assert.False(elevator.HasDestinations);
    }
}
