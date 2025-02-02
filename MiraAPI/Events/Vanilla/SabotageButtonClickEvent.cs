using System;

namespace MiraAPI.Events.Vanilla;

/// <summary>
/// Button click event for <see cref="SabotageButton"/> from Vanilla Among Us.
/// </summary>
[Obsolete("This class is deprecated. Use PlayerOpenSabotageEvent instead.")]
public class SabotageButtonClickEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="SabotageButton"/> that was clicked.
    /// </summary>
    public SabotageButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SabotageButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The SabotageButton.</param>
    public SabotageButtonClickEvent(SabotageButton button)
    {
        Button = button;
    }
}
