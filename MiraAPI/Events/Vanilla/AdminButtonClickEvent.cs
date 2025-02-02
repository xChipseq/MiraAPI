using System;

namespace MiraAPI.Events.Vanilla;

/// <summary>
/// Button click event for <see cref="AdminButton"/> from Vanilla Among Us.
/// </summary>
[Obsolete("This class is deprecated. Use PlayerUseEvent instead.")]
public class AdminButtonClickEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="AdminButton"/> that was clicked.
    /// </summary>
    public AdminButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The AdminButton.</param>
    public AdminButtonClickEvent(AdminButton button)
    {
        Button = button;
    }
}
