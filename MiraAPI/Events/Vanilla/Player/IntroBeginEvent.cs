namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when the intro cutscene is shown. Non cancellable.
/// </summary>
public class IntroBeginEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the IntroCutscene.
    /// </summary>
    public IntroCutscene IntroCutscene { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroBeginEvent"/> class.
    /// </summary>
    /// <param name="controller">The exile controller.</param>
    public IntroBeginEvent(IntroCutscene introCutscene)
    {
        IntroCutscene = introCutscene;
    }
}
