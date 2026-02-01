namespace ElevatorSystem.Models;

/// <summary>
/// Represents the direction an elevator is traveling or a passenger wants to go.
/// Named ElevatorDirection to avoid conflicts with System.Direction.
/// </summary>
public enum ElevatorDirection
{
    /// <summary>Elevator is not moving, waiting for requests.</summary>
    Idle,
    
    /// <summary>Elevator is moving upward or passenger wants to go up.</summary>
    Up,
    
    /// <summary>Elevator is moving downward or passenger wants to go down.</summary>
    Down
}

/// <summary>
/// Represents the current operational state of an elevator.
/// Used by the state machine for transitions.
/// </summary>
public enum ElevatorStateType
{
    /// <summary>Elevator is stationary with doors closed, no pending requests.</summary>
    Idle,
    
    /// <summary>Elevator is moving between floors.</summary>
    Moving,
    
    /// <summary>Elevator has stopped and doors are open for passengers.</summary>
    DoorsOpen,
    
    /// <summary>Doors are closing, preparing for movement or idle.</summary>
    DoorsClosing
}

/// <summary>
/// Type of elevator request.
/// </summary>
public enum RequestType
{
    /// <summary>External request from hall button (floor + direction).</summary>
    HallCall,
    
    /// <summary>Internal request from cab button (destination floor).</summary>
    CabCall
}
