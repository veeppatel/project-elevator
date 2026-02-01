using ElevatorSystem.Models;

namespace ElevatorSystem.Core.Interfaces;

/// <summary>
/// Interface for elevator state in the State Pattern.
/// Each state handles elevator behavior differently.
/// </summary>
public interface IElevatorState
{
    /// <summary>Gets the type of this state.</summary>
    ElevatorStateType StateType { get; }
    
    /// <summary>
    /// Processes the elevator in this state.
    /// May trigger state transitions.
    /// </summary>
    /// <param name="context">The elevator context.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The next state to transition to, or this state if no transition.</returns>
    Task<IElevatorState> ProcessAsync(IElevatorContext context, CancellationToken token);
    
    /// <summary>
    /// Called when entering this state.
    /// </summary>
    void OnEnter(IElevatorContext context);
    
    /// <summary>
    /// Called when exiting this state.
    /// </summary>
    void OnExit(IElevatorContext context);
}

/// <summary>
/// Context interface that states use to interact with the elevator.
/// Provides access to elevator data and actions without exposing full implementation.
/// </summary>
public interface IElevatorContext
{
    /// <summary>Elevator identifier.</summary>
    int Id { get; }
    
    /// <summary>Current floor position.</summary>
    int CurrentFloor { get; }
    
    /// <summary>Current travel direction.</summary>
    ElevatorDirection Direction { get; }
    
    /// <summary>Hall calls assigned to this elevator (pickups).</summary>
    IReadOnlyList<HallCall> AssignedHallCalls { get; }
    
    /// <summary>Cab calls from passengers inside (destinations).</summary>
    IReadOnlySet<int> CabCallFloors { get; }
    
    /// <summary>Event bus for publishing events.</summary>
    IEventBus EventBus { get; }
    
    /// <summary>Configuration for timing.</summary>
    int MovementDelayMs { get; }
    int DoorDelayMs { get; }
    
    /// <summary>Moves elevator one floor in current direction.</summary>
    void MoveOneFloor();
    
    /// <summary>Sets the elevator direction.</summary>
    void SetDirection(ElevatorDirection direction);
    
    /// <summary>Services the current floor (removes calls for this floor).</summary>
    void ServiceCurrentFloor();
    
    /// <summary>Checks if elevator should stop at current floor.</summary>
    bool ShouldStopAtCurrentFloor();
    
    /// <summary>Gets floors to stop at in current direction.</summary>
    IEnumerable<int> GetStopsInDirection(ElevatorDirection direction);
    
    /// <summary>Checks if there are any pending requests.</summary>
    bool HasPendingRequests { get; }
}
