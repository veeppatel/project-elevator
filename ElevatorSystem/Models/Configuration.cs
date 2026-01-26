namespace ElevatorSystem.Models;

/// <summary>
/// Configuration constants for the elevator system.
/// </summary>
public static class Configuration
{
    /// <summary>Total number of floors in the building (1-indexed).</summary>
    public const int TotalFloors = 10;
    
    /// <summary>Number of elevator cars in the building.</summary>
    public const int TotalElevators = 4;
    
    /// <summary>Time in milliseconds for an elevator to move one floor.</summary>
    public const int MovementTimeMs = 10000;
    
    /// <summary>Time in milliseconds for doors to stay open for passengers.</summary>
    public const int DoorTimeMs = 10000;
    
    /// <summary>
    /// Speed multiplier for simulation (1.0 = real-time, 10.0 = 10x faster).
    /// Useful for testing and demonstration.
    /// </summary>
    public const double SpeedMultiplier = 10.0;
    
    /// <summary>Minimum floor number.</summary>
    public const int MinFloor = 1;
    
    /// <summary>Maximum floor number.</summary>
    public const int MaxFloor = TotalFloors;
    
    /// <summary>Interval between random call generations in milliseconds.</summary>
    public const int CallGenerationIntervalMs = 5000;
}
