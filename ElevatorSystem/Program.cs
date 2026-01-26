using ElevatorSystem.Models;
using ElevatorSystem.Services;

namespace ElevatorSystem;

/// <summary>
/// Entry point for the elevator control system simulation.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Set up the system components
        var logger = new Logger();
        var controller = new ElevatorController(Configuration.TotalElevators, logger);
        var building = new Building(Configuration.TotalFloors, controller, logger);
        var simulation = new Simulation(building, controller, logger);

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
