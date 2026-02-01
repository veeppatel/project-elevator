using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Controls and coordinates multiple elevators using the Strategy Pattern for dispatch.
/// Manages hall call queue and assigns calls to appropriate elevators.
/// </summary>
public sealed class ElevatorController
{
    private readonly List<IElevator> _elevators;
    private readonly List<HallCall> _pendingHallCalls = new();
    private readonly IDispatchStrategy _dispatchStrategy;
    private readonly IEventBus _eventBus;
    private readonly object _lock = new();

    /// <summary>
    /// Gets a read-only view of all elevators.
    /// </summary>
    public IReadOnlyList<IElevator> Elevators => _elevators.AsReadOnly();

    /// <summary>
    /// Gets pending hall calls that haven't been serviced yet.
    /// </summary>
    public IReadOnlyList<HallCall> PendingHallCalls
    {
        get
        {
            lock (_lock)
            {
                return _pendingHallCalls.Where(c => !c.IsServiced).ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// The dispatch strategy being used.
    /// </summary>
    public IDispatchStrategy DispatchStrategy => _dispatchStrategy;

    /// <summary>
    /// Creates a new elevator controller.
    /// </summary>
    public ElevatorController(
        int elevatorCount, 
        IEventBus eventBus, 
        IDispatchStrategy dispatchStrategy,
        int movementDelayMs,
        int doorDelayMs)
    {
        _eventBus = eventBus;
        _dispatchStrategy = dispatchStrategy;
        _elevators = new List<IElevator>();

        for (int i = 1; i <= elevatorCount; i++)
        {
            _elevators.Add(new Elevator(i, eventBus, movementDelayMs, doorDelayMs));
        }
    }

    /// <summary>
    /// Constructor for testing with pre-configured elevators.
    /// </summary>
    internal ElevatorController(
        List<IElevator> elevators, 
        IEventBus eventBus, 
        IDispatchStrategy dispatchStrategy)
    {
        _elevators = elevators;
        _eventBus = eventBus;
        _dispatchStrategy = dispatchStrategy;
    }

    /// <summary>
    /// Registers a new hall call and dispatches an elevator to service it.
    /// </summary>
    public HallCall RegisterHallCall(int floor, ElevatorDirection direction)
    {
        var call = new HallCall(floor, direction);
        
        _eventBus.Publish(new HallCallReceivedEvent
        {
            Floor = floor,
            Direction = direction,
            CallId = call.Id
        });

        lock (_lock)
        {
            _pendingHallCalls.Add(call);
        }

        DispatchElevator(call);
        
        return call;
    }

    /// <summary>
    /// Dispatches an elevator to service a hall call.
    /// </summary>
    private void DispatchElevator(HallCall call)
    {
        var selectedElevator = _dispatchStrategy.SelectElevator(call, _elevators);

        if (selectedElevator != null)
        {
            selectedElevator.AssignHallCall(call);
            
            _eventBus.Publish(new HallCallAssignedEvent
            {
                CallId = call.Id,
                Floor = call.Floor,
                Direction = call.Direction,
                ElevatorId = selectedElevator.Id,
                EstimatedStops = CountStopsBefore(selectedElevator, call.Floor)
            });
        }
    }

    /// <summary>
    /// Counts stops before reaching the target floor (for ETA estimation).
    /// </summary>
    private int CountStopsBefore(IElevator elevator, int targetFloor)
    {
        var stops = elevator.CabCallFloors
            .Concat(elevator.AssignedHallCalls.Select(c => c.Floor))
            .Distinct()
            .ToList();

        if (elevator.Direction == ElevatorDirection.Up)
        {
            return stops.Count(f => f > elevator.CurrentFloor && f < targetFloor);
        }
        else if (elevator.Direction == ElevatorDirection.Down)
        {
            return stops.Count(f => f < elevator.CurrentFloor && f > targetFloor);
        }
        
        return 0;
    }

    /// <summary>
    /// Adds a cab call for a passenger inside an elevator.
    /// </summary>
    public void AddCabCall(int elevatorId, int destinationFloor)
    {
        var elevator = _elevators.FirstOrDefault(e => e.Id == elevatorId);
        elevator?.AddCabCall(destinationFloor);
    }

    /// <summary>
    /// Cleans up serviced hall calls.
    /// </summary>
    public void CleanupServicedCalls()
    {
        lock (_lock)
        {
            _pendingHallCalls.RemoveAll(c => c.IsServiced);
        }
    }

    /// <summary>
    /// Gets system status snapshot for display.
    /// </summary>
    public SystemStatusEvent GetSystemStatus()
    {
        var snapshots = _elevators.Select(e => new ElevatorSnapshot
        {
            Id = e.Id,
            CurrentFloor = e.CurrentFloor,
            Direction = e.Direction,
            State = e.StateType,
            Destinations = e.CabCallFloors.Concat(e.AssignedHallCalls.Select(c => c.Floor)).Distinct().Order().ToList(),
            AssignedHallCallCount = e.AssignedHallCalls.Count
        }).ToList();

        return new SystemStatusEvent
        {
            Elevators = snapshots,
            PendingHallCalls = PendingHallCalls.Count
        };
    }
}
