using System;
using System.Collections.Generic;
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
    private static readonly string[] Tabs = { "tab.player", "tab.world", "tab.items", "tab.farm", "tab.progress", "tab.friends", "tab.gifts", "tab.recipes" };
    private static readonly string[] Skills = { "skill.farming", "skill.fishing", "skill.mining", "skill.foraging", "skill.combat" };
    private static readonly Color Background = new(17, 29, 26);
    private static readonly Color Surface = new(25, 43, 37);
    private static readonly Color Hover = new(41, 65, 54);
    private static readonly Color Border = new(55, 78, 65);
    private static readonly Color Mint = new(161, 226, 181);
    private static readonly Color Gold = new(246, 207, 126);
    private static readonly Color Ink = new(243, 247, 236);
    private static readonly Color Muted = new(177, 197, 185);

    private readonly ModEntry mod;
    private readonly List<(Rectangle Bounds, Action Click)> buttons = new();
    private readonly TextBox search;
    private ParsedItemData[]? catalog;
    private ParsedItemData[] results = Array.Empty<ParsedItemData>();
    private string? lastQuery;
    private string status = "";
    private int tab;
    private int itemOffset;
    private int quantity = 1;
    private int qualityIndex;
    private int friendIndex;
    private NPC[]? friends;
    private const int ItemRows = 5;
    private static readonly int[] Qualities = { 0, 1, 2, 4 };
    private static readonly string[] QualityNames = { "quality.normal", "quality.silver", "quality.gold", "quality.iridium" };

    public TrainerMenu(ModEntry mod)
    {
        this.mod = mod;
        search = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (tab == 2 && new Rectangle(search.X, search.Y, search.Width, search.Height).Contains(x, y))
        {
            search.Selected = true;
            return;
        }

        search.Selected = false;
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

        if (!search.Selected)
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
        search.Selected = false;
        exitThisMenu();
    }

    public override void draw(SpriteBatch batch)
    {
        buttons.Clear();
        int vw = Game1.uiViewport.Width;
        int vh = Game1.uiViewport.Height;
        int width = Math.Min(940, vw - 24);
        int height = Math.Min(638, vh - 24);
        int x = (vw - width) / 2;
        int y = (vh - height) / 2;
        int row = Math.Min(55, Math.Max(30, (height - 208) / 7));
        int left = x + 22;
        int contentWidth = width - 44;

        Fill(batch, new Rectangle(0, 0, vw, vh), Color.Black * 0.62f);
        Fill(batch, new Rectangle(x + 6, y + 8, width, height), Color.Black * 0.5f);
        Fill(batch, new Rectangle(x, y, width, height), Border);
        Fill(batch, new Rectangle(x + 1, y + 1, width - 2, height - 2), Background);
        Fill(batch, new Rectangle(x + 1, y + 1, width - 2, 4), Mint);
        Text(batch, "Stardew Valley Trainer", left, y + 14, Ink, contentWidth - 68, true);
        Button(batch, new Rectangle(x + width - 58, y + 13, 38, 36), "X", Close);

        int tabWidth = contentWidth / Tabs.Length;
        for (int i = 0; i < Tabs.Length; i++)
        {
            int current = i;
            Button(batch, new Rectangle(left + i * tabWidth, y + 64, tabWidth - 5, 38),
                T(Tabs[i]), () => { tab = current; status = ""; }, tab == i);
        }

        int top = y + 115;
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
        }

        int footer = y + height - 43;
        Fill(batch, new Rectangle(left, footer, contentWidth, 1), Border);
        Text(batch, status.Length == 0 ? T("menu.close") : status, left, footer + 9, status.Length == 0 ? Muted : Gold, contentWidth);
        drawMouse(batch);
    }

    private void DrawPlayer(SpriteBatch batch, int x, int y, int width, int row)
    {
        Farmer player = Game1.player;
        Text(batch, T("player.stats", new { gold = player.Money.ToString("N0"), health = player.health, maxHealth = player.maxHealth, energy = Math.Ceiling(player.Stamina), maxEnergy = player.MaxStamina }), x, y + 6, Gold, width);
        Pair(batch, x, y + row, width, row, T("player.gold-small"), () => GiveGold(10_000), T("player.gold-large"), () => GiveGold(100_000));
        Pair(batch, x, y + row * 2, width, row, T("player.heal"), () => { player.health = player.maxHealth; status = T("player.healed"); },
            T("player.energy"), () => { player.Stamina = player.MaxStamina; status = T("player.energized"); });
        Pair(batch, x, y + row * 3, width, row, T("player.keep-health", new { state = OnOff(mod.KeepHealthy) }), () => mod.KeepHealthy = !mod.KeepHealthy,
            T("player.keep-energy", new { state = OnOff(mod.KeepEnergized) }), () => mod.KeepEnergized = !mod.KeepEnergized, mod.KeepHealthy, mod.KeepEnergized);
        Button(batch, new Rectangle(x, y + row * 4, width, row - 6), T("player.backpack", new { slots = player.MaxItems }), ExpandBackpack);
    }

    private void DrawWorld(SpriteBatch batch, int x, int y, int width, int row)
    {
        int time = Game1.timeOfDay;
        Text(batch, T("world.clock", new { day = Game1.dayOfMonth, year = Game1.year, time = $"{time / 100:00}:{time % 100:00}" }), x, y + 6, Gold, width);
        Button(batch, new Rectangle(x, y + row, width, row - 6), T("world.freeze", new { state = OnOff(mod.FreezeTime) }),
            () => mod.FreezeTime = !mod.FreezeTime, mod.FreezeTime);
        Pair(batch, x, y + row * 2, width, row, T("world.sun"), () => SetWeather("Sun", T("world.sun")),
            T("world.rain"), () => SetWeather("Rain", T("world.rain")));
        Pair(batch, x, y + row * 3, width, row, T("world.travel.farm"), WarpFarm,
            T("world.travel.town"), () => Warp("Town", 35, 57));
        Pair(batch, x, y + row * 4, width, row, T("world.travel.beach"), () => Warp("Beach", 20, 4),
            T("world.travel.mine"), () => Warp("Mine", 18, 13));
        Pair(batch, x, y + row * 5, width, row, T("world.travel.railroad"), () => WarpRailroad(),
            T("world.travel.bus"), () => Warp("BusStop", 14, 23));
        Text(batch, T("world.railroad.note"), x, y + row * 6 + 4, Muted, width);
    }

    private void SetWeather(string weather, string message)
    {
        Game1.weatherForTomorrow = weather;
        Game1.netWorldState.Value.GetWeatherForLocation("Default").WeatherForTomorrow = weather;
        status = message;
    }

    private void DrawItems(SpriteBatch batch, int x, int y, int width, int row)
    {
        RefreshItems();
        int searchWidth = width * 2 / 5;
        search.X = x;
        search.Y = y;
        search.Width = searchWidth;
        search.Draw(batch);
        if (search.Text.Length == 0 && !search.Selected)
            Text(batch, T("items.search"), x + 10, y + 8, Surface, searchWidth - 20);

        int optionX = x + searchWidth + 9;
        int optionWidth = (width - searchWidth - 27) / 3;
        Button(batch, new Rectangle(optionX, y, optionWidth, row - 5), T("items.quantity", new { count = quantity }), () =>
            quantity = quantity switch { 1 => 10, 10 => 50, 50 => 100, 100 => 999, _ => 1 });
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
        Text(batch, range, x + 82, controlsY + 4, Muted, width - 164);
    }

    private void DrawFarm(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("farm.heading"), x, y + 6, Mint, width);
        Button(batch, new Rectangle(x, y + row, width, row - 6), T("farm.water"), () => TendCrops(false));
        Button(batch, new Rectangle(x, y + row * 2, width, row - 6), T("farm.grow"), () => TendCrops(true));
        Text(batch, T("farm.note"), x, y + row * 3 + 8, Muted, width);
        Button(batch, new Rectangle(x, y + row * 4, width, row - 6), T("farm.refill"), RefillWateringCans);
        Pair(batch, x, y + row * 5, width, row, T("farm.feed"), () => TendAnimals(false),
            T("farm.pet"), () => TendAnimals(true));
        Text(batch, T("farm.animals-note"), x, y + row * 6 + 4, Muted, width);
    }

    private void DrawProgress(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("progress.heading"), x, y + 6, Mint, width);
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
        Button(batch, new Rectangle(x, y + row * 2, width, row - 6), T("friends.gain"), () =>
        {
            var friendship = Game1.player.friendshipData[friend.Name];
            int before = friendship.Points;
            Game1.player.changeFriendship(250, friend);
            status = T("friends.changed", new { name = friend.displayName, points = (friendship.Points - before).ToString("+#;-#;0") });
        });
    }

    private void MoveFriend(int amount)
    {
        if (friends is { Length: > 0 })
            friendIndex = (friendIndex + amount + friends.Length) % friends.Length;
    }

    private void GiveGold(int amount) => status = T("player.gold-given", new { amount = ModEntry.AddGold(amount).ToString("N0") });

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
        int added = Math.Min(1_000, 15_000 - Game1.player.experiencePoints[skill]);
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
        if (Game1.getLocationFromName(location) == null)
        {
            status = T("world.no-destination");
            return;
        }

        Close();
        Game1.warpFarmer(location, x, y, false);
    }

    private void RefreshItems()
    {
        string query = search.Text.Trim();
        if (query == lastQuery)
            return;

        results = GetCatalog().Where(data => MatchesItem(data, query)).ToArray();
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

    private void Spawn(ParsedItemData data) => Spawn(data, quantity, Qualities[qualityIndex]);

    private void Spawn(ParsedItemData data, int count, int quality)
    {
        Item item = ItemRegistry.Create(data.QualifiedItemId, 1, quality);
        item.Stack = Math.Min(count, item.maximumStackSize());
        int requested = item.Stack;
        Item? remaining = Game1.player.addItemToInventory(item);
        int added = requested - (remaining?.Stack ?? 0);
        status = added == 0 ? T("items.full") : T("items.added", new { count = added, name = data.DisplayName }) + (added < requested ? T("items.partial") : "");
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
        Fill(batch, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), active ? Mint : Border);
        buttons.Add((bounds, click));
    }

    private static void Fill(SpriteBatch batch, Rectangle bounds, Color color) =>
        batch.Draw(Game1.staminaRect, bounds, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0.86f);

    private static void Text(SpriteBatch batch, string text, int x, int y, Color color, int width, bool large = false)
    {
        SpriteFont font = large ? Game1.dialogueFont : Game1.smallFont;
        float scale = Math.Min(1f, width / Math.Max(1f, font.MeasureString(text).X));
        batch.DrawString(font, text, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9f);
    }
}
