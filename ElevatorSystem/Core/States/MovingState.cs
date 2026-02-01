using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.States;

/// <summary>
/// Elevator is moving between floors.
/// Transitions to DoorsOpenState when arriving at a stop, or reverses direction.
/// </summary>
public sealed class MovingState : IElevatorState
{
    public static MovingState Instance { get; } = new();
    
    public ElevatorStateType StateType => ElevatorStateType.Moving;

    private MovingState() { }

    public void OnEnter(IElevatorContext context) { }

    public void OnExit(IElevatorContext context) { }

    public async Task<IElevatorState> ProcessAsync(IElevatorContext context, CancellationToken token)
    {
        if (context.Direction == ElevatorDirection.Idle)
        {
            return IdleState.Instance;
        }

        // Check if we're at a boundary floor - need to reverse or stop
        if ((context.Direction == ElevatorDirection.Up && context.CurrentFloor >= Configuration.MaxFloor) ||
            (context.Direction == ElevatorDirection.Down && context.CurrentFloor <= Configuration.MinFloor))
        {
            // At boundary, check for stops in opposite direction
            var oppositeDirection = context.Direction == ElevatorDirection.Up 
                ? ElevatorDirection.Down 
                : ElevatorDirection.Up;
            var stopsInOpposite = context.GetStopsInDirection(oppositeDirection).ToList();
            
            if (stopsInOpposite.Any())
            {
                var oldDirection = context.Direction;
                context.SetDirection(oppositeDirection);
                
                context.EventBus.Publish(new DirectionChangedEvent
                {
                    ElevatorId = context.Id,
                    Floor = context.CurrentFloor,
                    OldDirection = oldDirection,
                    NewDirection = oppositeDirection
                });
                
                if (context.ShouldStopAtCurrentFloor())
                {
                    return DoorsOpenState.Instance;
                }
                
                return this; // Continue in new direction
            }
            
            return IdleState.Instance;
        }

        int fromFloor = context.CurrentFloor;
        var pendingStops = context.GetStopsInDirection(context.Direction).ToList();
        
        // Check if there are any stops to go to
        if (!pendingStops.Any())
        {
            // Check opposite direction
            var oppositeDirection = context.Direction == ElevatorDirection.Up 
                ? ElevatorDirection.Down 
                : ElevatorDirection.Up;
            var stopsInOpposite = context.GetStopsInDirection(oppositeDirection).ToList();
            
            if (stopsInOpposite.Any())
            {
                context.SetDirection(oppositeDirection);
                return this;
            }
            
            return IdleState.Instance;
        }
        
        // Calculate target floor (bounded)
        int toFloor = context.Direction == ElevatorDirection.Up 
            ? Math.Min(context.CurrentFloor + 1, Configuration.MaxFloor)
            : Math.Max(context.CurrentFloor - 1, Configuration.MinFloor);

        // Publish moving event
        context.EventBus.Publish(new ElevatorMovingEvent
        {
            ElevatorId = context.Id,
            Floor = context.CurrentFloor,
            FromFloor = fromFloor,
            ToFloor = toFloor,
            Direction = context.Direction,
            PendingStops = pendingStops
        });

        // Simulate travel time
        await Task.Delay(context.MovementDelayMs, token);

        // Move to next floor
        context.MoveOneFloor();

        // Publish arrival event
        context.EventBus.Publish(new ElevatorArrivedEvent
        {
            ElevatorId = context.Id,
            Floor = context.CurrentFloor,
            Direction = context.Direction
        });

        // Check if we should stop at this floor
        if (context.ShouldStopAtCurrentFloor())
        {
            return DoorsOpenState.Instance;
        }

        // Check if we need to reverse direction (SCAN algorithm)
        var stopsInCurrentDirection = context.GetStopsInDirection(context.Direction).ToList();
        
        if (!stopsInCurrentDirection.Any())
        {
            // No more stops in current direction - check opposite
            var oppositeDir = context.Direction == ElevatorDirection.Up 
                ? ElevatorDirection.Down 
                : ElevatorDirection.Up;
            var stopsOpposite = context.GetStopsInDirection(oppositeDir).ToList();
            
            if (stopsOpposite.Any())
            {
                var oldDirection = context.Direction;
                context.SetDirection(oppositeDir);
                
                context.EventBus.Publish(new DirectionChangedEvent
                {
                    ElevatorId = context.Id,
                    Floor = context.CurrentFloor,
                    OldDirection = oldDirection,
                    NewDirection = oppositeDir
                });
                
                // Check if current floor is now a stop in the new direction
                if (context.ShouldStopAtCurrentFloor())
                {
                    return DoorsOpenState.Instance;
                }
            }
            else
            {
                // No stops in either direction
                return IdleState.Instance;
            }
        }

        // Continue moving
        return this;
    }
}
