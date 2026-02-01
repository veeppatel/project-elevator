using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Core.Strategies;
using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem;

/// <summary>
/// Entry point for the elevator control system simulation.
/// Demonstrates Dependency Injection pattern by wiring up all components.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Parse command line arguments for strategy selection
        string strategyArg = args.FirstOrDefault()?.ToLower() ?? "scan";
        
        // Create the event bus (Observer pattern)
        IEventBus eventBus = new EventBus();
        
        // Select dispatch strategy (Strategy pattern)
        IDispatchStrategy dispatchStrategy = strategyArg switch
        {
            "nearest" => new NearestDispatchStrategy(),
            _ => new ScanDispatchStrategy()
        };

        // Calculate timing with speed multiplier
        double speedMultiplier = Configuration.SpeedMultiplier;
        int movementDelayMs = (int)(Configuration.MovementTimeMs / speedMultiplier);
        int doorDelayMs = (int)(Configuration.DoorTimeMs / speedMultiplier);

        // Create the elevator controller (Dependency Injection)
        var controller = new ElevatorController(
            Configuration.TotalElevators, 
            eventBus, 
            dispatchStrategy,
            movementDelayMs,
            doorDelayMs);

        // Create the building
        var building = new Building(Configuration.TotalFloors, controller, eventBus);

        // Create the logger (subscribes to events)
        using var logger = new ElevatorLogger(eventBus);

        // Create and run the simulation
        using var simulation = new Simulation(building, controller, logger, speedMultiplier);

        // Handle graceful shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            simulation.Stop();
        };

        // Run the simulation
        await simulation.RunAsync();
    }
}
