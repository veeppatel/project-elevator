using ElevatorSystem.Models;

namespace ElevatorSystem.Core.Interfaces;

/// <summary>
/// Interface for elevator instance.
/// </summary>
public interface IElevator
{
    /// <summary>Unique identifier for this elevator.</summary>
    int Id { get; }
    
    /// <summary>Current floor position.</summary>
    int CurrentFloor { get; }
    
    /// <summary>Current travel direction.</summary>
    ElevatorDirection Direction { get; }
    
    /// <summary>Current state type.</summary>
    ElevatorStateType StateType { get; }
    
    /// <summary>Cab calls (destination floors from passengers inside).</summary>
    IReadOnlySet<int> CabCallFloors { get; }
    
    /// <summary>Assigned hall calls (pickups).</summary>
    IReadOnlyList<HallCall> AssignedHallCalls { get; }
    
    /// <summary>Whether elevator has any pending requests.</summary>
    bool HasPendingRequests { get; }
    
    /// <summary>Assigns a hall call to this elevator.</summary>
    void AssignHallCall(HallCall call);
    
    /// <summary>Adds a cab call (passenger inside selects destination).</summary>
    void AddCabCall(int destinationFloor);
    
    /// <summary>Processes one tick of elevator operation.</summary>
    Task ProcessAsync(CancellationToken token);
    
    /// <summary>Calculates suitability score for servicing a hall call (lower is better).</summary>
    int CalculateSuitabilityScore(HallCall call);
}
