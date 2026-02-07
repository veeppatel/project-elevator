using System.Collections.Concurrent;
using System.Collections.Immutable;
using ElevatorSystem.Core.Interfaces;

namespace ElevatorSystem.Core.Events;

/// <summary>
/// Simple in-memory event bus implementation.
/// Uses lock-free immutable collections for high-performance publish operations.
/// Thread-safe publish/subscribe for decoupled component communication.
/// </summary>
public sealed class EventBus : IEventBus
{
    // Using ImmutableArray for lock-free reads during Publish
    // ConcurrentDictionary handles the outer thread-safety
    private readonly ConcurrentDictionary<Type, ImmutableArray<Delegate>> _handlers = new();

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent evt) where TEvent : IElevatorEvent
    {
        var eventType = typeof(TEvent);
        
        // Lock-free read - ImmutableArray is inherently thread-safe
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
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
        
        // Use AddOrUpdate with immutable add - atomic operation
        _handlers.AddOrUpdate(
            eventType,
            _ => ImmutableArray.Create<Delegate>(handler),
            (_, existing) => existing.Add(handler)
        );
        
        return new Subscription(() => Unsubscribe(eventType, handler));
    }

    private void Unsubscribe(Type eventType, Delegate handler)
    {
        // Atomic remove using AddOrUpdate pattern
        _handlers.AddOrUpdate(
            eventType,
            _ => ImmutableArray<Delegate>.Empty,
            (_, existing) => existing.Remove(handler)
        );
    }

    /// <summary>
    /// Subscription handle that unsubscribes when disposed.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private int _disposed; // Using int for Interlocked

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            // Lock-free dispose using Interlocked
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _unsubscribe();
            }
        }
    }
}
