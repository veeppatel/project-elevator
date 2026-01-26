using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Handles console output with color coding for easy visualization.
/// </summary>
public class Logger
{
    private readonly object _lock = new();

    /// <summary>
    /// Logs a floor call request.
    /// </summary>
    public void LogCallReceived(int floor, Direction direction)
    {
        Log(ConsoleColor.Yellow, $"📞 CALL: \"{direction}\" request on floor {floor}");
    }

    /// <summary>
    /// Logs elevator dispatch.
    /// </summary>
    public void LogElevatorDispatched(int elevatorId, int floor, Direction direction)
    {
        Log(ConsoleColor.Cyan, $"🚀 DISPATCH: Elevator {elevatorId} assigned to floor {floor} ({direction})");
    }

    /// <summary>
    /// Logs elevator movement.
    /// </summary>
    public void LogElevatorMoving(int elevatorId, int fromFloor, int toFloor)
    {
        Log(ConsoleColor.Blue, $"⬆️ MOVE: Elevator {elevatorId} moving from floor {fromFloor} to floor {toFloor}");
    }

    /// <summary>
    /// Logs elevator arrival at a floor.
    /// </summary>
    public void LogElevatorArrival(int elevatorId, int floor)
    {
        Log(ConsoleColor.Green, $"🔔 ARRIVAL: Elevator {elevatorId} arrived at floor {floor}");
    }

    /// <summary>
    /// Logs doors opening.
    /// </summary>
    public void LogDoorsOpen(int elevatorId, int floor)
    {
        Log(ConsoleColor.Green, $"🚪 DOORS OPEN: Elevator {elevatorId} doors open on floor {floor}");
    }

    /// <summary>
    /// Logs doors closing.
    /// </summary>
    public void LogDoorsClosed(int elevatorId)
    {
        Log(ConsoleColor.DarkGray, $"🚪 DOORS CLOSED: Elevator {elevatorId} doors closed");
    }

    /// <summary>
    /// Logs a passenger destination selection.
    /// </summary>
    public void LogPassengerDestination(int elevatorId, int floor)
    {
        Log(ConsoleColor.Magenta, $"👤 PASSENGER: Elevator {elevatorId} - passenger selected floor {floor}");
    }

    /// <summary>
    /// Logs current positions of all elevators.
    /// </summary>
    public void LogElevatorPositions(IReadOnlyList<Elevator> elevators)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    ELEVATOR STATUS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            
            foreach (var elevator in elevators)
            {
                string directionIcon = elevator.Direction switch
                {
                    Direction.Up => "⬆️",
                    Direction.Down => "⬇️",
                    _ => "⏸️"
                };

                string stateIcon = elevator.State switch
                {
                    ElevatorState.Moving => "🔄",
                    ElevatorState.DoorsOpen => "🚪",
                    _ => "⏹️"
                };

                var destinations = elevator.Destinations.ToList();
                string destStr = destinations.Any() 
                    ? $"→ [{string.Join(", ", destinations)}]" 
                    : "";

                Console.WriteLine($"  Elevator {elevator.Id}: Floor {elevator.CurrentFloor,2} {directionIcon} {stateIcon} {destStr}");
            }
            
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void LogWarning(string message)
    {
        Log(ConsoleColor.DarkYellow, $"⚠️ WARNING: {message}");
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void LogInfo(string message)
    {
        Log(ConsoleColor.Gray, $"ℹ️ {message}");
    }

    /// <summary>
    /// Logs a simulation start message.
    /// </summary>
    public void LogSimulationStart()
    {
        lock (_lock)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════╗
║           ELEVATOR CONTROL SYSTEM SIMULATION                  ║
║                                                               ║
║  • 10 Floors  • 4 Elevators                                   ║
║  • 10s per floor  • 10s door time                             ║
║                                                               ║
║  Press Ctrl+C to stop the simulation                          ║
╚═══════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Logs a simulation stop message.
    /// </summary>
    public void LogSimulationStop()
    {
        Log(ConsoleColor.Red, "\n🛑 Simulation stopped.");
    }

    private void Log(ConsoleColor color, string message)
    {
        lock (_lock)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{timestamp}] ");
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
