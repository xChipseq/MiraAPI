namespace MiraAPI.Events.Vanilla;

public class BeforeMurderEvent : MiraCancelableEvent
{
    public PlayerControl Source { get; }
    public PlayerControl Target { get; }
}
