using MiraAPI.Hud;

namespace MiraAPI.Events.Mira;

public class MiraButtonCancelledEvent<T> : MiraEvent where T : CustomActionButton
{
    public T Button { get; }

    public MiraButtonCancelledEvent(T button)
    {
        Button = button;
    }
}
