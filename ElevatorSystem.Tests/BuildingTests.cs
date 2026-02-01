using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.Strategies;
using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the Building class.
/// </summary>
public class BuildingTests
{
    private readonly IEventBus _eventBus = new EventBus();
    private readonly IDispatchStrategy _scanStrategy = new ScanDispatchStrategy();

    [Fact]
    public void Constructor_SetsCorrectTotalFloors()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Act
        var building = new Building(10, controller, _eventBus);

        // Assert
        Assert.Equal(10, building.TotalFloors);
    }

    [Fact]
    public void GenerateRandomCall_ReturnsValidHallCall()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);
        var building = new Building(10, controller, _eventBus);

        // Act
        var call = building.GenerateRandomCall();

        // Assert
        Assert.True(call.IsValid);
        Assert.InRange(call.Floor, 1, 10);
        Assert.NotEqual(ElevatorDirection.Idle, call.Direction);
    }

    [Fact]
    public void GenerateRandomCall_BottomFloor_OnlyGoesUp()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);
        var random = new Random(42);
        var building = new Building(10, controller, _eventBus, random);

        // Act & Assert - Generate many calls and check floor 1 cases
        for (int i = 0; i < 100; i++)
        {
            var call = building.GenerateRandomCall();
            if (call.Floor == 1)
            {
                Assert.Equal(ElevatorDirection.Up, call.Direction);
            }
        }
    }

    [Fact]
    public void GenerateRandomCall_TopFloor_OnlyGoesDown()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);
        var building = new Building(10, controller, _eventBus);

        // Act & Assert - Generate many calls and check floor 10 cases
        for (int i = 0; i < 100; i++)
        {
            var call = building.GenerateRandomCall();
            if (call.Floor == 10)
            {
                Assert.Equal(ElevatorDirection.Down, call.Direction);
            }
        }
    }
}
