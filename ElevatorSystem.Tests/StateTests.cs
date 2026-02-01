using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.States;
using ElevatorSystem.Models;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the State Machine (State Pattern).
/// </summary>
public class StateTests
{
    private readonly IEventBus _eventBus = new EventBus();

    [Fact]
    public void IdleState_NoRequests_StaysIdle()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var context = (IElevatorContext)elevator;
        var state = IdleState.Instance;

        // Act
        var nextState = state.ProcessAsync(context, CancellationToken.None).Result;

        // Assert
        Assert.Same(IdleState.Instance, nextState);
    }

    [Fact]
    public void IdleState_WithRequest_TransitionsToMoving()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        elevator.AddCabCall(5); // Request above current floor
        var context = (IElevatorContext)elevator;
        var state = IdleState.Instance;

        // Act
        var nextState = state.ProcessAsync(context, CancellationToken.None).Result;

        // Assert
        Assert.IsType<MovingState>(nextState);
    }

    [Fact]
    public void IdleState_WithRequestAtCurrentFloor_TransitionsToDoorsOpen()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var call = new HallCall(1, ElevatorDirection.Up);
        elevator.AssignHallCall(call);
        var context = (IElevatorContext)elevator;
        var state = IdleState.Instance;

        // Act
        var nextState = state.ProcessAsync(context, CancellationToken.None).Result;

        // Assert
        Assert.IsType<DoorsOpenState>(nextState);
    }

    [Fact]
    public void DoorsClosingState_NoRequests_TransitionsToIdle()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        var context = (IElevatorContext)elevator;
        var state = DoorsClosingState.Instance;

        // Act
        var nextState = state.ProcessAsync(context, CancellationToken.None).Result;

        // Assert
        Assert.Same(IdleState.Instance, nextState);
    }

    [Fact]
    public void DoorsClosingState_WithRequests_TransitionsToMoving()
    {
        // Arrange
        var elevator = new Elevator(1, _eventBus);
        elevator.AddCabCall(5);
        ((IElevatorContext)elevator).SetDirection(ElevatorDirection.Up);
        var context = (IElevatorContext)elevator;
        var state = DoorsClosingState.Instance;

        // Act
        var nextState = state.ProcessAsync(context, CancellationToken.None).Result;

        // Assert
        Assert.IsType<MovingState>(nextState);
    }
}
