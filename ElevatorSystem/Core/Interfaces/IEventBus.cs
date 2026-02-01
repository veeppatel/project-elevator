namespace ElevatorSystem.Core.Interfaces;

/// <summary>
/// Observer pattern: Simple event bus for decoupled event handling.
/// Components publish events; interested parties subscribe.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="evt">The event to publish.</param>
    void Publish<TEvent>(TEvent evt) where TEvent : IElevatorEvent;
    
    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">Event type to subscribe to.</typeparam>
    /// <param name="handler">Handler to invoke when event is published.</param>
    /// <returns>Subscription that can be disposed to unsubscribe.</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IElevatorEvent;
}

/// <summary>
/// Marker interface for elevator system events.
/// </summary>
public interface IElevatorEvent
{
    /// <summary>When the event occurred.</summary>
    DateTime Timestamp { get; }
}
