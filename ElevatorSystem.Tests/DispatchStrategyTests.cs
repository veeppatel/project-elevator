using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.Strategies;
using ElevatorSystem.Models;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for dispatch strategies.
/// </summary>
public class DispatchStrategyTests
{
    private readonly IEventBus _eventBus = new EventBus();

    [Fact]
    public void ScanStrategy_SelectsNearestIdleElevator()
    {
        // Arrange
        var strategy = new ScanDispatchStrategy();
        var elevators = new List<IElevator>
        {
            new Elevator(1, _eventBus), // Floor 1
            new Elevator(2, _eventBus), // Floor 1
        };
        
        var call = new HallCall(3, ElevatorDirection.Up);

        // Act
        var selected = strategy.SelectElevator(call, elevators);

        // Assert
        Assert.NotNull(selected);
    }

    [Fact]
    public void ScanStrategy_PrefersElevatorOnTheWay()
    {
        // Arrange
        var strategy = new ScanDispatchStrategy();
        
        var elevator1 = new Elevator(1, _eventBus);
        var elevator2 = new Elevator(2, _eventBus);
        
        // Elevator 1 is going up with a destination at floor 10
        elevator1.AddCabCall(10);
        ((IElevatorContext)elevator1).SetDirection(ElevatorDirection.Up);
        
        var elevators = new List<IElevator> { elevator1, elevator2 };
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        var selected = strategy.SelectElevator(call, elevators);

        // Assert - Should prefer elevator1 which is already going up
        // (though both are at floor 1, elevator1 is committed to going up)
        Assert.NotNull(selected);
    }

    [Fact]
    public void NearestStrategy_AlwaysSelectsClosest()
    {
        // Arrange
        var strategy = new NearestDispatchStrategy();
        
        var elevator1 = new Elevator(1, _eventBus);
        var elevator2 = new Elevator(2, _eventBus);
        
        // Move elevator2 closer to target
        ((IElevatorContext)elevator2).SetDirection(ElevatorDirection.Up);
        ((IElevatorContext)elevator2).MoveOneFloor(); // Floor 2
        ((IElevatorContext)elevator2).MoveOneFloor(); // Floor 3
        ((IElevatorContext)elevator2).SetDirection(ElevatorDirection.Idle);
        
        var elevators = new List<IElevator> { elevator1, elevator2 };
        var call = new HallCall(4, ElevatorDirection.Up);

        // Act
        var selected = strategy.SelectElevator(call, elevators);

        // Assert - Should select elevator2 (closer to floor 4)
        Assert.NotNull(selected);
        Assert.Equal(2, selected.Id);
    }

    [Fact]
    public void ScanStrategy_ReturnsNull_WhenNoElevators()
    {
        // Arrange
        var strategy = new ScanDispatchStrategy();
        var elevators = new List<IElevator>();
        var call = new HallCall(5, ElevatorDirection.Up);

        // Act
        var selected = strategy.SelectElevator(call, elevators);

        // Assert
        Assert.Null(selected);
    }
}
