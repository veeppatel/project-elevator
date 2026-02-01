using System.Collections.Concurrent;
using ElevatorSystem.Core.Interfaces;

namespace ElevatorSystem.Core.Events;

/// <summary>
/// Simple in-memory event bus implementation.
/// Thread-safe publish/subscribe for decoupled component communication.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent evt) where TEvent : IElevatorEvent
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            // Create a snapshot to avoid issues if handlers modify the list
            List<Delegate> snapshot;
            lock (_lock)
            {
                snapshot = handlers.ToList();
            }
            
            foreach (var handler in snapshot)
            {
                try
                {
                    ((Action<TEvent>)handler)(evt);
                }
                catch (Exception ex)
                {
                    // Log but don't throw - one handler failing shouldn't affect others
                    Console.Error.WriteLine($"Event handler error for {eventType.Name}: {ex.Message}");
                }
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IElevatorEvent
    {
        var eventType = typeof(TEvent);
        
        lock (_lock)
        {
            if (!_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers[eventType] = handlers;
            }
            
            handlers.Add(handler);
        }
        
        return new Subscription(() => Unsubscribe(eventType, handler));
    }

    private void Unsubscribe(Type eventType, Delegate handler)
    {
        lock (_lock)
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    /// <summary>
    /// Subscription handle that unsubscribes when disposed.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _unsubscribe();
                _disposed = true;
            }
        }
    }
}
