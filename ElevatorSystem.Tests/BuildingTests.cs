using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the Building class.
/// </summary>
public class BuildingTests
{
    private readonly Logger _logger = new();

    [Fact]
    public void Constructor_SetsCorrectTotalFloors()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);

        // Act
        var building = new Building(10, controller, _logger);

        // Assert
        Assert.Equal(10, building.TotalFloors);
    }

    [Fact]
    public void GenerateRandomCall_ReturnsValidFloorCall()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);
        var building = new Building(10, controller, _logger);

        // Act
        var call = building.GenerateRandomCall();

        // Assert
        Assert.True(call.IsValid);
        Assert.InRange(call.Floor, 1, 10);
        Assert.NotEqual(Direction.Idle, call.Direction);
    }

    [Fact]
    public void GenerateRandomCall_BottomFloor_OnlyGoesUp()
    {
        // Arrange - Use seeded random to force floor 1
        var controller = new ElevatorController(4, _logger);
        var seededRandom = new Random(42); // Seed that gives floor 1
        var building = new Building(10, controller, _logger, seededRandom);

        // Act - Generate multiple calls and check floor 1 cases
        for (int i = 0; i < 100; i++)
        {
            var call = building.GenerateRandomCall();
            if (call.Floor == 1)
            {
                // Assert - Floor 1 should only have Up direction
                Assert.Equal(Direction.Up, call.Direction);
            }
        }
    }

    [Fact]
    public void GenerateRandomCall_TopFloor_OnlyGoesDown()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);
        var building = new Building(10, controller, _logger);

        // Act - Generate multiple calls and check floor 10 cases
        for (int i = 0; i < 100; i++)
        {
            var call = building.GenerateRandomCall();
            if (call.Floor == 10)
            {
                // Assert - Floor 10 should only have Down direction
                Assert.Equal(Direction.Down, call.Direction);
            }
        }
    }

    [Fact]
    public void RegisterCall_DispatchesElevator()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);
        var building = new Building(10, controller, _logger);
        var call = new FloorCall(5, Direction.Up);

        // Act
        building.RegisterCall(call);

        // Assert - At least one elevator should have the destination
        Assert.True(controller.Elevators.Any(e => e.Destinations.Contains(5)));
    }

    [Fact]
    public void RegisterCall_InvalidCall_IsRejected()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);
        var building = new Building(10, controller, _logger);
        var invalidCall = new FloorCall(0, Direction.Up);

        // Act
        building.RegisterCall(invalidCall);

        // Assert - No elevator should have invalid destination
        Assert.True(controller.Elevators.All(e => !e.Destinations.Contains(0)));
    }
}
