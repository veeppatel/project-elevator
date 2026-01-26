namespace ElevatorSystem.Models;

public class Elevator
{
    private readonly SortedSet<int> _upDestinations = new();
    private readonly SortedSet<int> _downDestinations = new();

    /// <summary>
    /// Unique identifier for this elevator (1-indexed).
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Current floor position of the elevator (1-indexed).
    /// </summary>
    public int CurrentFloor { get; private set; }

    /// <summary>
    /// Current direction of travel.
    /// </summary>
    public Direction Direction { get; private set; }

    /// <summary>
    /// Current operational state of the elevator.
    /// </summary>
    public ElevatorState State { get; private set; }

    /// <summary>
    /// Creates a new elevator starting at floor 1.
    /// </summary>
    /// <param name="id">The elevator identifier.</param>
    public Elevator(int id)
    {
        Id = id;
        CurrentFloor = 1;
        Direction = Direction.Idle;
        State = ElevatorState.Stopped;
    }

    /// <summary>
    /// Gets all pending destinations for this elevator.
    /// </summary>
    public IEnumerable<int> Destinations => _upDestinations.Union(_downDestinations);

    /// <summary>
    /// Checks if the elevator has any pending destinations.
    /// </summary>
    public bool HasDestinations => _upDestinations.Count > 0 || _downDestinations.Count > 0;

    /// <summary>
    /// Adds a destination floor based on the direction of travel.
    /// </summary>
    /// <param name="floor">Target floor.</param>
    /// <param name="direction">Direction the passenger wants to go.</param>
    public void AddDestination(int floor, Direction direction)
    {
        if (floor < Configuration.MinFloor || floor > Configuration.MaxFloor)
            return;

        if (direction == Direction.Up || (direction == Direction.Idle && floor > CurrentFloor))
        {
            _upDestinations.Add(floor);
        }
        else if (direction == Direction.Down || (direction == Direction.Idle && floor < CurrentFloor))
        {
            _downDestinations.Add(floor);
        }
        else if (floor == CurrentFloor)
        {
            // Already at destination floor - open doors immediately
            State = ElevatorState.DoorsOpen;
        }

        UpdateDirection();
    }

    /// <summary>
    /// Adds a destination floor for a passenger already inside (selecting floor button).
    /// </summary>
    /// <param name="floor">Target floor selected by passenger.</param>
    public void AddInternalDestination(int floor)
    {
        if (floor < Configuration.MinFloor || floor > Configuration.MaxFloor)
            return;

        if (floor > CurrentFloor)
        {
            _upDestinations.Add(floor);
        }
        else if (floor < CurrentFloor)
        {
            _downDestinations.Add(floor);
        }

        UpdateDirection();
    }

    /// <summary>
    /// Moves the elevator one floor in its current direction.
    /// Should only be called when State is Stopped and there are destinations.
    /// </summary>
    /// <returns>True if movement occurred, false otherwise.</returns>
    public bool Move()
    {
        if (State != ElevatorState.Stopped || Direction == Direction.Idle)
            return false;

        State = ElevatorState.Moving;

        if (Direction == Direction.Up && CurrentFloor < Configuration.MaxFloor)
        {
            CurrentFloor++;
        }
        else if (Direction == Direction.Down && CurrentFloor > Configuration.MinFloor)
        {
            CurrentFloor--;
        }

        return true;
    }

    /// <summary>
    /// Completes the movement and checks if the elevator should stop at the current floor.
    /// </summary>
    /// <returns>True if the elevator should stop at this floor.</returns>
    public bool CompleteMove()
    {
        State = ElevatorState.Stopped;
        return ShouldStopAtFloor();
    }

    /// <summary>
    /// Determines if the elevator should stop at the current floor.
    /// </summary>
    public bool ShouldStopAtFloor()
    {
        // Check if current floor is in our destinations
        if (Direction == Direction.Up && _upDestinations.Contains(CurrentFloor))
            return true;
        if (Direction == Direction.Down && _downDestinations.Contains(CurrentFloor))
            return true;

        // Also stop if we're at the extreme floor in our direction
        if (Direction == Direction.Up && _upDestinations.Count > 0 && CurrentFloor == _upDestinations.Max)
            return true;
        if (Direction == Direction.Down && _downDestinations.Count > 0 && CurrentFloor == _downDestinations.Min)
            return true;

        return false;
    }

    /// <summary>
    /// Opens the doors for passenger boarding/deboarding.
    /// </summary>
    public void OpenDoors()
    {
        State = ElevatorState.DoorsOpen;

        // Remove current floor from destinations
        _upDestinations.Remove(CurrentFloor);
        _downDestinations.Remove(CurrentFloor);
    }

    /// <summary>
    /// Closes the doors and prepares for movement.
    /// </summary>
    public void CloseDoors()
    {
        State = ElevatorState.Stopped;
        UpdateDirection();
    }

    /// <summary>
    /// Calculates the distance to a floor, considering current direction (for dispatch optimization).
    /// </summary>
    /// <param name="floor">Target floor.</param>
    /// <param name="direction">Requested direction at that floor.</param>
    /// <returns>Effective distance (lower is better).</returns>
    public int CalculateEffectiveDistance(int floor, Direction direction)
    {
        int physicalDistance = Math.Abs(CurrentFloor - floor);

        // Idle elevator - just physical distance
        if (Direction == Direction.Idle)
            return physicalDistance;

        // Moving toward the floor in compatible direction
        bool isOnTheWay = Direction switch
        {
            Direction.Up => floor > CurrentFloor && (direction == Direction.Up || direction == Direction.Idle),
            Direction.Down => floor < CurrentFloor && (direction == Direction.Down || direction == Direction.Idle),
            _ => false
        };

        if (isOnTheWay)
            return physicalDistance;

        // Need to reverse direction - add penalty
        int penalty = Direction switch
        {
            Direction.Up => (_upDestinations.Count > 0 ? _upDestinations.Max - CurrentFloor : 0) +
                           (_upDestinations.Count > 0 ? _upDestinations.Max : CurrentFloor) - floor,
            Direction.Down => (_downDestinations.Count > 0 ? CurrentFloor - _downDestinations.Min : 0) +
                             floor - (_downDestinations.Count > 0 ? _downDestinations.Min : CurrentFloor),
            _ => 0
        };

        return physicalDistance + Math.Abs(penalty);
    }

    /// <summary>
    /// Updates the direction based on current destinations (SCAN algorithm).
    /// </summary>
    private void UpdateDirection()
    {
        if (!HasDestinations)
        {
            Direction = Direction.Idle;
            return;
        }

        // SCAN algorithm: continue in current direction if there are destinations that way
        if (Direction == Direction.Up)
        {
            // Any destinations above current floor?
            if (_upDestinations.Any(f => f > CurrentFloor) || _upDestinations.Contains(CurrentFloor))
                return; // Keep going up

            // Otherwise reverse
            Direction = _downDestinations.Count > 0 ? Direction.Down : Direction.Idle;
        }
        else if (Direction == Direction.Down)
        {
            // Any destinations below current floor?
            if (_downDestinations.Any(f => f < CurrentFloor) || _downDestinations.Contains(CurrentFloor))
                return; // Keep going down

            // Otherwise reverse
            Direction = _upDestinations.Count > 0 ? Direction.Up : Direction.Idle;
        }
        else // Idle
        {
            // Pick direction based on which destination list has items
            if (_upDestinations.Count > 0)
                Direction = Direction.Up;
            else if (_downDestinations.Count > 0)
                Direction = Direction.Down;
        }
    }

    public override string ToString() => $"Elevator {Id}: Floor {CurrentFloor}, {Direction}, {State}";
}
