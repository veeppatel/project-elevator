using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Core.States;

/// <summary>
/// Elevator doors are open, passengers boarding and alighting.
/// Transitions to DoorsClosingState after delay.
/// </summary>
public sealed class DoorsOpenState : IElevatorState
{
    public static DoorsOpenState Instance { get; } = new();
    
    public ElevatorStateType StateType => ElevatorStateType.DoorsOpen;

    private DoorsOpenState() { }

    public void OnEnter(IElevatorContext context)
    {
        // Count passengers (simulated based on calls being serviced)
        int alighting = context.CabCallFloors.Contains(context.CurrentFloor) ? 1 : 0;
        int boarding = context.AssignedHallCalls.Count(c => c.Floor == context.CurrentFloor);
        
        context.EventBus.Publish(new DoorsOpenedEvent
        {
            ElevatorId = context.Id,
            Floor = context.CurrentFloor,
            PassengersBoarding = boarding,
            PassengersAlighting = alighting
        });
        
        // Service the current floor (removes calls)
        context.ServiceCurrentFloor();
    }

    public void OnExit(IElevatorContext context) { }

    public async Task<IElevatorState> ProcessAsync(IElevatorContext context, CancellationToken token)
    {
        // Wait for passengers to board/alight
        await Task.Delay(context.DoorDelayMs, token);
        
        return DoorsClosingState.Instance;
    }
}
