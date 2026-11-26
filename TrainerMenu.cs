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

internal sealed class TrainerMenu : IClickableMenu
{
    private static readonly string[] Tabs = { "Spieler", "Welt", "Items", "Farm", "Fortschritt" };
    private static readonly string[] Skills = { "Landwirtschaft", "Angeln", "Bergbau", "Sammeln", "Kampf" };
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
    private const int ItemRows = 5;
    private static readonly int[] Qualities = { 0, 1, 2, 4 };
    private static readonly string[] QualityNames = { "Normal", "Silber", "Gold", "Iridium" };

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
        if (key == Keys.Escape || key == Keys.F5)
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
    }

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
                Tabs[i], () => { tab = current; status = ""; }, tab == i);
        }

        int top = y + 115;
        switch (tab)
        {
            case 0: DrawPlayer(batch, left, top, contentWidth, row); break;
            case 1: DrawWorld(batch, left, top, contentWidth, row); break;
            case 2: DrawItems(batch, left, top, contentWidth, row); break;
            case 3: DrawFarm(batch, left, top, contentWidth, row); break;
            case 4: DrawProgress(batch, left, top, contentWidth, row); break;
        }

        int footer = y + height - 43;
        Fill(batch, new Rectangle(left, footer, contentWidth, 1), Border);
        Text(batch, status.Length == 0 ? "F5 / Esc  Schließen" : status, left, footer + 9, status.Length == 0 ? Muted : Gold, contentWidth);
        drawMouse(batch);
    }

    private void DrawPlayer(SpriteBatch batch, int x, int y, int width, int row)
    {
        Farmer player = Game1.player;
        Text(batch, $"Gold  {player.Money:N0}     LP  {player.health}/{player.maxHealth}     Energie  {Math.Ceiling(player.Stamina)}/{player.MaxStamina}", x, y + 6, Gold, width);
        Pair(batch, x, y + row, width, row, "+10.000 Gold", () => GiveGold(10_000), "+100.000 Gold", () => GiveGold(100_000));
        Pair(batch, x, y + row * 2, width, row, "Gesundheit auffüllen", () => { player.health = player.maxHealth; status = "Gesundheit aufgefüllt."; },
            "Ausdauer auffüllen", () => { player.Stamina = player.MaxStamina; status = "Ausdauer aufgefüllt."; });
        Pair(batch, x, y + row * 3, width, row, $"Gesundheit halten · {OnOff(mod.KeepHealthy)}", () => mod.KeepHealthy = !mod.KeepHealthy,
            $"Ausdauer halten · {OnOff(mod.KeepEnergized)}", () => mod.KeepEnergized = !mod.KeepEnergized, mod.KeepHealthy, mod.KeepEnergized);
        Button(batch, new Rectangle(x, y + row * 4, width, row - 6), $"Rucksack erweitern · {player.MaxItems} Plätze", ExpandBackpack);
        Text(batch, "F6  Gold     F7  Gesundheit     F8  Ausdauer", x, y + row * 5 + 8, Muted, width);
    }

    private void DrawWorld(SpriteBatch batch, int x, int y, int width, int row)
    {
        int time = Game1.timeOfDay;
        Text(batch, $"Tag {Game1.dayOfMonth}  ·  Jahr {Game1.year}  ·  {time / 100:00}:{time % 100:00}", x, y + 6, Gold, width);
        Button(batch, new Rectangle(x, y + row, width, row - 6), $"Spielzeit einfrieren · {OnOff(mod.FreezeTime)}  [F9]",
            () => mod.FreezeTime = !mod.FreezeTime, mod.FreezeTime);
        Text(batch, "Schnellreise", x, y + row * 2 + 6, Mint, width);
        Pair(batch, x, y + row * 3, width, row, "Bauernhof", WarpFarm,
            "Pelikan-Stadt", () => Warp("Town", 35, 57));
        Pair(batch, x, y + row * 4, width, row, "Strand", () => Warp("Beach", 20, 4),
            "Minen-Eingang", () => Warp("Mine", 18, 13));
        Pair(batch, x, y + row * 5, width, row, "Bahnhof*", () => WarpRailroad(),
            "Bus-Haltestelle", () => Warp("BusStop", 14, 23));
        Text(batch, "* Ab Sommer des ersten Jahres", x, y + row * 6 + 4, Muted, width);
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
            Text(batch, "Name oder Item-ID suchen", x + 10, y + 8, Surface, searchWidth - 20);

        int optionX = x + searchWidth + 9;
        int optionWidth = (width - searchWidth - 27) / 3;
        Button(batch, new Rectangle(optionX, y, optionWidth, row - 5), $"Menge {quantity}", () =>
            quantity = quantity switch { 1 => 10, 10 => 50, 50 => 100, 100 => 999, _ => 1 });
        Button(batch, new Rectangle(optionX + optionWidth + 6, y, optionWidth, row - 5),
            QualityNames[qualityIndex], () => qualityIndex = (qualityIndex + 1) % Qualities.Length);
        Button(batch, new Rectangle(optionX + 2 * (optionWidth + 6), y, optionWidth, row - 5), "Suche leeren",
            () => search.Text = "");

        int listY = y + Math.Max(row, search.Height) + 5;
        if (results.Length == 0)
            Text(batch, "Keine passenden Items.", x, listY + 6, Muted, width);
        for (int i = 0; i < ItemRows && itemOffset + i < results.Length; i++)
        {
            ParsedItemData item = results[itemOffset + i];
            Rectangle bounds = new(x, listY + row * i, width, row - 5);
            DrawButton(batch, bounds, () => Spawn(item));
            int icon = Math.Min(34, bounds.Height - 6);
            batch.Draw(item.GetTexture(), new Rectangle(x + 9, bounds.Y + (bounds.Height - icon) / 2, icon, icon),
                item.GetSourceRect(), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
            int textX = x + icon + 18;
            Text(batch, item.DisplayName, textX, bounds.Y + 4, Ink, width * 3 / 5 - icon);
            Text(batch, item.QualifiedItemId, x + width * 2 / 3, bounds.Y + 4, Muted, width / 3 - 10);
        }

        int controlsY = listY + ItemRows * row;
        Button(batch, new Rectangle(x, controlsY, 68, row - 5), "<", () => MoveItems(-ItemRows));
        Button(batch, new Rectangle(x + width - 68, controlsY, 68, row - 5), ">", () => MoveItems(ItemRows));
        string range = results.Length == 0 ? "0 Treffer" : $"{itemOffset + 1}–{Math.Min(itemOffset + ItemRows, results.Length)} von {results.Length}";
        Text(batch, range, x + 82, controlsY + 4, Muted, width - 164);
    }

    private void DrawFarm(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, "Felder am aktuellen Ort", x, y + 6, Mint, width);
        Button(batch, new Rectangle(x, y + row, width, row - 6), "Gepflanzte Felder gießen", () => TendCrops(false));
        Button(batch, new Rectangle(x, y + row * 2, width, row - 6), "Lebende Kulturen erntereif machen", () => TendCrops(true));
        Text(batch, "Wirkt auf bepflanzten Boden, auch im Gewächshaus.", x, y + row * 3 + 8, Muted, width);
    }

    private void DrawProgress(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, "Fähigkeiten", x, y + 6, Mint, width);
        Farmer player = Game1.player;
        for (int i = 0; i < Skills.Length; i++)
        {
            int skill = i;
            int lineY = y + row * (i + 1);
            Text(batch, $"{Skills[i]}  ·  {player.experiencePoints[i]:N0} EP", x, lineY + 6, Ink, width * 3 / 5);
            Button(batch, new Rectangle(x + width * 2 / 3, lineY, width - width * 2 / 3, row - 6),
                "+1.000 EP", () => GainExperience(skill));
        }
    }

    private void GiveGold(int amount) => status = $"+{ModEntry.AddGold(amount):N0} Gold";

    private static string OnOff(bool active) => active ? "An" : "Aus";

    private void ExpandBackpack()
    {
        Farmer player = Game1.player;
        if (player.MaxItems >= 36)
        {
            status = "Der Rucksack ist bereits vollständig erweitert.";
            return;
        }

        player.MaxItems = 36;
        status = "Der Rucksack hat jetzt 36 Plätze.";
    }

    private void GainExperience(int skill)
    {
        int added = Math.Min(1_000, 15_000 - Game1.player.experiencePoints[skill]);
        if (added <= 0)
        {
            status = $"{Skills[skill]} hat die Grundspiel-EP-Grenze erreicht.";
            return;
        }

        Game1.player.gainExperience(skill, added);
        status = $"+{added:N0} EP für {Skills[skill]}.";
    }

    private void WarpRailroad()
    {
        if (Game1.year == 1 && Game1.currentSeason == "spring")
            status = "Der Bahnhof wird im Sommer freigeschaltet.";
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
            status = "Das Ziel ist in diesem Spielstand nicht verfügbar.";
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

        catalog ??= ItemRegistry.ItemTypes
            .SelectMany(type => type.GetAllIds().Select(id => type.GetData(id)))
            .Where(data => data != null && !data.IsErrorItem)
            .OrderBy(data => data.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        results = catalog.Where(data => data.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || (data.InternalName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || data.QualifiedItemId.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
        lastQuery = query;
        itemOffset = 0;
    }

    private void MoveItems(int amount) => itemOffset = Math.Clamp(itemOffset + amount, 0, Math.Max(0, results.Length - ItemRows));

    private void Spawn(ParsedItemData data)
    {
        Item item = ItemRegistry.Create(data.QualifiedItemId, 1, Qualities[qualityIndex]);
        item.Stack = Math.Min(quantity, item.maximumStackSize());
        int requested = item.Stack;
        Item? remaining = Game1.player.addItemToInventory(item);
        int added = requested - (remaining?.Stack ?? 0);
        status = added == 0 ? "Inventar voll." : $"+{added} × {data.DisplayName}" + (added < requested ? " · Inventar voll" : "");
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

        status = grow ? $"{count} Kulturen bearbeitet." : $"{count} Felder gegossen.";
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
