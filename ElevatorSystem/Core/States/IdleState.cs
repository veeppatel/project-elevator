using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.States;

/// <summary>
/// Elevator is stationary with doors closed, waiting for requests.
/// Transitions to MovingState when requests arrive.
/// </summary>
public sealed class IdleState : IElevatorState
{
    public static IdleState Instance { get; } = new();
    
    public ElevatorStateType StateType => ElevatorStateType.Idle;

    private IdleState() { }

    public void OnEnter(IElevatorContext context)
    {
        context.SetDirection(ElevatorDirection.Idle);
        context.EventBus.Publish(new ElevatorIdleEvent
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
            return Task.FromResult<IElevatorState>(this);
        }

        // Determine direction based on requests
        var upStops = context.GetStopsInDirection(ElevatorDirection.Up).ToList();
        var downStops = context.GetStopsInDirection(ElevatorDirection.Down).ToList();

        ElevatorDirection newDirection;
        
        if (upStops.Count > 0 && downStops.Count > 0)
        {
            // Both directions have requests - choose based on closest
            int closestUp = upStops.Min(f => Math.Abs(f - context.CurrentFloor));
            int closestDown = downStops.Min(f => Math.Abs(f - context.CurrentFloor));
            newDirection = closestUp <= closestDown ? ElevatorDirection.Up : ElevatorDirection.Down;
        }
        else if (upStops.Count > 0)
        {
            newDirection = ElevatorDirection.Up;
        }
        else if (downStops.Count > 0)
        {
            newDirection = ElevatorDirection.Down;
        }
        else
        {
            return Task.FromResult<IElevatorState>(this);
        }

        context.SetDirection(newDirection);
        
        // Check if we need to service current floor first
        if (context.ShouldStopAtCurrentFloor())
        {
            return Task.FromResult<IElevatorState>(DoorsOpenState.Instance);
        }

        return Task.FromResult<IElevatorState>(MovingState.Instance);
    }
}
