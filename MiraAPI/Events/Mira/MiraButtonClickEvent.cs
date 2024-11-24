using MiraAPI.Hud;

namespace MiraAPI.Events.Mira;

public class MiraButtonClickEvent<T> : MiraCancelableEvent where T : CustomActionButton
{
    public T Button { get; }

    public MiraButtonClickEvent(T button)
    {
        Button = button;
    }
}
