using MiraAPI.Hud;

namespace MiraAPI.Events.Mira;

/// <summary>
/// Button click event for Mira Buttons only. Do not use for vanilla buttons.
/// </summary>
/// <typeparam name="T">The Mira Button type.</typeparam>
public sealed class MiraButtonClickEvent<T> : MiraCancelableEvent where T : CustomActionButton
{
    /// <summary>
    /// Gets Mira Button that was clicked.
    /// </summary>
    public T Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraButtonClickEvent{T}"/> class.
    /// </summary>
    /// <param name="button">The Mira Button that was clicked.</param>
    public MiraButtonClickEvent(T button)
    {
        Button = button;
    }
}

/// <summary>
/// Button click event for Mira Buttons only. Do not use for vanilla buttons.
/// </summary>
public sealed class MiraButtonClickEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets Mira Button that was clicked.
    /// </summary>
    public CustomActionButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The Mira Button that was clicked.</param>
    public MiraButtonClickEvent(CustomActionButton button)
    {
        Button = button;
    }
}
