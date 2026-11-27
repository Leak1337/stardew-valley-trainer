using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private const string ExpandedItemPrefix = "FlashShifter.StardewValleyExpanded";
    private const int ModRows = 5;
    private (string Name, string Version)[]? detectedMods;
    private bool showModList;
    private int modListOffset;

    private static bool IsExpandedItem(string itemId) =>
        itemId.StartsWith(ExpandedItemPrefix, StringComparison.OrdinalIgnoreCase);

    private void DrawMods(SpriteBatch batch, int x, int y, int width, int row)
    {
        detectedMods ??= mod.GetDetectedMods();
        if (showModList)
        {
            DrawModList(batch, x, y, width, row);
            return;
        }

        string? expanded = mod.ExpandedVersion;
        string compatibility = expanded is null
            ? T(detectedMods.Length == 0 ? "mods.standard" : "mods.generic")
            : T("mods.expanded", new { version = expanded });
        Text(batch, compatibility, x, y + 6, Gold, width);
        int listButtonWidth = expanded is null ? Math.Min(width / 2, 260) : width / 3;
        Button(batch, new Rectangle(x, y + row, listButtonWidth, row - 6),
            T("mods.detected", new { count = detectedMods.Length }), () => showModList = true);
        if (expanded is not null)
        {
            Button(batch, new Rectangle(x + listButtonWidth, y + row, listButtonWidth - 8, row - 6),
                T("mods.browse-expanded"), OpenExpandedItems);
            Button(batch, new Rectangle(x + listButtonWidth * 2, y + row, width - listButtonWidth * 2, row - 6),
                T("mods.browse-expanded-residents"), OpenExpandedResidents);
        }
        Text(batch, T("mods.waypoints"), x, y + row * 2 + 5, Mint, width);

        for (int i = 0; i < ModEntry.WaypointCount; i++)
        {
            int slot = i;
            int line = y + row * (3 + i);
            int actionWidth = Math.Min(width / 4, 180);
            Waypoint? saved = mod.GetWaypoint(slot);
            GameLocation? location = saved is null ? null : Game1.getLocationFromName(saved.Location);
            string label = saved is null ? T("mods.empty-slot") : location?.DisplayName ?? T("mods.missing-location");
            Text(batch, $"{slot + 1}. {label}", x, line + 5, Ink, width - 2 * actionWidth - 16);
            Button(batch, new Rectangle(x + width - 2 * actionWidth - 6, line, actionWidth, row - 6),
                T(saved is null ? "mods.mark" : "mods.replace"), () => MarkWaypoint(slot));
            if (saved is not null)
            {
                Button(batch, new Rectangle(x + width - actionWidth, line, actionWidth, row - 6),
                    T("mods.travel"), () => TravelToWaypoint(saved));
            }
        }
        Text(batch, T("mods.waypoints-note"), x, y + row * 6 + 5, Muted, width);
    }

    private void DrawModList(SpriteBatch batch, int x, int y, int width, int row)
    {
        var mods = detectedMods!;
        Button(batch, new Rectangle(x, y, Math.Min(width / 2, 230), row - 6),
            T("mods.back"), () => showModList = false);
        if (mods.Length == 0)
            Text(batch, T("mods.none"), x, y + row + 5, Muted, width);
        for (int i = 0; i < ModRows && i + modListOffset < mods.Length; i++)
        {
            var detected = mods[i + modListOffset];
            Text(batch, $"{detected.Name}  ·  {detected.Version}", x, y + row * (i + 1) + 5, Ink, width);
        }

        Pair(batch, x, y + row * 6, width, row, "<",
            () => modListOffset = Math.Max(0, modListOffset - ModRows), ">",
            () => modListOffset = Math.Min(Math.Max(0, mods.Length - ModRows), modListOffset + ModRows));
    }

    private void OpenExpandedItems()
    {
        expandedItemsOnly = true;
        search.Text = "";
        lastQuery = null;
        status = "";
        tab = 2;
    }

    private void MarkWaypoint(int slot)
    {
        GameLocation location = Game1.currentLocation;
        Point tile = Game1.player.TilePoint;
        if (!Game1.locations.Contains(location) || !IsWalkable(location, tile.X, tile.Y))
        {
            status = T("mods.unmarkable");
            return;
        }

        mod.SetWaypoint(slot, new Waypoint { Location = location.NameOrUniqueName, X = tile.X, Y = tile.Y });
        status = T("mods.marked", new { location = location.DisplayName });
    }

    private void TravelToWaypoint(Waypoint saved)
    {
        GameLocation? location = Game1.getLocationFromName(saved.Location);
        if (location is null || !Game1.locations.Contains(location))
        {
            status = T("mods.missing-location");
            return;
        }

        if (Game1.currentLocation == location && Game1.player.TilePoint == new Point(saved.X, saved.Y))
        {
            status = T("mods.already-there");
            return;
        }

        if (!IsWalkable(location, saved.X, saved.Y) || location.IsTileOccupiedBy(new Vector2(saved.X, saved.Y)))
        {
            status = T("mods.blocked");
            return;
        }

        Warp(saved.Location, saved.X, saved.Y);
    }

    private static bool IsWalkable(GameLocation location, int x, int y) =>
        location.isTileOnMap(new Vector2(x, y))
        && location.isTilePassable(new xTile.Dimensions.Location(x, y), Game1.viewport);
}
