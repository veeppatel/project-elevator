using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.States;

/// <summary>
/// Elevator doors are closing, preparing for next action.
/// Transitions to MovingState or IdleState.
/// </summary>
public sealed class DoorsClosingState : IElevatorState
{
    public static DoorsClosingState Instance { get; } = new();
    
    public ElevatorStateType StateType => ElevatorStateType.DoorsClosing;

    private DoorsClosingState() { }

    public void OnEnter(IElevatorContext context)
    {
        context.EventBus.Publish(new DoorsClosedEvent
        {
            ElevatorId = context.Id,
            Floor = context.CurrentFloor
        });
    }

    public void OnExit(IElevatorContext context) { }

    public Task<IElevatorState> ProcessAsync(IElevatorContext context, CancellationToken token)
    {
        if (!context.HasPendingRequests)
        {
            return Task.FromResult<IElevatorState>(IdleState.Instance);
        }

        // Determine next direction based on remaining requests
        var currentDirection = context.Direction;
        
        // SCAN: Continue in current direction if there are stops that way
        if (currentDirection != ElevatorDirection.Idle)
        {
            var stopsInDirection = context.GetStopsInDirection(currentDirection).ToList();
            if (stopsInDirection.Any())
            {
                return Task.FromResult<IElevatorState>(MovingState.Instance);
            }
        }

        // Check opposite direction
        var oppositeDirection = currentDirection == ElevatorDirection.Up 
            ? ElevatorDirection.Down 
            : (currentDirection == ElevatorDirection.Down ? ElevatorDirection.Up : ElevatorDirection.Idle);
        
        if (oppositeDirection != ElevatorDirection.Idle)
        {
            var stopsInOpposite = context.GetStopsInDirection(oppositeDirection).ToList();
            if (stopsInOpposite.Any())
            {
                var oldDirection = context.Direction;
                context.SetDirection(oppositeDirection);
                
                if (oldDirection != oppositeDirection)
                {
                    context.EventBus.Publish(new DirectionChangedEvent
                    {
                        ElevatorId = context.Id,
                        Floor = context.CurrentFloor,
                        OldDirection = oldDirection,
                        NewDirection = oppositeDirection
                    });
                }
                
                return Task.FromResult<IElevatorState>(MovingState.Instance);
            }
        }

        // Check both directions from idle
        var upStops = context.GetStopsInDirection(ElevatorDirection.Up).ToList();
        var downStops = context.GetStopsInDirection(ElevatorDirection.Down).ToList();

        if (upStops.Any())
        {
            context.SetDirection(ElevatorDirection.Up);
            return Task.FromResult<IElevatorState>(MovingState.Instance);
        }
        
        if (downStops.Any())
        {
            context.SetDirection(ElevatorDirection.Down);
            return Task.FromResult<IElevatorState>(MovingState.Instance);
        }

        return Task.FromResult<IElevatorState>(IdleState.Instance);
    }
}
