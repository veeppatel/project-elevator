using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Controls and coordinates multiple elevators. Handles call dispatching
/// using an optimized algorithm that considers proximity and direction.
/// </summary>
public class ElevatorController
{
    private readonly List<Elevator> _elevators;
    private readonly Logger _logger;

    /// <summary>
    /// Gets a read-only view of all elevators.
    /// </summary>
    public IReadOnlyList<Elevator> Elevators => _elevators.AsReadOnly();

    /// <summary>
    /// Creates a new elevator controller with the specified number of elevators.
    /// </summary>
    /// <param name="elevatorCount">Number of elevators to manage.</param>
    /// <param name="logger">Logger for output.</param>
    public ElevatorController(int elevatorCount, Logger logger)
    {
        _logger = logger;
        _elevators = new List<Elevator>();
        
        for (int i = 1; i <= elevatorCount; i++)
        {
            _elevators.Add(new Elevator(i));
        }
    }

    /// <summary>
    /// Constructor for testing - allows injecting pre-configured elevators.
    /// </summary>
    internal ElevatorController(List<Elevator> elevators, Logger logger)
    {
        _elevators = elevators;
        _logger = logger;
    }

    /// <summary>
    /// Dispatches an elevator to respond to a floor call.
    /// Uses an optimized algorithm that prioritizes:
    /// 1. Idle elevators closest to the floor
    /// 2. Elevators moving toward the floor in the same direction
    /// 3. Any available elevator (by distance)
    /// </summary>
    /// <param name="call">The floor call to service.</param>
    /// <returns>The dispatched elevator, or null if none available.</returns>
    public Elevator? DispatchElevator(FloorCall call)
    {
        if (!call.IsValid)
            return null;

        Elevator? bestElevator = null;
        int bestScore = int.MaxValue;

        foreach (var elevator in _elevators)
        {
            int score = elevator.CalculateEffectiveDistance(call.Floor, call.Direction);

            // Bonus for idle elevators (prefer them slightly)
            if (elevator.Direction == Direction.Idle)
                score -= 1;

            if (score < bestScore)
            {
                bestScore = score;
                bestElevator = elevator;
            }
        }

        if (bestElevator != null)
        {
            bestElevator.AddDestination(call.Floor, call.Direction);
            _logger.LogElevatorDispatched(bestElevator.Id, call.Floor, call.Direction);
        }

        return bestElevator;
    }

    /// <summary>
    /// Simulates a passenger inside an elevator pressing a floor button.
    /// </summary>
    /// <param name="elevatorId">The elevator the passenger is in.</param>
    /// <param name="destinationFloor">The desired destination floor.</param>
    public void AddPassengerDestination(int elevatorId, int destinationFloor)
    {
        var elevator = _elevators.FirstOrDefault(e => e.Id == elevatorId);
        elevator?.AddInternalDestination(destinationFloor);
        
        if (elevator != null)
        {
            _logger.LogPassengerDestination(elevatorId, destinationFloor);
        }
    }

    /// <summary>
    /// Gets the current status of all elevators for display.
    /// </summary>
    /// <returns>Formatted status string.</returns>
    public string GetSystemStatus()
    {
        return string.Join("\n", _elevators.Select(e => e.ToString()));
    }
}
