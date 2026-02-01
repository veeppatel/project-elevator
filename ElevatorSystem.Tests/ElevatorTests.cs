using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.States;
using ElevatorSystem.Models;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the Elevator class with State Pattern.
/// </summary>
public class ElevatorTests
{
    private readonly IEventBus _eventBus = new EventBus();

    [Fact]
    public void NewElevator_StartsAtFloor1_InIdleState()
    {
        // Arrange & Act
        var elevator = new Elevator(1, _eventBus);

        // Assert
        Assert.Equal(1, elevator.Id);
        Assert.Equal(1, elevator.CurrentFloor);
        Assert.Equal(ElevatorDirection.Idle, elevator.Direction);
        Assert.Equal(ElevatorStateType.Idle, elevator.StateType);
        Assert.False(elevator.HasPendingRequests);
    }

    [Fact]
    public void AssignHallCall_AddsToAssignedCalls()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        elevator.AssignHallCall(call);

        // Assert
        Assert.True(elevator.HasPendingRequests);
        Assert.Single(elevator.AssignedHallCalls);
        Assert.Equal(5, elevator.AssignedHallCalls[0].Floor);
    }

    [Fact]
    public void AddCabCall_AddsToDestinations()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);

        // Act
        elevator.AddCabCall(5);

        // Assert
        Assert.True(elevator.HasPendingRequests);
        Assert.Contains(5, elevator.CabCallFloors);
    }

    [Fact]
    public void AddCabCall_InvalidFloor_IsIgnored()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);

        // Act
        elevator.AddCabCall(0);
        elevator.AddCabCall(11);

        // Assert
        Assert.False(elevator.HasPendingRequests);
    }

    [Fact]
    public void GetStopsInDirection_ReturnsCorrectFloors()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        elevator.AddCabCall(3);
        elevator.AddCabCall(5);
        elevator.AddCabCall(7);

        // Act - Elevator is at floor 1, should get all above
        var upStops = ((IElevatorContext)elevator).GetStopsInDirection(ElevatorDirection.Up).ToList();

        // Assert
        Assert.Equal(3, upStops.Count);
        Assert.Equal(new[] { 3, 5, 7 }, upStops);
    }

    [Fact]
    public void ShouldStopAtCurrentFloor_TrueForCabCall()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        elevator.AddCabCall(1); // Same as current floor

        // Act
        var shouldStop = ((IElevatorContext)elevator).ShouldStopAtCurrentFloor();

        // Assert
        Assert.True(shouldStop);
    }

    [Fact]
    public void ShouldStopAtCurrentFloor_TrueForHallCall()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var call = new HallCall(1, ElevatorDirection.Up);
        elevator.AssignHallCall(call);
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Up);

        // Act
        var shouldStop = ((IElevatorContext)elevator).ShouldStopAtCurrentFloor();

        // Assert
        Assert.True(shouldStop);
    }

    [Fact]
    public void MoveOneFloor_MovesUp_WhenDirectionUp()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Up);

        // Act
        ((IElevatorContext)elevator).MoveOneFloor();

        // Assert
        Assert.Equal(2, elevator.CurrentFloor);
    }

    [Fact]
    public void MoveOneFloor_MovesDown_WhenDirectionDown()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Up);
        ((IElevatorContext)elevator).MoveOneFloor(); // Go to floor 2
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Down);

        // Act
        ((IElevatorContext)elevator).MoveOneFloor();

        // Assert
        Assert.Equal(1, elevator.CurrentFloor);
    }

    [Fact]
    public void ServiceCurrentFloor_RemovesCabCall()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        elevator.AddCabCall(1);

        // Act
        ((IElevatorContext)elevator).ServiceCurrentFloor();

        // Assert
        Assert.False(elevator.CabCallFloors.Contains(1));
    }

    [Fact]
    public void ServiceCurrentFloor_MarksHallCallAsServiced()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var call = new HallCall(1, ElevatorDirection.Up);
        elevator.AssignHallCall(call);
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Up);

        // Act
        ((IElevatorContext)elevator).ServiceCurrentFloor();

        // Assert
        Assert.True(call.IsServiced);
        Assert.Empty(elevator.AssignedHallCalls); // Serviced calls are removed
    }

    [Fact]
    public void CalculateSuitabilityScore_IdleElevator_ReturnsLowScore()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        int score = elevator.CalculateSuitabilityScore(call);

        // Assert - Distance is 4, so score should be 4*10 - 5 = 35
        Assert.Equal(35, score);
    }

    [Fact]
    public void CalculateSuitabilityScore_MovingToward_ReturnsDistanceScore()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        elevator.AddCabCall(10); // Going up
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Up);
        
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        int score = elevator.CalculateSuitabilityScore(call);

        // Assert - Distance is 4, same direction = 4*10 = 40
        Assert.Equal(40, score);
    }
}
