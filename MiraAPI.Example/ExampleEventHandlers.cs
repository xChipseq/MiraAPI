using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla;
using MiraAPI.Example.Buttons.Freezer;
using Reactor.Utilities;

namespace MiraAPI.Example;

public static class ExampleEventHandlers
{
    public static void Initialize()
    {
        // Register event handlers here
        MiraEventManager.RegisterEventHandler<MiraButtonClickEvent<FreezeButton>>(FreezeButtonClickHandler, 1);
        MiraEventManager.RegisterEventHandler<MiraButtonCancelledEvent<FreezeButton>>(FreezeButtonCancelledHandler);
        MiraEventManager.RegisterEventHandler<EnterVentEvent>(EnterVentHandler);
        MiraEventManager.RegisterEventHandler<ExitVentEvent>(ExitVentHandler);
    }

    public static void EnterVentHandler(EnterVentEvent @event)
    {
        Logger<ExamplePlugin>.Warning("Entering vent!");
        var random = new System.Random();
        if (random.Next(0, 2) == 0)
        {
            @event.Cancel();
        }
    }

    public static void ExitVentHandler(ExitVentEvent @event)
    {
        Logger<ExamplePlugin>.Warning("Exiting vent!");
        var random = new System.Random();
        if (random.Next(0, 2) == 0)
        {
            @event.Cancel();
        }
    }

    // Example event handler
    public static void FreezeButtonClickHandler(MiraButtonClickEvent<FreezeButton> @event)
    {
        Logger<ExamplePlugin>.Warning("Freeze button clicked!");

        if (PlayerControl.LocalPlayer.Data.PlayerName == "stupid")
        {
            @event.Cancel();
            @event.Button.SetTimer(15f);
        }
    }

    // Example event handler
    public static void FreezeButtonCancelledHandler(MiraButtonCancelledEvent<FreezeButton> @event)
    {
        Logger<ExamplePlugin>.Warning("Freeze button cancelled!");
        @event.Button.OverrideName("Freeze Canceled");
    }
}
