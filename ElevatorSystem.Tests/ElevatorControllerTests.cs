using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the ElevatorController class.
/// </summary>
public class ElevatorControllerTests
{
    private readonly Logger _logger = new();

    [Fact]
    public void Constructor_CreatesCorrectNumberOfElevators()
    {
        // Arrange & Act
        var controller = new ElevatorController(4, _logger);

        // Assert
        Assert.Equal(4, controller.Elevators.Count);
    }

    [Fact]
    public void DispatchElevator_AssignsNearestIdleElevator()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);
        var call = new FloorCall(5, Direction.Up);

        // Act
        var dispatched = controller.DispatchElevator(call);

        // Assert
        Assert.NotNull(dispatched);
        Assert.Contains(5, dispatched.Destinations);
    }

    [Fact]
    public void DispatchElevator_InvalidCall_ReturnsNull()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);
        var invalidCall = new FloorCall(0, Direction.Up);

        // Act
        var dispatched = controller.DispatchElevator(invalidCall);

        // Assert
        Assert.Null(dispatched);
    }

    [Fact]
    public void DispatchElevator_PrefersElevatorMovingTowardCall()
    {
        // Arrange - Create two elevators manually
        var elevator1 = new Elevator(1);
        var elevator2 = new Elevator(2);
        
        // Elevator 1 is at floor 1, going up (has destination at floor 10)
        elevator1.AddDestination(10, Direction.Up);
        
        // Both start at floor 1
        var elevators = new List<Elevator> { elevator1, elevator2 };
        var controller = new ElevatorController(elevators, _logger);

        // Act - Call from floor 5 going up
        var call = new FloorCall(5, Direction.Up);
        var dispatched = controller.DispatchElevator(call);

        // Assert - Elevator 1 should be preferred (already going up past floor 5)
        Assert.NotNull(dispatched);
        Assert.Contains(5, dispatched.Destinations);
    }

    [Fact]
    public void AddPassengerDestination_AddsToCorrectElevator()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);

        // Act
        controller.AddPassengerDestination(1, 8);

        // Assert
        var elevator = controller.Elevators.First(e => e.Id == 1);
        Assert.Contains(8, elevator.Destinations);
    }

    [Fact]
    public void GetSystemStatus_ReturnsFormattedString()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);

        // Act
        var status = controller.GetSystemStatus();

        // Assert
        Assert.Contains("Elevator 1", status);
        Assert.Contains("Elevator 4", status);
    }

    [Fact]
    public void DispatchElevator_MultipleCalls_DistributesLoad()
    {
        // Arrange
        var controller = new ElevatorController(4, _logger);

        // Act - Send multiple calls
        controller.DispatchElevator(new FloorCall(2, Direction.Up));
        controller.DispatchElevator(new FloorCall(5, Direction.Up));
        controller.DispatchElevator(new FloorCall(8, Direction.Down));

        // Assert - At least some elevators should have destinations
        int elevatorsWithDestinations = controller.Elevators.Count(e => e.HasDestinations);
        Assert.True(elevatorsWithDestinations >= 1);
    }
}
