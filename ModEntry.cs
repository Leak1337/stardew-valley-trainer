using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewValleyTrainer;

public sealed class ModEntry : Mod
{
    internal bool FreezeTime { get; set; }
    internal bool KeepHealthy { get; set; }
    internal bool KeepEnergized { get; set; }

    public override void Entry(IModHelper helper)
    {
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.GameLoop.DayStarted += (_, _) => FreezeTime = false;
        helper.Events.GameLoop.ReturnedToTitle += (_, _) =>
        {
            FreezeTime = KeepHealthy = KeepEnergized = false;
        };
    }

    private static bool CanUseTrainer =>
        Context.IsWorldReady && !Context.IsMultiplayer && Context.IsPlayerFree;

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!CanUseTrainer)
            return;

        Farmer player = Game1.player;
        switch (e.Button)
        {
            case SButton.F5:
                Game1.activeClickableMenu = new TrainerMenu(this);
                break;
            case SButton.F6:
                int added = AddGold(10_000);
                Game1.showGlobalMessage(added > 0 ? $"+{added:N0} Gold" : "Goldlimit erreicht");
                break;
            case SButton.F7:
                player.health = player.maxHealth;
                Game1.showGlobalMessage("Gesundheit aufgefuellt");
                break;
            case SButton.F8:
                player.Stamina = player.MaxStamina;
                Game1.showGlobalMessage("Ausdauer aufgefuellt");
                break;
            case SButton.F9:
                FreezeTime = !FreezeTime;
                Game1.showGlobalMessage(FreezeTime ? "Spielzeit eingefroren" : "Spielzeit läuft weiter");
                break;
            default:
                return;
        }

        Helper.Input.Suppress(e.Button);
    }

    internal static int AddGold(int amount)
    {
        int added = (int)Math.Min(amount, (long)int.MaxValue - Game1.player.Money);
        Game1.player.Money += added;
        return added;
    }

    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (FreezeTime && CanUseTrainer)
            Game1.gameTimeInterval = 0;
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!CanUseTrainer)
            return;

        Farmer player = Game1.player;
        if (KeepHealthy && player.health > 0)
            player.health = player.maxHealth;
        if (KeepEnergized && player.Stamina >= 0)
            player.Stamina = player.MaxStamina;
    }
}
