using System;
using System.Collections.Generic;

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
        foreach (var handler in EventHandlers[typeof(T)])
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
    }
}
