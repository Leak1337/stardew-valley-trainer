using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Tools;
using StardewValley.Locations;
using StardewValley.Tools;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private bool showMines;

    private void DrawEquipment(SpriteBatch batch, int x, int y, int width, int row)
    {
        Pair(batch, x, y, width, row, T("equipment.tools"), () => { showMines = false; status = ""; },
            T("equipment.mines"), () => { showMines = true; status = ""; }, !showMines, showMines);

        if (showMines)
        {
            DrawMineControls(batch, x, y + row, width, row);
            return;
        }

        Tool? held = Game1.player.CurrentTool;
        string label = held is null ? T("equipment.no-tool") : T("equipment.current-tool", new
        {
            tool = held.DisplayName,
            level = held.UpgradeLevel
        });
        Text(batch, label, x, y + row + 6, Gold, width);
        Text(batch, T("equipment.tool-tier"), x, y + row * 2 + 6, Mint, width);
        int fieldWidth = width / 4;
        DrawInput(batch, toolLevel, x, y + row * 3, fieldWidth - 5);
        Button(batch, new Rectangle(x + fieldWidth + 5, y + row * 3, width - fieldWidth - 5, row - 6),
            T("equipment.change-tier"), ChangeToolTier);
        Text(batch, T("equipment.tool-note"), x, y + row * 4 + 6, Muted, width);
    }

    private void ChangeToolTier()
    {
        Farmer player = Game1.player;
        Tool? original = player.CurrentTool;
        if (original is not (Axe or Pickaxe or Hoe or WateringCan or Pan)
            || ItemRegistry.GetData(original.QualifiedItemId)?.RawData is not ToolData originalData)
        {
            status = T("equipment.unsupported-tool");
            return;
        }

        if (!int.TryParse(toolLevel.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int level)
            || level > 4 || (original is Pan && level == 0))
        {
            status = T(original is Pan ? "equipment.pan-limit" : "equipment.tier-limit");
            return;
        }
        if (level == original.UpgradeLevel)
        {
            status = T("equipment.same-tier");
            return;
        }

        var definition = GetCatalog().FirstOrDefault(item => item.RawData is ToolData tool
            && tool.ClassName == originalData.ClassName && tool.UpgradeLevel == level
            && !item.ItemId.Contains('.') && !item.ItemId.Contains('_'));
        if (definition is null || ItemRegistry.Create(definition.QualifiedItemId, allowNull: true) is not Tool replacement
            || replacement.GetType() != original.GetType())
        {
            status = T("equipment.unavailable-tier");
            return;
        }

        replacement.UpgradeFrom(original);
        foreach (var pair in original.modData.Pairs)
            replacement.modData[pair.Key] = pair.Value;
        if (replacement is WateringCan newCan && original is WateringCan oldCan)
            newCan.WaterLeft = Math.Min(newCan.waterCanMax, oldCan.WaterLeft);

        player.Items[player.CurrentToolIndex] = replacement;
        status = T("equipment.changed", new { tool = replacement.DisplayName });
    }

    private void DrawMineControls(SpriteBatch batch, int x, int y, int width, int row)
    {
        string location = Game1.currentLocation is MineShaft shaft
            ? T("equipment.mine-location", new { floor = shaft.mineLevel > 120 ? shaft.mineLevel - 120 : shaft.mineLevel })
            : T("equipment.outside-mine");
        Text(batch, location, x, y + 6, Gold, width);
        Text(batch, T("equipment.floor-label"), x, y + row + 6, Mint, width);
        int fieldWidth = width / 4;
        DrawInput(batch, mineFloor, x, y + row * 2, fieldWidth - 5);
        Pair(batch, x + fieldWidth, y + row * 2, width - fieldWidth, row,
            T("equipment.mine-warp"), () => TravelToMine(false),
            T("equipment.skull-warp"), () => TravelToMine(true));
        Pair(batch, x, y + row * 3, width, row, T("equipment.ladder"), CreateMineLadder,
            TeleportLabel("world.travel.mine"), () => Warp("Mine", 18, 13));
        Text(batch, T("equipment.mine-note"), x, y + row * 4 + 6, Muted, width);
    }

    private void TravelToMine(bool skullCavern)
    {
        if (!TryReadAmount(mineFloor, out int floor))
            return;
        if (floor > (skullCavern ? 999 : 120))
        {
            status = T(skullCavern ? "equipment.skull-limit" : "equipment.mine-limit");
            return;
        }
        if (skullCavern && !Game1.player.hasSkullKey)
        {
            status = T("equipment.skull-locked");
            return;
        }

        Close();
        Game1.enterMine(floor + (skullCavern ? 120 : 0));
    }

    private void CreateMineLadder()
    {
        if (Game1.currentLocation is not MineShaft shaft || shaft.mineLevel == 120)
        {
            status = T("equipment.no-ladder");
            return;
        }

        Point player = Game1.player.TilePoint;
        var neighbours = new[] { new Point(0, 1), new Point(1, 0), new Point(-1, 0), new Point(0, -1) };
        foreach (Point offset in neighbours)
        {
            Point tile = player + offset;
            if (!IsWalkable(shaft, tile.X, tile.Y) || !shaft.isTileClearForMineObjects(tile.X, tile.Y)
                || shaft.IsTileOccupiedBy(tile.ToVector2()))
                continue;

            shaft.createLadderDown(tile.X, tile.Y, forceShaft: false);
            status = T("equipment.ladder-created");
            return;
        }
        status = T("equipment.ladder-blocked");
    }
}
