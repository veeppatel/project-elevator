using ElevatorSystem.Core.Events;
using ElevatorSystem.Core.Interfaces;
using ElevatorSystem.Models;

namespace ElevatorSystem.Services;

/// <summary>
/// Enhanced logger with visual building display and structured output.
/// Subscribes to elevator events for decoupled logging.
/// </summary>
public sealed class ElevatorLogger : IDisposable
{
    private readonly object _lock = new();
    private readonly List<IDisposable> _subscriptions = new();
    
    // Track hall calls for visual display
    private readonly HashSet<(int Floor, ElevatorDirection Direction)> _activeHallCalls = new();

    public ElevatorLogger(IEventBus eventBus)
    {
        // Subscribe to all relevant events
        _subscriptions.Add(eventBus.Subscribe<ElevatorMovingEvent>(OnElevatorMoving));
        _subscriptions.Add(eventBus.Subscribe<ElevatorArrivedEvent>(OnElevatorArrived));
        _subscriptions.Add(eventBus.Subscribe<DoorsOpenedEvent>(OnDoorsOpened));
        _subscriptions.Add(eventBus.Subscribe<DoorsClosedEvent>(OnDoorsClosed));
        _subscriptions.Add(eventBus.Subscribe<ElevatorIdleEvent>(OnElevatorIdle));
        _subscriptions.Add(eventBus.Subscribe<DirectionChangedEvent>(OnDirectionChanged));
        _subscriptions.Add(eventBus.Subscribe<HallCallReceivedEvent>(OnHallCallReceived));
        _subscriptions.Add(eventBus.Subscribe<HallCallAssignedEvent>(OnHallCallAssigned));
        _subscriptions.Add(eventBus.Subscribe<HallCallServicedEvent>(OnHallCallServiced));
        _subscriptions.Add(eventBus.Subscribe<CabCallAddedEvent>(OnCabCallAdded));
        _subscriptions.Add(eventBus.Subscribe<PassengerDeliveredEvent>(OnPassengerDelivered));
    }

    #region Event Handlers

    private void OnElevatorMoving(ElevatorMovingEvent evt)
    {
        string arrow = evt.Direction == ElevatorDirection.Up ? "▲" : "▼";
        string stops = evt.PendingStops.Any() ? $"[{string.Join(",", evt.PendingStops.Take(5))}{(evt.PendingStops.Count > 5 ? "..." : "")}]" : "";
        LogEvent($"E{evt.ElevatorId}", "MOVING", $"{evt.FromFloor}→{evt.ToFloor} {arrow} {stops}", ConsoleColor.Blue);
    }

    private void OnElevatorArrived(ElevatorArrivedEvent evt)
    {
        string arrow = evt.Direction == ElevatorDirection.Up ? "▲" : "▼";
        LogEvent($"E{evt.ElevatorId}", "ARRIVED", $"Floor {evt.Floor} {arrow}", ConsoleColor.Cyan);
    }

    private void OnDoorsOpened(DoorsOpenedEvent evt)
    {
        string action = "";
        if (evt.PassengersBoarding > 0) action += $"+{evt.PassengersBoarding}";
        if (evt.PassengersAlighting > 0) action += $" -{evt.PassengersAlighting}";
        LogEvent($"E{evt.ElevatorId}", "DOORS_OPEN", $"Floor {evt.Floor} {action}".Trim(), ConsoleColor.Green);
    }

    private void OnDoorsClosed(DoorsClosedEvent evt)
    {
        LogEvent($"E{evt.ElevatorId}", "DOORS_CLOSED", $"Floor {evt.Floor}", ConsoleColor.DarkGray);
    }

    private void OnElevatorIdle(ElevatorIdleEvent evt)
    {
        LogEvent($"E{evt.ElevatorId}", "IDLE", $"Floor {evt.Floor}", ConsoleColor.Gray);
    }

    private void OnDirectionChanged(DirectionChangedEvent evt)
    {
        string oldDir = evt.OldDirection == ElevatorDirection.Up ? "▲" : "▼";
        string newDir = evt.NewDirection == ElevatorDirection.Up ? "▲" : "▼";
        LogEvent($"E{evt.ElevatorId}", "REVERSE", $"{oldDir}→{newDir} at floor {evt.Floor}", ConsoleColor.Yellow);
    }

    private void OnHallCallReceived(HallCallReceivedEvent evt)
    {
        string arrow = evt.Direction == ElevatorDirection.Up ? "▲" : "▼";
        lock (_lock)
        {
            _activeHallCalls.Add((evt.Floor, evt.Direction));
        }
        LogEvent("SYS", "HALL_CALL", $"Floor {evt.Floor} {arrow} requested", ConsoleColor.Yellow);
    }

    private void OnHallCallAssigned(HallCallAssignedEvent evt)
    {
        string arrow = evt.Direction == ElevatorDirection.Up ? "▲" : "▼";
        string eta = evt.EstimatedStops > 0 ? $" (ETA: {evt.EstimatedStops} stops)" : "";
        LogEvent("SYS", "ASSIGNED", $"Floor {evt.Floor} {arrow} → E{evt.ElevatorId}{eta}", ConsoleColor.Cyan);
    }

    private void OnHallCallServiced(HallCallServicedEvent evt)
    {
        lock (_lock)
        {
            _activeHallCalls.RemoveWhere(c => c.Floor == evt.Floor);
        }
        LogEvent($"E{evt.ElevatorId}", "PICKUP", $"Floor {evt.Floor} (waited {evt.WaitTime.TotalSeconds:F1}s)", ConsoleColor.Green);
    }

    private void OnCabCallAdded(CabCallAddedEvent evt)
    {
        LogEvent($"E{evt.ElevatorId}", "CAB_CALL", $"Passenger selected floor {evt.DestinationFloor}", ConsoleColor.Magenta);
    }

    private void OnPassengerDelivered(PassengerDeliveredEvent evt)
    {
        LogEvent($"E{evt.ElevatorId}", "DELIVERED", $"Passenger to floor {evt.DestinationFloor}", ConsoleColor.Green);
    }

    #endregion

    /// <summary>
    /// Logs an event with structured format.
    /// </summary>
    private void LogEvent(string source, string eventType, string details, ConsoleColor color)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{timestamp}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{source,-4} ");
            Console.ForegroundColor = color;
            Console.Write($"{eventType,-12} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(details);
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Displays visual building status with elevator positions.
    /// </summary>
    public void DisplayBuildingStatus(SystemStatusEvent status)
    {
        lock (_lock)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            
            // Simple fixed-width display
            Console.WriteLine("┌──────┬───────────────────────────────────────────────────────────┐");
            Console.WriteLine("│FLOOR │  E1    E2    E3    E4  │ CALLS  │ STATUS                 │");
            Console.WriteLine("├──────┼───────────────────────────────────────────────────────────┤");

            // Display each floor from top to bottom
            for (int floor = Configuration.MaxFloor; floor >= Configuration.MinFloor; floor--)
            {
                DisplayFloorRowSimple(floor, status.Elevators);
            }

            Console.WriteLine("└──────┴───────────────────────────────────────────────────────────┘");
            
            // Legend
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Pending Calls: {status.PendingHallCalls} | [▲]=Up [▼]=Down [●]=Idle [◐]=DoorsOpen");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private void DisplayFloorRowSimple(int floor, IReadOnlyList<ElevatorSnapshot> elevators)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"│  {floor,2}  │ ");

        // Show each elevator's position with fixed width
        foreach (var elevator in elevators)
        {
            if (elevator.CurrentFloor == floor)
            {
                ConsoleColor color = elevator.State switch
                {
                    ElevatorStateType.DoorsOpen => ConsoleColor.Green,
                    ElevatorStateType.Moving => ConsoleColor.Cyan,
                    _ => ConsoleColor.Yellow
                };
                Console.ForegroundColor = color;
                
                string symbol = elevator.State switch
                {
                    ElevatorStateType.DoorsOpen => "◐",
                    ElevatorStateType.Moving => elevator.Direction == ElevatorDirection.Up ? "▲" : "▼",
                    _ => "●"
                };
                Console.Write($" [{elevator.Id}]{symbol}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  ·  ");
            }
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" │ ");
        
        // Show hall calls at this floor
        bool hasUpCall = _activeHallCalls.Contains((floor, ElevatorDirection.Up));
        bool hasDownCall = _activeHallCalls.Contains((floor, ElevatorDirection.Down));
        
        if (hasUpCall || hasDownCall)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(hasUpCall ? "▲" : " ");
            Console.Write(hasDownCall ? "▼" : " ");
            Console.Write("   ");
        }
        else
        {
            Console.Write("      ");
        }
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");

        // Show elevator details if at this floor (truncated)
        var elevatorsAtFloor = elevators.Where(e => e.CurrentFloor == floor).ToList();
        if (elevatorsAtFloor.Any())
        {
            Console.ForegroundColor = ConsoleColor.White;
            var details = elevatorsAtFloor
                .Select(e => $"E{e.Id}→{(e.Destinations.Any() ? string.Join(",", e.Destinations.Take(3)) : "-")}")
                .Take(2);
            Console.Write(string.Join(" ", details).PadRight(22));
        }
        else
        {
            Console.Write("                      ");
        }
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" │");
    }

    /// <summary>
    /// Displays simulation start banner.
    /// </summary>
    public void LogSimulationStart(string strategyName, double speedMultiplier)
    {
        lock (_lock)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║              ELEVATOR CONTROL SYSTEM - PRODUCTION GRADE                       ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║  • 10 Floors  • 4 Elevators  • State Machine Architecture                     ║
║  • Dispatch Strategy: {strategyName,-10}  • Speed: {speedMultiplier}x                              ║
║  • Design Patterns: State, Strategy, Observer, DI                             ║
║                                                                               ║
║  Press Ctrl+C to stop the simulation                                          ║
╚═══════════════════════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Displays simulation stop message.
    /// </summary>
    public void LogSimulationStop()
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n🛑 Simulation stopped.");
            Console.ResetColor();
        }
    }

    public void Dispose()
    {
        foreach (var sub in _subscriptions)
        {
            sub.Dispose();
        }
        _subscriptions.Clear();
    }
}
