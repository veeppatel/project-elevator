using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Represents the building and handles random call generation.
/// Passengers make hall calls; cab calls are made when they board.
/// </summary>
public sealed class Building
{
    private readonly Random _random;
    private readonly ElevatorController _controller;
    private readonly IEventBus _eventBus;
    
    // Track passengers waiting at each floor (for simulation realism)
    private readonly Dictionary<Guid, int> _waitingPassengerDestinations = new();
    private readonly object _lock = new();

    /// <summary>
    /// Total number of floors in the building.
    /// </summary>
    public int TotalFloors { get; }

    /// <summary>
    /// Creates a new building.
    /// </summary>
    public Building(int totalFloors, ElevatorController controller, IEventBus eventBus)
    {
        TotalFloors = totalFloors;
        _controller = controller;
        _eventBus = eventBus;
        _random = new Random();
        
        // Subscribe to hall call serviced events to add cab calls
        _eventBus.Subscribe<Core.Events.HallCallServicedEvent>(OnHallCallServiced);
    }

    /// <summary>
    /// Constructor for testing with seeded random.
    /// </summary>
    internal Building(int totalFloors, ElevatorController controller, IEventBus eventBus, Random random)
        : this(totalFloors, controller, eventBus)
    {
        _random = random;
    }

    /// <summary>
    /// Generates a random hall call (passenger presses button on a floor).
    /// </summary>
    public HallCall GenerateRandomCall()
    {
        int floor = _random.Next(Configuration.MinFloor, Configuration.MaxFloor + 1);
        
        // Determine valid direction for this floor
        ElevatorDirection direction;
        if (floor == Configuration.MinFloor)
        {
            direction = ElevatorDirection.Up;
        }
        else if (floor == Configuration.MaxFloor)
        {
            direction = ElevatorDirection.Down;
        }
        else
        {
            direction = _random.Next(2) == 0 ? ElevatorDirection.Up : ElevatorDirection.Down;
        }

        var call = _controller.RegisterHallCall(floor, direction);
        
        // Generate the passenger's intended destination (they'll select it when boarding)
        int destination = GenerateRandomDestination(floor, direction);
        
        lock (_lock)
        {
            _waitingPassengerDestinations[call.Id] = destination;
        }
        
        return call;
    }

    /// <summary>
    /// Called when a hall call is serviced - passenger boards and selects destination.
    /// </summary>
    private void OnHallCallServiced(Core.Events.HallCallServicedEvent evt)
    {
        int destination;
        lock (_lock)
        {
            if (!_waitingPassengerDestinations.TryGetValue(evt.CallId, out destination))
            {
                return;
            }
            _waitingPassengerDestinations.Remove(evt.CallId);
        }

        // Passenger boards and presses their destination floor
        _controller.AddCabCall(evt.ElevatorId, destination);
    }

    /// <summary>
    /// Generates a random destination floor based on the call direction.
    /// </summary>
    private int GenerateRandomDestination(int fromFloor, ElevatorDirection direction)
    {
        if (direction == ElevatorDirection.Up)
        {
            if (fromFloor >= Configuration.MaxFloor)
                return Configuration.MaxFloor;
            return _random.Next(fromFloor + 1, Configuration.MaxFloor + 1);
        }
        else
        {
            if (fromFloor <= Configuration.MinFloor)
                return Configuration.MinFloor;
            return _random.Next(Configuration.MinFloor, fromFloor);
        }
    }
}
