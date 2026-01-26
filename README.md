# Elevator Control System

A C# .NET 8 simulation of an elevator control system for a 10-floor building with 4 elevators.


## Project Structure

```
ElevatorSystem/
├── Models/
│   ├── Enums.cs           # Direction, ElevatorState enums
│   ├── Configuration.cs   # System constants
│   ├── FloorCall.cs       # Floor call record
│   └── Elevator.cs        # Elevator class with SCAN logic
├── Services/
│   ├── ElevatorController.cs  # Dispatch coordination
│   ├── Building.cs            # Call generation
│   ├── Simulation.cs          # Main simulation loop
│   └── Logger.cs              # Console output
└── Program.cs             # Entry point

ElevatorSystem.Tests/       # xUnit test project
├── ElevatorTests.cs
├── ElevatorControllerTests.cs
└── BuildingTests.cs
```

## Requirements

- .NET 8.0 SDK

## Running the Simulation

```bash
# Build the solution
dotnet build

# Run the simulation
dotnet run --project ElevatorSystem

# Press Ctrl+C to stop
```

## Running Tests

```bash
dotnet test
```

## Configuration

Edit `ElevatorSystem/Models/Configuration.cs` to adjust:

| Constant | Default | Description |
|----------|---------|-------------|
| `TotalFloors` | 10 | Number of floors in the building |
| `TotalElevators` | 4 | Number of elevator cars |
| `MovementTimeMs` | 10000 | Time to move one floor (ms) |
| `DoorTimeMs` | 10000 | Time doors stay open (ms) |
| `SpeedMultiplier` | 10.0 | Speed up simulation (10 = 10x faster) |
| `CallGenerationIntervalMs` | 5000 | Interval between random calls (ms) |

## Example Output

```
═══════════════════════════════════════════════════════════════
                    ELEVATOR STATUS
═══════════════════════════════════════════════════════════════
  Elevator 1: Floor  3 ⬆️ 🔄 → [5, 7]
  Elevator 2: Floor  6 ⬇️ ⏹️ → [2]
  Elevator 3: Floor  1 ⏸️ ⏹️ 
  Elevator 4: Floor  8 🚪 🚪 
═══════════════════════════════════════════════════════════════

[11:15:42.123] 📞 CALL: "Up" request on floor 4
[11:15:42.125] 🚀 DISPATCH: Elevator 3 assigned to floor 4 (Up)
[11:15:43.456] ⬆️ MOVE: Elevator 1 moving from floor 3 to floor 4
[11:15:44.789] 🔔 ARRIVAL: Elevator 1 arrived at floor 5
```

## Algorithm Details

### SCAN (Elevator) Algorithm

The elevator uses the SCAN algorithm (also known as the elevator algorithm):

1. Continue moving in the current direction
2. Stop at floors with matching destinations
3. When no more destinations in current direction, reverse
4. If no destinations at all, become idle

This ensures passengers inside the elevator aren't "yo-yoed" between floors and provides predictable, efficient service.

### Dispatch Optimization

When a new call arrives, the controller evaluates all elevators:

1. **Idle elevators**: Distance = physical distance - 1 (slight preference)
2. **Moving toward call**: Distance = physical distance (optimal)
3. **Moving away**: Distance = physical distance + travel to end + return (penalized)

The elevator with the lowest effective distance is dispatched.

## Design Decisions

- **Separation of Concerns**: Models, Services, and entry point are clearly separated
- **Testability**: Internal constructors allow dependency injection for testing
- **Thread Safety**: Logger uses locking for console output consistency
- **Immutable Records**: `FloorCall` is a record for value semantics
- **Configuration Centralization**: All constants in one place for easy tuning

## License

MIT
