using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewValleyTrainer;

public sealed class ModEntry : Mod
{
    internal const string ExpandedId = "FlashShifter.StardewValleyExpandedCP";
    private const string WaypointsKey = "waypoints";
    internal const int WaypointCount = 3;
    private Waypoint?[]? waypoints;

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
            waypoints = null;
        };
    }

    private static bool CanUseTrainer =>
        Context.IsWorldReady && !Context.IsMultiplayer && Context.IsPlayerFree;

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button != SButton.F5 || !CanUseTrainer)
            return;

        Game1.activeClickableMenu = new TrainerMenu(this);
        Helper.Input.Suppress(e.Button);
    }

    internal string Translate(string key, object? tokens = null) => Helper.Translation.Get(key, tokens).ToString();

    internal string? ExpandedVersion => Helper.ModRegistry.Get(ExpandedId)?.Manifest.Version.ToString();

    internal (string Name, string Version)[] GetDetectedMods() => Helper.ModRegistry.GetAll()
        .Where(info => info.Manifest.UniqueID != ModManifest.UniqueID
            && info.Manifest.UniqueID != "SMAPI.ConsoleCommands"
            && info.Manifest.UniqueID != "SMAPI.SaveBackup")
        .Select(info => (info.Manifest.Name, info.Manifest.Version.ToString()))
        .OrderBy(info => info.Name, StringComparer.CurrentCultureIgnoreCase)
        .ToArray();

    internal Waypoint? GetWaypoint(int slot) => GetWaypoints()[slot];

    internal void SetWaypoint(int slot, Waypoint waypoint)
    {
        Waypoint?[] updated = (Waypoint?[])GetWaypoints().Clone();
        updated[slot] = waypoint;
        Helper.Data.WriteSaveData(WaypointsKey, updated);
        waypoints = updated;
    }

    private Waypoint?[] GetWaypoints()
    {
        if (waypoints is not null)
            return waypoints;

        var saved = Helper.Data.ReadSaveData<Waypoint?[]>(WaypointsKey) ?? Array.Empty<Waypoint?>();
        Array.Resize(ref saved, WaypointCount);
        return waypoints = saved;
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

internal sealed class Waypoint
{
    public string Location { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
}
