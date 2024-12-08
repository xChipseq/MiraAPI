namespace MiraAPI.Events.Vanilla;

/// <summary>
/// Button click event for <see cref="UseButton"/> from Vanilla Among Us.
/// </summary>
public class UseButtonClickEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="UseButton"/> that was clicked.
    /// </summary>
    public UseButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UseButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The UseButton.</param>
    public UseButtonClickEvent(UseButton button)
    {
        Button = button;
    }
}
