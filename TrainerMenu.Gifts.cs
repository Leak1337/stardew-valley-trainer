using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private const int GiftRows = 3;
    private static readonly (int Taste, string Key)[] GiftFilters =
    {
        (NPC.gift_taste_love, "gifts.loved"),
        (NPC.gift_taste_like, "gifts.liked"),
        (NPC.gift_taste_neutral, "gifts.neutral"),
        (NPC.gift_taste_dislike, "gifts.disliked"),
        (NPC.gift_taste_hate, "gifts.hated")
    };
    private NPC[]? residents;
    private int residentIndex;
    private int giftFilter;
    private int giftOffset;
    private (ParsedItemData Data, Item Item)[]? giftableItems;
    private readonly Dictionary<string, Dictionary<int, ParsedItemData[]>> giftTastes = new();
    private ParsedItemData[] visibleGifts = Array.Empty<ParsedItemData>();

    private void DrawGifts(SpriteBatch batch, int x, int y, int width, int row)
    {
        residents ??= Utility.getAllVillagers()
            .Where(npc => npc.CanReceiveGifts())
            .OrderBy(npc => npc.displayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        if (residents.Length == 0)
        {
            Text(batch, T("gifts.no-residents"), x, y + 6, Muted, width);
            return;
        }

        NPC resident = residents[residentIndex];
        Button(batch, new Rectangle(x, y, 62, row - 6), "<", () => MoveResident(-1));
        Button(batch, new Rectangle(x + width - 62, y, 62, row - 6), ">", () => MoveResident(1));
        string name = resident.currentLocation == null
            ? resident.displayName
            : T("gifts.location", new { name = resident.displayName, location = resident.currentLocation.DisplayName });
        Text(batch, $"{name}  ·  {residentIndex + 1}/{residents.Length}", x + 76, y + 6, Gold, width - 152);
        string birthday = GetBirthday(resident);
        string relationship = Game1.player.friendshipData.TryGetValue(resident.Name, out Friendship friendship)
            ? T("gifts.relationship", new
            {
                hearts = Game1.player.getFriendshipHeartLevelForNPC(resident.Name),
                today = friendship.GiftsToday,
                week = friendship.GiftsThisWeek
            })
            : T("gifts.not-met");
        Text(batch, $"{birthday}   ·   {relationship}", x, y + row + 5, Ink, width);

        int filterWidth = width / GiftFilters.Length;
        for (int i = 0; i < GiftFilters.Length; i++)
        {
            int filter = i;
            Button(batch, new Rectangle(x + i * filterWidth, y + row * 2, filterWidth - 5, row - 6),
                T(GiftFilters[i].Key), () => { giftFilter = filter; giftOffset = 0; }, i == giftFilter);
        }

        int selectedTaste = GiftFilters[giftFilter].Taste;
        var tastes = GetGiftTastes(resident);
        visibleGifts = tastes.TryGetValue(selectedTaste, out var gifts) ? gifts : Array.Empty<ParsedItemData>();
        MoveGifts(0);
        if (visibleGifts.Length == 0)
            Text(batch, T("gifts.no-items"), x, y + row * 3 + 4, Muted, width);
        for (int i = 0; i < GiftRows && giftOffset + i < visibleGifts.Length; i++)
        {
            ParsedItemData item = visibleGifts[giftOffset + i];
            int itemY = y + row * (i + 3);
            int actionWidth = Math.Min(170, width / 3);
            Rectangle bounds = new(x, itemY, width - actionWidth - 7, row - 5);
            Fill(batch, bounds, Surface);
            DrawItemName(batch, bounds, item, bounds.Width - 8);
            Button(batch, new Rectangle(x + width - actionWidth, itemY, actionWidth, row - 5),
                T("gifts.add-one"), () => Spawn(item, 1, 0));
        }

        int footer = y + row * 6;
        Button(batch, new Rectangle(x, footer, 62, row - 5), "<", () => MoveGifts(-GiftRows));
        Button(batch, new Rectangle(x + width - 62, footer, 62, row - 5), ">", () => MoveGifts(GiftRows));
        string range = visibleGifts.Length == 0 ? T("items.zero") : T("items.range", new
        {
            start = giftOffset + 1,
            end = Math.Min(giftOffset + GiftRows, visibleGifts.Length),
            total = visibleGifts.Length
        });
        Text(batch, range, x + 76, footer + 4, Muted, width - 152);
    }

    private Dictionary<int, ParsedItemData[]> GetGiftTastes(NPC resident)
    {
        if (giftTastes.TryGetValue(resident.Name, out var tastes))
            return tastes;

        giftableItems ??= GetCatalog()
            .Select(data => (Data: data, Item: ItemRegistry.Create(data.QualifiedItemId, allowNull: true)))
            .Where(entry => entry.Item != null && entry.Item.canBeGivenAsGift())
            .ToArray();
        tastes = giftableItems
            .GroupBy(entry => resident.getGiftTasteForThisItem(entry.Item))
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Data).ToArray());
        giftTastes.Add(resident.Name, tastes);
        return tastes;
    }

    private string GetBirthday(NPC resident)
    {
        if (resident.Birthday_Day <= 0)
            return T("gifts.birthday-unknown");
        if (resident.isBirthday())
            return T("gifts.birthday-today");

        string season = T("season." + resident.Birthday_Season.ToString().ToLowerInvariant());
        return T("gifts.birthday", new { season, day = resident.Birthday_Day });
    }

    private void MoveResident(int amount)
    {
        if (residents is not { Length: > 0 })
            return;

        residentIndex = (residentIndex + amount + residents.Length) % residents.Length;
        giftFilter = 0;
        giftOffset = 0;
        status = "";
    }

    private void MoveGifts(int amount) =>
        giftOffset = Math.Clamp(giftOffset + amount, 0, Math.Max(0, visibleGifts.Length - GiftRows));
}
