using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu : IClickableMenu
{
    private static readonly string[] Tabs = { "tab.player", "tab.world", "tab.items", "tab.farm", "tab.progress", "tab.friends", "tab.gifts", "tab.recipes", "tab.mods", "tab.fancy" };
    private static readonly string[] Skills = { "skill.farming", "skill.fishing", "skill.mining", "skill.foraging", "skill.combat" };
    private static readonly string[] BaseDebuffs =
    {
        Buff.goblinsCurse, Buff.slimed, Buff.evilEye, Buff.tipsy, Buff.fear, Buff.frozen,
        Buff.spawnMonsters, Buff.nauseous, Buff.darkness, Buff.weakness
    };
    private static readonly Color Background = new(24, 38, 34);
    private static readonly Color Sidebar = new(16, 28, 25);
    private static readonly Color Surface = new(32, 52, 45);
    private static readonly Color Hover = new(50, 77, 65);
    private static readonly Color Border = new(60, 86, 74);
    private static readonly Color Mint = new(177, 237, 193);
    private static readonly Color Gold = new(248, 215, 148);
    private static readonly Color Ink = new(245, 248, 238);
    private static readonly Color Muted = new(187, 207, 196);

    private readonly ModEntry mod;
    private readonly List<(Rectangle Bounds, Action Click)> buttons = new();
    private readonly List<TextBox> visibleInputs = new();
    private readonly TextBox search;
    private readonly TextBox goldAmount;
    private readonly TextBox itemAmount;
    private readonly TextBox experienceAmount;
    private readonly TextBox friendshipAmount;
    private readonly TextBox clockMinutes;
    private readonly TextBox playerSpeed;
    private readonly TextBox fancyStrength;
    private readonly TextBox fancyDuration;
    private ParsedItemData[]? catalog;
    private ParsedItemData[] results = Array.Empty<ParsedItemData>();
    private string? lastQuery;
    private string status = "";
    private int tab;
    private int itemOffset;
    private int qualityIndex;
    private bool expandedItemsOnly;
    private int friendIndex;
    private NPC[]? friends;
    private const int ItemRows = 5;
    private static readonly int[] Qualities = { 0, 1, 2, 4 };
    private static readonly string[] QualityNames = { "quality.normal", "quality.silver", "quality.gold", "quality.iridium" };

    public TrainerMenu(ModEntry mod)
    {
        this.mod = mod;
        search = CreateInput("");
        goldAmount = CreateInput("10000");
        itemAmount = CreateInput("1");
        experienceAmount = CreateInput("1000");
        friendshipAmount = CreateInput("250");
        clockMinutes = CreateInput("60");
        playerSpeed = CreateInput(mod.PlayerSpeedBonus > 0 ? mod.PlayerSpeedBonus.ToString(CultureInfo.InvariantCulture) : "3");
        fancyStrength = CreateInput("5");
        fancyDuration = CreateInput("60");
    }

    private static TextBox CreateInput(string text) =>
        new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black)
        {
            Text = text
        };

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        foreach (TextBox input in visibleInputs)
        {
            if (new Rectangle(input.X, input.Y, input.Width, input.Height).Contains(x, y))
            {
                ClearInputFocus();
                input.Selected = true;
                return;
            }
        }

        ClearInputFocus();
        foreach (var button in buttons)
        {
            if (!button.Bounds.Contains(x, y))
                continue;

            if (playSound)
                Game1.playSound("smallSelect");
            button.Click();
            return;
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape)
        {
            Close();
            return;
        }

        if (!visibleInputs.Any(input => input.Selected))
            base.receiveKeyPress(key);
    }

    public override void receiveScrollWheelAction(int direction)
    {
        if (tab == 2)
            MoveItems(direction > 0 ? -1 : 1);
        if (tab == 6)
            MoveGifts(direction > 0 ? -1 : 1);
    }

    private string T(string key, object? tokens = null) => mod.Translate(key, tokens);

    private void Close()
    {
        ClearInputFocus();
        exitThisMenu();
    }

    private void ClearInputFocus()
    {
        foreach (TextBox input in visibleInputs)
        {
            if (input.Selected)
                input.Selected = false;
        }
    }

    private void DrawInput(SpriteBatch batch, TextBox input, int x, int y, int width)
    {
        input.X = x;
        input.Y = y;
        input.Width = width;
        input.Draw(batch);
        visibleInputs.Add(input);
    }

    private bool TryReadAmount(TextBox input, out int amount)
    {
        if (int.TryParse(input.Text, NumberStyles.None, CultureInfo.InvariantCulture, out amount) && amount > 0)
            return true;

        status = T("amount.invalid");
        return false;
    }

    public override void draw(SpriteBatch batch)
    {
        buttons.Clear();
        visibleInputs.Clear();
        int vw = Game1.uiViewport.Width;
        int vh = Game1.uiViewport.Height;
        int width = Math.Min(1040, vw - 24);
        int height = Math.Min(668, vh - 24);
        int x = (vw - width) / 2;
        int y = (vh - height) / 2;
        int railWidth = Math.Clamp(width / 5, 108, 194);
        int gutter = width >= 720 ? 28 : 14;
        int row = Math.Min(59, Math.Max(28, (height - 164) / 7));
        int left = x + railWidth + gutter;
        int contentWidth = width - railWidth - 2 * gutter;

        Fill(batch, new Rectangle(0, 0, vw, vh), Color.Black * 0.68f, 0.78f);
        Fill(batch, new Rectangle(x + 7, y + 9, width, height), Color.Black * 0.55f, 0.79f);
        Fill(batch, new Rectangle(x, y, width, height), Border, 0.8f);
        Fill(batch, new Rectangle(x + 1, y + 1, width - 2, height - 2), Background, 0.81f);
        Fill(batch, new Rectangle(x + 1, y + 1, railWidth - 1, height - 2), Sidebar, 0.82f);
        Fill(batch, new Rectangle(x + railWidth, y + 1, 1, height - 2), Border, 0.83f);
        Fill(batch, new Rectangle(x + 1, y + 1, railWidth - 1, 3), Mint, 0.84f);

        Text(batch, "Stardew Valley", x + 13, y + 17, Muted, railWidth - 26);
        Text(batch, "Trainer", x + 13, y + 37, Mint, railWidth - 26, true);
        Text(batch, T(Tabs[tab]), left, y + 19, Ink, Math.Max(1, contentWidth - 55), true);
        Fill(batch, new Rectangle(left, y + 65, contentWidth, 1), Border, 0.84f);
        Button(batch, new Rectangle(x + width - gutter - 38, y + 17, 38, 36), "X", Close);

        int navRow = Math.Min(47, Math.Max(25, (height - 103) / Tabs.Length));
        for (int i = 0; i < Tabs.Length; i++)
        {
            int current = i;
            Button(batch, new Rectangle(x + 11, y + 87 + navRow * i, railWidth - 22, navRow - 4),
                T(Tabs[i]), () => { tab = current; status = ""; }, tab == i);
        }

        int top = y + 82;
        switch (tab)
        {
            case 0: DrawPlayer(batch, left, top, contentWidth, row); break;
            case 1: DrawWorld(batch, left, top, contentWidth, row); break;
            case 2: DrawItems(batch, left, top, contentWidth, row); break;
            case 3: DrawFarm(batch, left, top, contentWidth, row); break;
            case 4: DrawProgress(batch, left, top, contentWidth, row); break;
            case 5: DrawFriends(batch, left, top, contentWidth, row); break;
            case 6: DrawGifts(batch, left, top, contentWidth, row); break;
            case 7: DrawRecipes(batch, left, top, contentWidth, row); break;
            case 8: DrawMods(batch, left, top, contentWidth, row); break;
            case 9: DrawFancy(batch, left, top, contentWidth, row); break;
        }

        int footer = y + height - 47;
        Fill(batch, new Rectangle(left, footer, contentWidth, 1), Border, 0.84f);
        Text(batch, status.Length == 0 ? T("menu.close") : status, left, footer + 12, status.Length == 0 ? Muted : Gold, contentWidth);
        drawMouse(batch);
    }

    private void DrawPlayer(SpriteBatch batch, int x, int y, int width, int row)
    {
        Farmer player = Game1.player;
        Text(batch, T("player.stats", new { gold = player.Money.ToString("N0"), health = player.health, maxHealth = player.maxHealth, energy = Math.Ceiling(player.Stamina), maxEnergy = player.MaxStamina }), x, y + 6, Gold, width);
        DrawInput(batch, goldAmount, x, y + row, width / 2 - 5);
        Button(batch, new Rectangle(x + width / 2 + 5, y + row, width - width / 2 - 5, row - 6),
            T("player.gold-add"), GiveGold);
        Pair(batch, x, y + row * 2, width, row, T("player.heal"), () => { player.health = player.maxHealth; status = T("player.healed"); },
            T("player.energy"), () => { player.Stamina = player.MaxStamina; status = T("player.energized"); });
        Pair(batch, x, y + row * 3, width, row, T("player.keep-health", new { state = OnOff(mod.KeepHealthy) }), () => mod.KeepHealthy = !mod.KeepHealthy,
            T("player.keep-energy", new { state = OnOff(mod.KeepEnergized) }), () => mod.KeepEnergized = !mod.KeepEnergized, mod.KeepHealthy, mod.KeepEnergized);
        Pair(batch, x, y + row * 4, width, row, T("player.backpack", new { slots = player.MaxItems }), ExpandBackpack,
            T("player.cleanse"), RemoveDebuffs);
        Text(batch, T("player.speed", new { amount = mod.PlayerSpeedBonus }), x, y + row * 5 + 6, Mint, width);
        int fieldWidth = width / 4;
        DrawInput(batch, playerSpeed, x, y + row * 6, fieldWidth - 5);
        Pair(batch, x + fieldWidth, y + row * 6, width - fieldWidth, row, T("player.speed-apply"), SetPlayerSpeed,
            T("player.speed-off"), () => { mod.SetPlayerSpeed(0); status = T("player.speed-reset"); }, mod.PlayerSpeedBonus > 0);
    }

    private void SetPlayerSpeed()
    {
        if (!TryReadAmount(playerSpeed, out int amount))
            return;
        if (amount > 20)
        {
            status = T("player.speed-limit");
            return;
        }

        mod.SetPlayerSpeed(amount);
        status = T("player.speed-set", new { amount });
    }

    private void RemoveDebuffs()
    {
        var buffs = Game1.player.buffs;
        string[] debuffs = BaseDebuffs.Where(buffs.IsApplied).ToArray();
        foreach (string id in debuffs)
            buffs.Remove(id);

        status = T("player.cleansed", new { count = debuffs.Length });
    }

    private void DrawItems(SpriteBatch batch, int x, int y, int width, int row)
    {
        RefreshItems();
        int searchWidth = width * 2 / 5;
        DrawInput(batch, search, x, y, searchWidth);
        if (search.Text.Length == 0 && !search.Selected)
            Text(batch, T("items.search"), x + 10, y + 8, Surface, searchWidth - 20);

        int optionX = x + searchWidth + 9;
        int optionWidth = (width - searchWidth - 27) / 3;
        int quantityLabelWidth = optionWidth / 3;
        Text(batch, T("items.quantity"), optionX, y + 8, Muted, quantityLabelWidth - 4);
        DrawInput(batch, itemAmount, optionX + quantityLabelWidth, y, optionWidth - quantityLabelWidth);
        Button(batch, new Rectangle(optionX + optionWidth + 6, y, optionWidth, row - 5),
            T(QualityNames[qualityIndex]), () => qualityIndex = (qualityIndex + 1) % Qualities.Length);
        Button(batch, new Rectangle(optionX + 2 * (optionWidth + 6), y, optionWidth, row - 5), T("items.clear"),
            () => search.Text = "");

        int listY = y + Math.Max(row, search.Height) + 5;
        if (results.Length == 0)
            Text(batch, T("items.empty"), x, listY + 6, Muted, width);
        for (int i = 0; i < ItemRows && itemOffset + i < results.Length; i++)
        {
            ParsedItemData item = results[itemOffset + i];
            Rectangle bounds = new(x, listY + row * i, width, row - 5);
            DrawButton(batch, bounds, () => Spawn(item));
            DrawItemName(batch, bounds, item, width * 3 / 5);
            Text(batch, item.QualifiedItemId, x + width * 2 / 3, bounds.Y + 4, Muted, width / 3 - 10);
        }

        int controlsY = listY + ItemRows * row;
        Button(batch, new Rectangle(x, controlsY, 68, row - 5), "<", () => MoveItems(-ItemRows));
        Button(batch, new Rectangle(x + width - 68, controlsY, 68, row - 5), ">", () => MoveItems(ItemRows));
        string range = results.Length == 0 ? T("items.zero") : T("items.range", new { start = itemOffset + 1, end = Math.Min(itemOffset + ItemRows, results.Length), total = results.Length });
        int filterWidth = mod.ExpandedVersion is null ? 0 : Math.Min(width / 3, 190);
        Text(batch, range, x + 82, controlsY + 4, Muted, width - 164 - filterWidth);
        if (filterWidth > 0)
        {
            Button(batch, new Rectangle(x + width - 74 - filterWidth, controlsY, filterWidth, row - 5),
                T(expandedItemsOnly ? "mods.filter.expanded" : "mods.filter.all"),
                () => { expandedItemsOnly = !expandedItemsOnly; lastQuery = null; }, expandedItemsOnly);
        }
    }

    private void DrawFarm(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("farm.heading"), x, y + 6, Mint, width);
        Button(batch, new Rectangle(x, y + row, width, row - 6), T("farm.water"), () => TendCrops(false));
        Button(batch, new Rectangle(x, y + row * 2, width, row - 6), T("farm.grow"), () => TendCrops(true));
        Button(batch, new Rectangle(x, y + row * 3, width, row - 6), T("farm.refill"), RefillWateringCans);
        Pair(batch, x, y + row * 4, width, row, T("farm.repair"), RepairFences,
            T("farm.machines"), FinishMachines);
        Pair(batch, x, y + row * 5, width, row, T("farm.feed"), () => TendAnimals(false),
            T("farm.pet"), () => TendAnimals(true));
        Text(batch, T("farm.animals-note"), x, y + row * 6 + 4, Muted, width);
    }

    private void DrawProgress(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("progress.heading"), x, y + 6, Mint, width);
        int amountX = x + width * 2 / 3;
        Text(batch, T("progress.amount"), x + width / 2, y + 6, Muted, width / 6 - 8);
        DrawInput(batch, experienceAmount, amountX, y, x + width - amountX);
        Farmer player = Game1.player;
        for (int i = 0; i < Skills.Length; i++)
        {
            int skill = i;
            int lineY = y + row * (i + 1);
            Text(batch, T("progress.xp", new { skill = T(Skills[i]), experience = player.experiencePoints[i].ToString("N0") }), x, lineY + 6, Ink, width * 3 / 5);
            Button(batch, new Rectangle(x + width * 2 / 3, lineY, width - width * 2 / 3, row - 6),
                T("progress.gain"), () => GainExperience(skill));
        }
    }

    private void DrawFriends(SpriteBatch batch, int x, int y, int width, int row)
    {
        friends ??= Game1.player.friendshipData.Keys
            .Select(name => Game1.getCharacterFromName(name))
            .OfType<NPC>()
            .Where(npc => npc.IsVillager)
            .OrderBy(npc => npc.displayName)
            .ToArray();
        if (friends.Length == 0)
        {
            Text(batch, T("friends.empty"), x, y + 6, Muted, width);
            return;
        }

        NPC friend = friends[friendIndex];
        int hearts = Game1.player.getFriendshipHeartLevelForNPC(friend.Name);
        Text(batch, T("friends.hearts", new { name = friend.displayName, hearts }), x, y + 6, Gold, width);
        Pair(batch, x, y + row, width, row, "<", () => MoveFriend(-1), ">", () => MoveFriend(1));
        DrawInput(batch, friendshipAmount, x, y + row * 2, width / 2 - 5);
        Button(batch, new Rectangle(x + width / 2 + 5, y + row * 2, width - width / 2 - 5, row - 6), T("friends.gain"), () =>
        {
            if (!TryReadAmount(friendshipAmount, out int amount))
                return;

            var friendship = Game1.player.friendshipData[friend.Name];
            int before = friendship.Points;
            Game1.player.changeFriendship((int)Math.Min(amount, (long)int.MaxValue - before), friend);
            status = T("friends.changed", new { name = friend.displayName, points = (friendship.Points - before).ToString("+#;-#;0") });
        });
    }

    private void MoveFriend(int amount)
    {
        if (friends is { Length: > 0 })
            friendIndex = (friendIndex + amount + friends.Length) % friends.Length;
    }

    private void GiveGold()
    {
        if (TryReadAmount(goldAmount, out int amount))
            status = T("player.gold-given", new { amount = ModEntry.AddGold(amount).ToString("N0") });
    }

    private string OnOff(bool active) => active ? T("state.on") : T("state.off");

    private void ExpandBackpack()
    {
        Farmer player = Game1.player;
        if (player.MaxItems >= 36)
        {
            status = T("player.backpack.full");
            return;
        }

        player.MaxItems = 36;
        status = T("player.backpack.upgraded");
    }

    private void GainExperience(int skill)
    {
        if (!TryReadAmount(experienceAmount, out int amount))
            return;

        int added = Math.Min(amount, 15_000 - Game1.player.experiencePoints[skill]);
        if (added <= 0)
        {
            status = T("progress.cap", new { skill = T(Skills[skill]) });
            return;
        }

        Game1.player.gainExperience(skill, added);
        status = T("progress.added", new { amount = added.ToString("N0"), skill = T(Skills[skill]) });
    }

    private void WarpRailroad()
    {
        if (Game1.year == 1 && Game1.currentSeason == "spring")
            status = T("world.railroad.locked");
        else
            Warp("Railroad", 30, 34);
    }

    private void WarpFarm()
    {
        Farm farm = Game1.getFarm();
        Point entry = farm.GetMainFarmHouseEntry();
        Warp(farm.NameOrUniqueName, entry.X, entry.Y + 1);
    }

    private void Warp(string location, int x, int y)
    {
        GameLocation? destination = Game1.getLocationFromName(location);
        if (destination is null)
        {
            status = T("world.no-destination");
            return;
        }
        if (!IsWalkable(destination, x, y))
        {
            status = T("mods.blocked");
            return;
        }

        if (Game1.currentLocation == destination && Game1.player.TilePoint == new Point(x, y))
        {
            status = T("mods.already-there");
            return;
        }

        GameLocation previous = Game1.currentLocation;
        if (Game1.locations.Contains(previous))
        {
            Point tile = Game1.player.TilePoint;
            mod.PreviousTeleport = new Waypoint { Location = previous.NameOrUniqueName, X = tile.X, Y = tile.Y };
        }
        else
            mod.PreviousTeleport = null;

        Close();
        Game1.warpFarmer(location, x, y, false);
    }

    private void RefreshItems()
    {
        string query = search.Text.Trim();
        if (query == lastQuery)
            return;

        results = GetCatalog()
            .Where(data => (!expandedItemsOnly || IsExpandedItem(data.ItemId)) && MatchesItem(data, query))
            .ToArray();
        lastQuery = query;
        itemOffset = 0;
    }

    private ParsedItemData[] GetCatalog() =>
        catalog ??= ItemRegistry.ItemTypes
            .SelectMany(type => type.GetAllIds().Select(id => type.GetData(id)))
            .Where(data => data != null && !data.IsErrorItem)
            .OrderBy(data => data.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

    private static bool MatchesItem(ParsedItemData data, string query) =>
        data.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
        || (data.InternalName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
        || data.QualifiedItemId.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static void DrawItemName(SpriteBatch batch, Rectangle bounds, ParsedItemData item, int width)
    {
        int icon = Math.Min(34, bounds.Height - 6);
        batch.Draw(item.GetTexture(), new Rectangle(bounds.X + 9, bounds.Y + (bounds.Height - icon) / 2, icon, icon),
            item.GetSourceRect(), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
        Text(batch, item.DisplayName, bounds.X + icon + 18, bounds.Y + 4, Ink, width - icon - 18);
    }

    private void MoveItems(int amount) => itemOffset = Math.Clamp(itemOffset + amount, 0, Math.Max(0, results.Length - ItemRows));

    private void Spawn(ParsedItemData data)
    {
        if (TryReadAmount(itemAmount, out int count))
            Spawn(data, count, Qualities[qualityIndex]);
    }

    private void Spawn(ParsedItemData data, int count, int quality)
    {
        int added = 0;
        while (added < count)
        {
            Item item = ItemRegistry.Create(data.QualifiedItemId, 1, quality);
            int requested = Math.Min(count - added, Math.Max(1, item.maximumStackSize()));
            item.Stack = requested;
            Item? remaining = Game1.player.addItemToInventory(item);
            added += requested - (remaining?.Stack ?? 0);
            if (remaining is not null)
                break;
        }

        status = added == 0 ? T("items.full") : T("items.added", new { count = added, name = data.DisplayName }) + (added < count ? T("items.partial") : "");
    }

    private void TendCrops(bool grow)
    {
        int count = 0;
        foreach (TerrainFeature feature in Game1.currentLocation.terrainFeatures.Values)
        {
            if (feature is not HoeDirt dirt || dirt.crop is not Crop crop || crop.dead.Value)
                continue;

            if (grow)
                crop.growCompletely();
            else
                dirt.state.Value = 1;
            count++;
        }

        status = grow ? T("farm.grown", new { count }) : T("farm.watered", new { count });
    }

    private void Pair(SpriteBatch batch, int x, int y, int width, int row, string first, Action firstClick,
        string second, Action secondClick, bool firstActive = false, bool secondActive = false)
    {
        int half = (width - 10) / 2;
        Button(batch, new Rectangle(x, y, half, row - 6), first, firstClick, firstActive);
        Button(batch, new Rectangle(x + half + 10, y, width - half - 10, row - 6), second, secondClick, secondActive);
    }

    private void Button(SpriteBatch batch, Rectangle bounds, string label, Action click, bool active = false)
    {
        DrawButton(batch, bounds, click, active);
        Text(batch, label, bounds.X + 10, bounds.Y + (bounds.Height - Game1.smallFont.LineSpacing) / 2,
            active ? Mint : Ink, bounds.Width - 20);
    }

    private void DrawButton(SpriteBatch batch, Rectangle bounds, Action click, bool active = false)
    {
        bool hovered = bounds.Contains(Game1.getMouseX(), Game1.getMouseY());
        Fill(batch, bounds, hovered || active ? Hover : Surface);
        if (active)
            Fill(batch, new Rectangle(bounds.X, bounds.Y, 3, bounds.Height), Mint, 0.87f);
        else
            Fill(batch, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), Border, 0.87f);
        buttons.Add((bounds, click));
    }

    private static void Fill(SpriteBatch batch, Rectangle bounds, Color color, float depth = 0.86f) =>
        batch.Draw(Game1.staminaRect, bounds, null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);

    private static void Text(SpriteBatch batch, string text, int x, int y, Color color, int width, bool large = false)
    {
        SpriteFont font = large ? Game1.dialogueFont : Game1.smallFont;
        float scale = Math.Clamp(width / Math.Max(1f, font.MeasureString(text).X), 0f, 1f);
        batch.DrawString(font, text, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9f);
    }
}
