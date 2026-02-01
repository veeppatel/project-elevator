using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.Events;

/// <summary>
/// Base class for elevator events with common properties.
/// </summary>
public abstract record ElevatorEventBase : IElevatorEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public int ElevatorId { get; init; }
    public int Floor { get; init; }
}

/// <summary>
/// Elevator started moving from one floor toward another.
/// </summary>
public sealed record ElevatorMovingEvent : ElevatorEventBase
{
    public required int FromFloor { get; init; }
    public required int ToFloor { get; init; }
    public required ElevatorDirection Direction { get; init; }
    public required IReadOnlyList<int> PendingStops { get; init; }
}

/// <summary>
/// Elevator arrived at a floor.
/// </summary>
public sealed record ElevatorArrivedEvent : ElevatorEventBase
{
    public required ElevatorDirection Direction { get; init; }
}

/// <summary>
/// Elevator doors opened.
/// </summary>
public sealed record DoorsOpenedEvent : ElevatorEventBase
{
    public required int PassengersBoarding { get; init; }
    public required int PassengersAlighting { get; init; }
}

/// <summary>
/// Elevator doors closed.
/// </summary>
public sealed record DoorsClosedEvent : ElevatorEventBase;

/// <summary>
/// Elevator became idle (no pending requests).
/// </summary>
public sealed record ElevatorIdleEvent : ElevatorEventBase;

/// <summary>
/// Elevator changed direction.
/// </summary>
public sealed record DirectionChangedEvent : ElevatorEventBase
{
    public required ElevatorDirection OldDirection { get; init; }
    public required ElevatorDirection NewDirection { get; init; }
}

/// <summary>
/// Hall call received from a floor.
/// </summary>
public sealed record HallCallReceivedEvent : IElevatorEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public required int Floor { get; init; }
    public required ElevatorDirection Direction { get; init; }
    public required Guid CallId { get; init; }
}

/// <summary>
/// Hall call assigned to an elevator.
/// </summary>
public sealed record HallCallAssignedEvent : IElevatorEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public required Guid CallId { get; init; }
    public required int Floor { get; init; }
    public required ElevatorDirection Direction { get; init; }
    public required int ElevatorId { get; init; }
    public required int EstimatedStops { get; init; }
}

/// <summary>
/// Hall call serviced (passenger picked up).
/// </summary>
public sealed record HallCallServicedEvent : IElevatorEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public required Guid CallId { get; init; }
    public required int Floor { get; init; }
    public required int ElevatorId { get; init; }
    public required TimeSpan WaitTime { get; init; }
}

/// <summary>
/// Cab call added by passenger inside elevator.
/// </summary>
public sealed record CabCallAddedEvent : ElevatorEventBase
{
    public required int DestinationFloor { get; init; }
}

/// <summary>
/// Passenger delivered to destination.
/// </summary>
public sealed record PassengerDeliveredEvent : ElevatorEventBase
{
    public required int DestinationFloor { get; init; }
}

/// <summary>
/// System status update (periodic).
/// </summary>
public sealed record SystemStatusEvent : IElevatorEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public required IReadOnlyList<ElevatorSnapshot> Elevators { get; init; }
    public required int PendingHallCalls { get; init; }
}

/// <summary>
/// Snapshot of elevator state for display.
/// </summary>
public sealed record ElevatorSnapshot
{
    public required int Id { get; init; }
    public required int CurrentFloor { get; init; }
    public required ElevatorDirection Direction { get; init; }
    public required ElevatorStateType State { get; init; }
    public required IReadOnlyList<int> Destinations { get; init; }
    public required int AssignedHallCallCount { get; init; }
}
