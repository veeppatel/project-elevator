using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.States;

namespace ElevatorSystem.Models;

/// <summary>
/// Represents a single elevator car in the building.
/// Uses State Pattern for behavior and properly separates hall calls from cab calls.
/// </summary>
public sealed class Elevator : IElevator, IElevatorContext
{
    private readonly List<HallCall> _assignedHallCalls = new();
    private readonly HashSet<int> _cabCallFloors = new();
    private readonly object _lock = new();
    
    private IElevatorState _currentState;
    private ElevatorDirection _direction;
    private int _currentFloor;

    #region IElevator Properties

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public int CurrentFloor => _currentFloor;

    /// <inheritdoc />
    public ElevatorDirection Direction => _direction;

    /// <inheritdoc />
    public ElevatorStateType StateType => _currentState.StateType;

    /// <inheritdoc />
    public IReadOnlySet<int> CabCallFloors
    {
        get
        {
            lock (_lock)
            {
                return _cabCallFloors.ToHashSet();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<HallCall> AssignedHallCalls
    {
        get
        {
            lock (_lock)
            {
                return _assignedHallCalls.ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc />
    public bool HasPendingRequests
    {
        get
        {
            lock (_lock)
            {
                return _cabCallFloors.Count > 0 || _assignedHallCalls.Any(c => !c.IsServiced);
            }
        }
    }

    #endregion

    #region IElevatorContext Properties

    IReadOnlyList<HallCall> IElevatorContext.AssignedHallCalls => AssignedHallCalls;
    IReadOnlySet<int> IElevatorContext.CabCallFloors => CabCallFloors;
    public IEventBus EventBus { get; }
    
    public int MovementDelayMs { get; }
    public int DoorDelayMs { get; }

    #endregion

    /// <summary>
    /// Creates a new elevator starting at floor 1.
    /// </summary>
    public Elevator(int id, IEventBus eventBus, int movementDelayMs, int doorDelayMs)
    {
        Id = id;
        EventBus = eventBus;
        MovementDelayMs = movementDelayMs;
        DoorDelayMs = doorDelayMs;
        
        _currentFloor = 1;
        _direction = ElevatorDirection.Idle;
        _currentState = IdleState.Instance;
    }

    /// <summary>
    /// Constructor for testing with default timing.
    /// </summary>
    internal Elevator(int id, IEventBus eventBus) 
        : this(id, eventBus, 1000, 1000) { }

    #region IElevator Methods

    /// <inheritdoc />
    public void AssignHallCall(HallCall call)
    {
        lock (_lock)
        {
            if (!_assignedHallCalls.Contains(call))
            {
                call.AssignTo(Id);
                _assignedHallCalls.Add(call);
            }
        }
    }

    /// <inheritdoc />
    public void AddCabCall(int destinationFloor)
    {
        if (destinationFloor < Configuration.MinFloor || destinationFloor > Configuration.MaxFloor)
            return;

        lock (_lock)
        {
            if (_cabCallFloors.Add(destinationFloor))
            {
                EventBus.Publish(new CabCallAddedEvent
                {
                    ElevatorId = Id,
                    Floor = CurrentFloor,
                    DestinationFloor = destinationFloor
                });
            }
        }
    }

    /// <inheritdoc />
    public async Task ProcessAsync(CancellationToken token)
    {
        var nextState = await _currentState.ProcessAsync(this, token);
        
        if (nextState != _currentState)
        {
            _currentState.OnExit(this);
            _currentState = nextState;
            _currentState.OnEnter(this);
        }
    }

    /// <inheritdoc />
    public int CalculateSuitabilityScore(HallCall call)
    {
        int physicalDistance = Math.Abs(CurrentFloor - call.Floor);

        if (Direction == ElevatorDirection.Idle)
            return physicalDistance * 10 - 5;

        bool movingToward = Direction switch
        {
            ElevatorDirection.Up => call.Floor > CurrentFloor,
            ElevatorDirection.Down => call.Floor < CurrentFloor,
            _ => false
        };

        if (movingToward && Direction == call.Direction)
            return physicalDistance * 10;

        // Penalty for not being optimal
        return physicalDistance * 10 + 100;
    }

    #endregion

    #region IElevatorContext Methods

    /// <inheritdoc />
    public void MoveOneFloor()
    {
        if (_direction == ElevatorDirection.Up && _currentFloor < Configuration.MaxFloor)
        {
            _currentFloor++;
        }
        else if (_direction == ElevatorDirection.Down && _currentFloor > Configuration.MinFloor)
        {
            _currentFloor--;
        }
    }

    /// <inheritdoc />
    public void SetDirection(ElevatorDirection direction)
    {
        _direction = direction;
    }

    /// <inheritdoc />
    public void ServiceCurrentFloor()
    {
        lock (_lock)
        {
            // Service cab calls for this floor
            if (_cabCallFloors.Remove(_currentFloor))
            {
                EventBus.Publish(new PassengerDeliveredEvent
                {
                    ElevatorId = Id,
                    Floor = _currentFloor,
                    DestinationFloor = _currentFloor
                });
            }

            // Service hall calls for this floor in current direction
            var servicedCalls = _assignedHallCalls
                .Where(c => c.Floor == _currentFloor && 
                           !c.IsServiced && 
                           (c.Direction == _direction || _direction == ElevatorDirection.Idle))
                .ToList();

            foreach (var call in servicedCalls)
            {
                call.MarkServiced();
                
                EventBus.Publish(new HallCallServicedEvent
                {
                    CallId = call.Id,
                    Floor = call.Floor,
                    ElevatorId = Id,
                    WaitTime = DateTime.UtcNow - call.Timestamp
                });
            }

            // Remove serviced calls
            _assignedHallCalls.RemoveAll(c => c.IsServiced);
        }
    }

    /// <inheritdoc />
    public bool ShouldStopAtCurrentFloor()
    {
        lock (_lock)
        {
            // Stop for cab calls
            if (_cabCallFloors.Contains(_currentFloor))
                return true;

            // Stop for hall calls in current direction
            return _assignedHallCalls.Any(c => 
                c.Floor == _currentFloor && 
                !c.IsServiced &&
                (c.Direction == _direction || _direction == ElevatorDirection.Idle));
        }
    }

    /// <inheritdoc />
    public IEnumerable<int> GetStopsInDirection(ElevatorDirection direction)
    {
        lock (_lock)
        {
            var stops = new HashSet<int>();

            // Add cab calls in direction
            foreach (var floor in _cabCallFloors)
            {
                if (direction == ElevatorDirection.Up && floor > _currentFloor)
                    stops.Add(floor);
                else if (direction == ElevatorDirection.Down && floor < _currentFloor)
                    stops.Add(floor);
            }

            // Add hall calls in direction
            foreach (var call in _assignedHallCalls.Where(c => !c.IsServiced))
            {
                if (direction == ElevatorDirection.Up && call.Floor > _currentFloor)
                    stops.Add(call.Floor);
                else if (direction == ElevatorDirection.Down && call.Floor < _currentFloor)
                    stops.Add(call.Floor);
                else if (call.Floor == _currentFloor)
                    stops.Add(call.Floor);
            }

            return stops.OrderBy(f => direction == ElevatorDirection.Up ? f : -f);
        }
    }

    #endregion

    public override string ToString() => 
        $"Elevator {Id}: Floor {CurrentFloor}, {Direction}, {StateType}";
}
