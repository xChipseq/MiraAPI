namespace MiraAPI.Events.Vanilla;

/// <summary>
/// Button click event for <see cref="ReportButton"/> from Vanilla Among Us.
/// </summary>
public class ReportButtonClickEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="ReportButton"/> that was clicked.
    /// </summary>
    public ReportButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The ReportButton.</param>
    public ReportButtonClickEvent(ReportButton button)
    {
        Button = button;
    }
}
