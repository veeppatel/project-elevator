using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Runs the elevator simulation, coordinating timing and events.
/// </summary>
public class Simulation
{
    private readonly Building _building;
    private readonly ElevatorController _controller;
    private readonly Logger _logger;
    private readonly CancellationTokenSource _cts;
    
    private readonly int _movementDelayMs;
    private readonly int _doorDelayMs;
    private readonly int _callIntervalMs;
    private readonly int _statusIntervalMs;

    /// <summary>
    /// Creates a new simulation instance.
    /// </summary>
    public Simulation(Building building, ElevatorController controller, Logger logger)
    {
        _building = building;
        _controller = controller;
        _logger = logger;
        _cts = new CancellationTokenSource();

        // Apply speed multiplier to all timings
        double multiplier = Configuration.SpeedMultiplier;
        _movementDelayMs = (int)(Configuration.MovementTimeMs / multiplier);
        _doorDelayMs = (int)(Configuration.DoorTimeMs / multiplier);
        _callIntervalMs = (int)(Configuration.CallGenerationIntervalMs / multiplier);
        _statusIntervalMs = 2000; // Status update every 2 seconds (fixed)
    }

    /// <summary>
    /// Starts the simulation. Returns when cancelled.
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogSimulationStart();

        // Start background tasks
        var elevatorTask = RunElevatorsAsync(_cts.Token);
        var callGeneratorTask = RunCallGeneratorAsync(_cts.Token);
        var statusTask = RunStatusDisplayAsync(_cts.Token);

        try
        {
            await Task.WhenAll(elevatorTask, callGeneratorTask, statusTask);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }

        _logger.LogSimulationStop();
    }

    /// <summary>
    /// Stops the simulation gracefully.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// Runs all elevator movement logic.
    /// </summary>
    private async Task RunElevatorsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var elevator in _controller.Elevators)
            {
                await ProcessElevatorAsync(elevator, token);
            }

            // Small delay between processing cycles
            await Task.Delay(100, token);
        }
    }

    /// <summary>
    /// Processes a single elevator's state and movement.
    /// </summary>
    private async Task ProcessElevatorAsync(Elevator elevator, CancellationToken token)
    {
        switch (elevator.State)
        {
            case ElevatorState.Stopped when elevator.HasDestinations:
                // Start moving
                int fromFloor = elevator.CurrentFloor;
                if (elevator.Move())
                {
                    _logger.LogElevatorMoving(elevator.Id, fromFloor, elevator.CurrentFloor);
                    await Task.Delay(_movementDelayMs, token);
                    
                    if (elevator.CompleteMove())
                    {
                        // Arrived at a destination floor
                        _logger.LogElevatorArrival(elevator.Id, elevator.CurrentFloor);
                        elevator.OpenDoors();
                        _logger.LogDoorsOpen(elevator.Id, elevator.CurrentFloor);
                    }
                }
                break;

            case ElevatorState.DoorsOpen:
                // Wait for passengers, then close doors
                await Task.Delay(_doorDelayMs, token);
                elevator.CloseDoors();
                _logger.LogDoorsClosed(elevator.Id);
                break;
        }
    }

    /// <summary>
    /// Generates random calls at intervals.
    /// </summary>
    private async Task RunCallGeneratorAsync(CancellationToken token)
    {
        // Initial delay before first call
        await Task.Delay(_callIntervalMs, token);

        while (!token.IsCancellationRequested)
        {
            _building.GenerateRandomCall();
            
            // Randomize next call interval slightly (50% to 150% of base interval)
            int nextInterval = _callIntervalMs / 2 + Random.Shared.Next(_callIntervalMs);
            await Task.Delay(nextInterval, token);
        }
    }

    /// <summary>
    /// Periodically displays system status.
    /// </summary>
    private async Task RunStatusDisplayAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(_statusIntervalMs, token);
            _logger.LogElevatorPositions(_controller.Elevators);
        }
    }
}
