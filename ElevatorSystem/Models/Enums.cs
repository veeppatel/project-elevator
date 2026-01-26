namespace ElevatorSystem.Models;

/// <summary>
/// Represents the direction an elevator is moving or a passenger wants to go.
/// </summary>
public enum Direction
{
    Up,
    Down,
    Idle
}

/// <summary>
/// Represents the current operational state of an elevator.
/// </summary>
public enum ElevatorState
{
    /// <summary>Elevator is stationary with doors closed, ready to move.</summary>
    Stopped,
    
    /// <summary>Elevator is moving between floors.</summary>
    Moving,
    
    /// <summary>Elevator has stopped and doors are open for passengers.</summary>
    DoorsOpen
}
