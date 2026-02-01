using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.Strategies;

/// <summary>
/// SCAN dispatch strategy (elevator algorithm).
/// Prioritizes elevators moving toward the call in the same direction,
/// then idle elevators, then any elevator.
/// </summary>
public sealed class ScanDispatchStrategy : IDispatchStrategy
{
    public string Name => "SCAN";

    public IElevator? SelectElevator(HallCall call, IReadOnlyList<IElevator> elevators)
    {
        if (elevators.Count == 0)
            return null;

        IElevator? bestElevator = null;
        int bestScore = int.MaxValue;

        foreach (var elevator in elevators)
        {
            int score = CalculateScore(elevator, call);
            
            if (score < bestScore)
            {
                bestScore = score;
                bestElevator = elevator;
            }
        }

        return bestElevator;
    }

    private int CalculateScore(IElevator elevator, HallCall call)
    {
        int physicalDistance = Math.Abs(elevator.CurrentFloor - call.Floor);

        // Case 1: Elevator is idle - use physical distance with slight preference
        if (elevator.Direction == ElevatorDirection.Idle)
        {
            return physicalDistance * 10 - 5; // Slight bonus for idle
        }

        // Case 2: Elevator moving toward call in compatible direction (best case)
        bool movingToward = elevator.Direction switch
        {
            ElevatorDirection.Up => call.Floor > elevator.CurrentFloor,
            ElevatorDirection.Down => call.Floor < elevator.CurrentFloor,
            _ => false
        };

        bool sameDirection = elevator.Direction == call.Direction;

        if (movingToward && sameDirection)
        {
            // Optimal: elevator will pass this floor going the same direction
            return physicalDistance * 10;
        }

        if (movingToward && !sameDirection)
        {
            // Will pass floor but going opposite direction - need to come back
            // Penalty: distance to extreme floor + distance back
            int extremeFloor = elevator.Direction == ElevatorDirection.Up
                ? Configuration.MaxFloor
                : Configuration.MinFloor;
            int distanceToExtreme = Math.Abs(elevator.CurrentFloor - extremeFloor);
            int distanceBack = Math.Abs(extremeFloor - call.Floor);
            return (distanceToExtreme + distanceBack) * 10 + 100;
        }

        // Case 3: Elevator moving away from call
        // Need to complete current direction, reverse, then service
        int currentExtreme = elevator.Direction == ElevatorDirection.Up
            ? Configuration.MaxFloor
            : Configuration.MinFloor;
        
        // Find furthest destination in current direction
        var destinationsInDirection = elevator.CabCallFloors
            .Where(f => elevator.Direction == ElevatorDirection.Up ? f > elevator.CurrentFloor : f < elevator.CurrentFloor)
            .ToList();
        
        if (destinationsInDirection.Any())
        {
            currentExtreme = elevator.Direction == ElevatorDirection.Up
                ? destinationsInDirection.Max()
                : destinationsInDirection.Min();
        }

        int toExtreme = Math.Abs(elevator.CurrentFloor - currentExtreme);
        int fromExtreme = Math.Abs(currentExtreme - call.Floor);
        
        return (toExtreme + fromExtreme) * 10 + 200; // Large penalty for moving away
    }
}
