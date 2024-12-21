namespace MiraAPI.Events.Vanilla;

/// <summary>
/// Button click event for <see cref="VentButton"/> from Vanilla Among Us.
/// </summary>
public class VentButtonClickEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="VentButton"/> that was clicked.
    /// </summary>
    public VentButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VentButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The VentButton.</param>
    public VentButtonClickEvent(VentButton button)
    {
        Button = button;
    }
}
