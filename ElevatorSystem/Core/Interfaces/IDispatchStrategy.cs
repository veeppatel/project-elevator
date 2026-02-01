using ElevatorSystem.Models;

namespace ElevatorSystem.Core.Interfaces;

/// <summary>
/// Strategy pattern interface for elevator dispatch algorithms.
/// Different strategies can be swapped without changing the controller.
/// </summary>
public interface IDispatchStrategy
{
    /// <summary>Gets the name of this dispatch strategy.</summary>
    string Name { get; }
    
    /// <summary>
    /// Selects the best elevator to service a hall call.
    /// </summary>
    /// <param name="call">The hall call to service.</param>
    /// <param name="elevators">Available elevators.</param>
    /// <returns>The selected elevator, or null if none suitable.</returns>
    IElevator? SelectElevator(HallCall call, IReadOnlyList<IElevator> elevators);
}
