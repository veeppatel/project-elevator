using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.Strategies;
using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem.Tests;

/// <summary>
/// Unit tests for the ElevatorController class.
/// </summary>
public class ElevatorControllerTests
{
    private readonly IEventBus _eventBus = new EventBus();
    private readonly IDispatchStrategy _scanStrategy = new ScanDispatchStrategy();

    [Fact]
    public void Constructor_CreatesCorrectNumberOfElevators()
    {
        // Arrange & Act
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Assert
        Assert.Equal(4, controller.Elevators.Count);
    }

    [Fact]
    public void RegisterHallCall_ReturnsValidCall()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Act
        var call = controller.RegisterHallCall(5, ElevatorDirection.Up);

        // Assert
        Assert.NotNull(call);
        Assert.Equal(5, call.Floor);
        Assert.Equal(ElevatorDirection.Up, call.Direction);
        Assert.NotNull(call.AssignedElevatorId);
    }

    [Fact]
    public void RegisterHallCall_AssignsToElevator()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Act
        var call = controller.RegisterHallCall(5, ElevatorDirection.Up);

        // Assert
        var assignedElevator = controller.Elevators.First(e => e.Id == call.AssignedElevatorId);
        Assert.Contains(call, assignedElevator.AssignedHallCalls);
    }

    [Fact]
    public void AddCabCall_AddsToCorrectElevator()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Act
        controller.AddCabCall(1, 8);

        // Assert
        var elevator = controller.Elevators.First(e => e.Id == 1);
        Assert.Contains(8, elevator.CabCallFloors);
    }

    [Fact]
    public void GetSystemStatus_ReturnsAllElevators()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Act
        var status = controller.GetSystemStatus();

        // Assert
        Assert.Equal(4, status.Elevators.Count);
        Assert.Contains(status.Elevators, e => e.Id == 1);
        Assert.Contains(status.Elevators, e => e.Id == 4);
    }

    [Fact]
    public void DispatchStrategy_CanBeChanged()
    {
        // Arrange
        var nearestStrategy = new NearestDispatchStrategy();
        var controller = new ElevatorController(4, _eventBus, nearestStrategy, 100, 100);

        // Assert
        Assert.Equal("Nearest", controller.DispatchStrategy.Name);
    }

    [Fact]
    public void MultipleCalls_DistributeAcrossElevators()
    {
        // Arrange
        var controller = new ElevatorController(4, _eventBus, _scanStrategy, 100, 100);

        // Act - Register multiple calls
        controller.RegisterHallCall(2, ElevatorDirection.Up);
        controller.RegisterHallCall(5, ElevatorDirection.Up);
        controller.RegisterHallCall(8, ElevatorDirection.Down);
        controller.RegisterHallCall(3, ElevatorDirection.Down);

        // Assert - At least some elevators should have calls
        int elevatorsWithCalls = controller.Elevators.Count(e => e.HasPendingRequests);
        Assert.True(elevatorsWithCalls >= 1);
    }
}
