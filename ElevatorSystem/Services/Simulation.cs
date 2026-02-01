using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Runs the elevator simulation, coordinating timing and events.
/// Uses async/await for non-blocking elevator processing.
/// </summary>
public sealed class Simulation : IDisposable
{
    private readonly Building _building;
    private readonly ElevatorController _controller;
    private readonly ElevatorLogger _logger;
    private readonly CancellationTokenSource _cts;
    
    private readonly int _callIntervalMs;
    private readonly int _statusIntervalMs;
    private readonly double _speedMultiplier;

    /// <summary>
    /// Creates a new simulation instance.
    /// </summary>
    public Simulation(
        Building building, 
        ElevatorController controller, 
        ElevatorLogger logger,
        double speedMultiplier = Configuration.SpeedMultiplier)
    {
        _building = building;
        _controller = controller;
        _logger = logger;
        _cts = new CancellationTokenSource();
        _speedMultiplier = speedMultiplier;

        // Apply speed multiplier to intervals
        _callIntervalMs = (int)(Configuration.CallGenerationIntervalMs / speedMultiplier);
        _statusIntervalMs = (int)(3000 / speedMultiplier); // Status every 3 seconds (adjusted)
    }

    /// <summary>
    /// Starts the simulation. Returns when cancelled.
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogSimulationStart(_controller.DispatchStrategy.Name, _speedMultiplier);

        var token = _cts.Token;

        // Start all elevator processors
        var elevatorTasks = _controller.Elevators
            .Select(e => RunElevatorAsync(e, token))
            .ToList();

        // Start call generator
        var callGeneratorTask = RunCallGeneratorAsync(token);
        
        // Start status display
        var statusTask = RunStatusDisplayAsync(token);

        // Start cleanup task
        var cleanupTask = RunCleanupAsync(token);

        try
        {
            await Task.WhenAll(
                elevatorTasks.Concat(new[] { callGeneratorTask, statusTask, cleanupTask })
            );
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
    /// Runs a single elevator's processing loop.
    /// </summary>
    private async Task RunElevatorAsync(IElevator elevator, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await elevator.ProcessAsync(token);
                
                // Small delay between processing cycles to prevent tight loops
                await Task.Delay(50, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
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
            try
            {
                _building.GenerateRandomCall();
                
                // Randomize next call interval (50% to 150% of base interval)
                int nextInterval = _callIntervalMs / 2 + Random.Shared.Next(_callIntervalMs);
                await Task.Delay(nextInterval, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Periodically displays system status.
    /// </summary>
    private async Task RunStatusDisplayAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_statusIntervalMs, token);
                
                var status = _controller.GetSystemStatus();
                _logger.DisplayBuildingStatus(status);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Periodically cleans up serviced calls.
    /// </summary>
    private async Task RunCleanupAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, token);
                _controller.CleanupServicedCalls();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}
