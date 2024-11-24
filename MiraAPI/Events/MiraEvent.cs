namespace MiraAPI.Events;

public abstract class MiraEvent;

public abstract class MiraCancelableEvent : MiraEvent
{
    public bool IsCancelled { get; private set; }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public void UnCancel()
    {
        IsCancelled = false;
    }
}
