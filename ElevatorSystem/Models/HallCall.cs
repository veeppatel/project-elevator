namespace ElevatorSystem.Models;

/// <summary>
/// Represents an external request from a hall button on a floor.
/// A passenger on a floor presses up or down to request an elevator.
/// </summary>
public sealed class HallCall : IEquatable<HallCall>
{
    /// <summary>Unique identifier for this hall call.</summary>
    public Guid Id { get; } = Guid.NewGuid();
    
    /// <summary>Floor where the call was made.</summary>
    public int Floor { get; }
    
    /// <summary>Direction the passenger wants to travel.</summary>
    public ElevatorDirection Direction { get; }
    
    /// <summary>When the call was made.</summary>
    public DateTime Timestamp { get; }
    
    /// <summary>ID of the elevator assigned to service this call, or null if unassigned.</summary>
    public int? AssignedElevatorId { get; private set; }
    
    /// <summary>Whether this call has been serviced (passenger picked up).</summary>
    public bool IsServiced { get; private set; }

    public HallCall(int floor, ElevatorDirection direction)
    {
        if (floor < Configuration.MinFloor || floor > Configuration.MaxFloor)
            throw new ArgumentOutOfRangeException(nameof(floor), 
                $"Floor must be between {Configuration.MinFloor} and {Configuration.MaxFloor}");
        
        if (direction == ElevatorDirection.Idle)
            throw new ArgumentException("Hall call direction cannot be Idle", nameof(direction));
        
        // Validate direction for edge floors
        if (floor == Configuration.MinFloor && direction == ElevatorDirection.Down)
            throw new ArgumentException("Cannot go down from the bottom floor", nameof(direction));
        
        if (floor == Configuration.MaxFloor && direction == ElevatorDirection.Up)
            throw new ArgumentException("Cannot go up from the top floor", nameof(direction));
        
        Floor = floor;
        Direction = direction;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns an elevator to service this call.
    /// </summary>
    public void AssignTo(int elevatorId)
    {
        AssignedElevatorId = elevatorId;
    }

    /// <summary>
    /// Unassigns the elevator (e.g., for reassignment if elevator becomes unavailable).
    /// </summary>
    public void Unassign()
    {
        AssignedElevatorId = null;
    }

    /// <summary>
    /// Marks this call as serviced (passenger picked up).
    /// </summary>
    public void MarkServiced()
    {
        IsServiced = true;
    }

    /// <summary>
    /// Validates the hall call is within valid bounds.
    /// </summary>
    public bool IsValid => Floor >= Configuration.MinFloor 
                          && Floor <= Configuration.MaxFloor 
                          && Direction != ElevatorDirection.Idle;

    public bool Equals(HallCall? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is HallCall other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"HallCall(Floor {Floor} {Direction}, Assigned: {AssignedElevatorId?.ToString() ?? "None"})";
}
