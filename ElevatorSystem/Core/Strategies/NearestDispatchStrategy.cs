using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.Strategies;

/// <summary>
/// Simple nearest elevator dispatch strategy.
/// Always picks the closest elevator regardless of direction.
/// </summary>
public sealed class NearestDispatchStrategy : IDispatchStrategy
{
    public string Name => "Nearest";

    public IElevator? SelectElevator(HallCall call, IReadOnlyList<IElevator> elevators)
    {
        if (elevators.Count == 0)
            return null;

        return elevators
            .OrderBy(e => Math.Abs(e.CurrentFloor - call.Floor))
            .ThenBy(e => e.Direction == ElevatorDirection.Idle ? 0 : 1) // Prefer idle if tied
            .FirstOrDefault();
    }
}
