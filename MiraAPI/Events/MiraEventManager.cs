using System;
using System.Collections.Generic;
using Reactor.Utilities;

namespace MiraAPI.Events;

/// <summary>
/// Mira Event manager.
/// </summary>
public static class MiraEventManager
{
    private static readonly Dictionary<Type, List<Delegate>> EventHandlers = [];

    /// <summary>
    /// Invoke an event.
    /// </summary>
    /// <param name="eventInstance">The event instance.</param>
    /// <typeparam name="T">Type of Event.</typeparam>
    public static void InvokeEvent<T>(T eventInstance) where T : MiraEvent
    {
        EventHandlers.TryGetValue(typeof(T), out var handlers);
        if (handlers == null)
        {
            Logger<MiraApiPlugin>.Warning("No handlers for event " + typeof(T).Name);
            return;
        }

        foreach (var handler in handlers)
        {
            ((Action<T>)handler).Invoke(eventInstance);
        }
    }

    /// <summary>
    /// Register an event.
    /// </summary>
    /// <param name="handler">The callback method/handler for the event.</param>
    /// <typeparam name="T">Type of event.</typeparam>
    public static void RegisterEventHandler<T>(Action<T> handler) where T : MiraEvent
    {
        if (!EventHandlers.ContainsKey(typeof(T)))
        {
            EventHandlers.Add(typeof(T), []);
        }
        EventHandlers[typeof(T)].Add(handler);
        Logger<MiraApiPlugin>.Info("Registered event handler for " + typeof(T).Name);
    }
}
