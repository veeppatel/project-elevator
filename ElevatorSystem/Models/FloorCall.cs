namespace ElevatorSystem.Models;

/// <summary>
/// Represents a call for an elevator from a specific floor with a desired direction.
/// </summary>
/// <param name="Floor">The floor number where the call originated (1-indexed).</param>
/// <param name="Direction">The direction the passenger wants to travel.</param>
public record FloorCall(int Floor, Direction Direction)
{
    /// <summary>
    /// Validates that the floor call is within valid bounds.
    /// </summary>
    public bool IsValid => Floor >= Configuration.MinFloor 
                          && Floor <= Configuration.MaxFloor 
                          && Direction != Direction.Idle;
}
