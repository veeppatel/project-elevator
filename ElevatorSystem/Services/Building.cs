using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Represents the building and handles random call generation.
/// </summary>
public class Building
{
    private readonly Random _random;
    private readonly ElevatorController _controller;
    private readonly Logger _logger;

    /// <summary>
    /// Total number of floors in the building.
    /// </summary>
    public int TotalFloors { get; }

    /// <summary>
    /// Creates a new building with the specified configuration.
    /// </summary>
    /// <param name="totalFloors">Number of floors in the building.</param>
    /// <param name="controller">Elevator controller to dispatch requests to.</param>
    /// <param name="logger">Logger for output.</param>
    public Building(int totalFloors, ElevatorController controller, Logger logger)
    {
        TotalFloors = totalFloors;
        _controller = controller;
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Constructor for testing - allows injecting a seeded random.
    /// </summary>
    internal Building(int totalFloors, ElevatorController controller, Logger logger, Random random)
    {
        TotalFloors = totalFloors;
        _controller = controller;
        _logger = logger;
        _random = random;
    }

    /// <summary>
    /// Generates a random floor call and dispatches it.
    /// </summary>
    /// <returns>The generated floor call.</returns>
    public FloorCall GenerateRandomCall()
    {
        int floor = _random.Next(Configuration.MinFloor, Configuration.MaxFloor + 1);
        
        // Determine valid directions for this floor
        Direction direction;
        if (floor == Configuration.MinFloor)
        {
            direction = Direction.Up;
        }
        else if (floor == Configuration.MaxFloor)
        {
            direction = Direction.Down;
        }
        else
        {
            direction = _random.Next(2) == 0 ? Direction.Up : Direction.Down;
        }

        var call = new FloorCall(floor, direction);
        RegisterCall(call);
        
        return call;
    }

    /// <summary>
    /// Registers a floor call and dispatches an elevator.
    /// </summary>
    /// <param name="call">The floor call to register.</param>
    public void RegisterCall(FloorCall call)
    {
        if (!call.IsValid)
        {
            _logger.LogWarning($"Invalid call rejected: Floor {call.Floor}, Direction {call.Direction}");
            return;
        }

        _logger.LogCallReceived(call.Floor, call.Direction);
        
        // Generate a random destination for the simulated passenger
        int destination = GenerateRandomDestination(call.Floor, call.Direction);
        
        var elevator = _controller.DispatchElevator(call);
        
        // When elevator arrives, passenger will select their destination
        // This is handled in the simulation loop
        if (elevator != null)
        {
            elevator.AddInternalDestination(destination);
        }
    }

    /// <summary>
    /// Generates a random destination floor based on the call direction.
    /// </summary>
    private int GenerateRandomDestination(int fromFloor, Direction direction)
    {
        if (direction == Direction.Up)
        {
            return _random.Next(fromFloor + 1, Configuration.MaxFloor + 1);
        }
        else
        {
            return _random.Next(Configuration.MinFloor, fromFloor);
        }
    }
}
