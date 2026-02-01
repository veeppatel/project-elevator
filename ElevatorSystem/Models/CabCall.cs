namespace ElevatorSystem.Models;

/// <summary>
/// Represents an internal request from a passenger inside an elevator.
/// Passenger presses a floor button inside the cab to select their destination.
/// </summary>
public sealed class CabCall : IEquatable<CabCall>
{
    /// <summary>Unique identifier for this cab call.</summary>
    public Guid Id { get; } = Guid.NewGuid();
    
    /// <summary>Destination floor selected by the passenger.</summary>
    public int DestinationFloor { get; }
    
    /// <summary>When the call was made.</summary>
    public DateTime Timestamp { get; }
    
    /// <summary>Whether this call has been serviced (passenger delivered).</summary>
    public bool IsServiced { get; private set; }

    public CabCall(int destinationFloor)
    {
        if (destinationFloor < Configuration.MinFloor || destinationFloor > Configuration.MaxFloor)
            throw new ArgumentOutOfRangeException(nameof(destinationFloor), 
                $"Floor must be between {Configuration.MinFloor} and {Configuration.MaxFloor}");
        
        DestinationFloor = destinationFloor;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks this call as serviced (passenger delivered to destination).
    /// </summary>
    public void MarkServiced()
    {
        IsServiced = true;
    }

    public bool Equals(CabCall? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is CabCall other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"CabCall(Floor {DestinationFloor}, Serviced: {IsServiced})";
}
