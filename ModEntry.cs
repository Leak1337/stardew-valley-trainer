using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;

namespace StardewValleyTrainer;

public sealed class ModEntry : Mod
{
    internal const string ExpandedId = "FlashShifter.StardewValleyExpandedCP";
    internal const string RidgesideId = "Rafseazz.RSVCP";
    private const string AutomateId = "Pathoschild.Automate";
    private const string WaypointsKey = "waypoints";
    internal const int WaypointCount = 3;
    private Waypoint?[]? waypoints;
    private TrainerConfig config = new();
    private ITrainerAutomateApi? automate;

    internal bool FreezeTime { get; set; }
    internal bool KeepHealthy { get; set; }
    internal bool KeepEnergized { get; set; }
    internal int PlayerSpeedBonus { get; private set; }
    internal Waypoint? PreviousTeleport { get; set; }
    private const string SpeedBuffId = "Leak1337.StardewValleyTrainer.Speed";

    public override void Entry(IModHelper helper)
    {
        config = helper.ReadConfig<TrainerConfig>();
        helper.Events.GameLoop.GameLaunched += (_, _) => automate = helper.ModRegistry.GetApi<ITrainerAutomateApi>(AutomateId);
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.GameLoop.DayStarted += (_, _) =>
        {
            FreezeTime = false;
            PreviousTeleport = null;
            if (PlayerSpeedBonus > 0 && !Context.IsMultiplayer)
                SetPlayerSpeed(PlayerSpeedBonus);
        };
        helper.Events.GameLoop.ReturnedToTitle += (_, _) =>
        {
            FreezeTime = KeepHealthy = KeepEnergized = false;
            waypoints = null;
            PreviousTeleport = null;
            PlayerSpeedBonus = 0;
        };
    }

    private static bool CanUseTrainer =>
        Context.IsWorldReady && !Context.IsMultiplayer && Context.IsPlayerFree;

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button != config.MenuKey || !CanUseTrainer || Helper.Input.IsSuppressed(e.Button))
            return;

        Game1.activeClickableMenu = new TrainerMenu(this);
        Helper.Input.Suppress(e.Button);
    }

    internal string Translate(string key, object? tokens = null) => Helper.Translation.Get(key, tokens).ToString();

    internal string? ExpandedVersion => Helper.ModRegistry.Get(ExpandedId)?.Manifest.Version.ToString();
    internal string? RidgesideVersion => Helper.ModRegistry.Get(RidgesideId)?.Manifest.Version.ToString();
    internal bool HasAutomate => Helper.ModRegistry.IsLoaded(AutomateId);

    internal bool TryGetAutomatedTiles(GameLocation location, out IDictionary<Vector2, int>? tiles)
    {
        tiles = null;
        if (!HasAutomate)
            return true;
        if (automate is null || location.Map.Layers.Count == 0)
            return false;

        var layer = location.Map.Layers[0];
        try
        {
            tiles = automate.GetMachineStates(location, new Rectangle(0, 0, layer.LayerWidth, layer.LayerHeight));
            return tiles is not null;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Could not read Automate's machine groups: {ex.Message}", LogLevel.Warn);
            return false;
        }
    }

    internal void SetPlayerSpeed(int amount)
    {
        Game1.player.buffs.Remove(SpeedBuffId);
        PlayerSpeedBonus = amount;
        if (amount > 0)
        {
            Game1.player.applyBuff(new Buff(SpeedBuffId, duration: Buff.ENDLESS, effects: new BuffEffects
            {
                Speed = { Value = amount }
            }, displayName: Translate("player.speed-buff")));
        }
    }

    internal (string Name, string Version, string? Support)[] GetDetectedMods() => Helper.ModRegistry.GetAll()
        .Where(info => info.Manifest.UniqueID != ModManifest.UniqueID
            && info.Manifest.UniqueID != "SMAPI.ConsoleCommands"
            && info.Manifest.UniqueID != "SMAPI.SaveBackup")
        .Select(info => (info.Manifest.Name, info.Manifest.Version.ToString(), GetSupportLabel(info.Manifest.UniqueID, info.Manifest.UpdateKeys)))
        .OrderBy(info => info.Name, StringComparer.CurrentCultureIgnoreCase)
        .ToArray();

    private static string? GetSupportLabel(string id, string[]? updateKeys)
    {
        if (string.Equals(id, ExpandedId, StringComparison.OrdinalIgnoreCase) || string.Equals(id, RidgesideId, StringComparison.OrdinalIgnoreCase))
            return "mods.support-expansion";
        if (string.Equals(id, AutomateId, StringComparison.OrdinalIgnoreCase))
            return "mods.support-automation";

        bool recognised = id.ToLowerInvariant() is "annosz.uiinfosuite2" or "bouhm.npcmaplocations"
            or "pathoschild.lookupanything" or "pathoschild.chestsanywhere"
            || (updateKeys?.Any(key => key.Split('@')[0].ToLowerInvariant()
                is "nexus:239" or "nexus:541" or "nexus:518" or "nexus:5255" or "nexus:5450" or "nexus:8897") ?? false);
        return recognised ? "mods.support-coexist" : null;
    }

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

internal sealed class TrainerConfig
{
    public SButton MenuKey { get; set; } = SButton.F5;
}

public interface ITrainerAutomateApi
{
    IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea);
}

internal sealed class Waypoint
{
    public string Location { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
}
