using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private enum CommunityPage { Bundles, Museum, Quests }
    private const int ProgressRows = 3;
    private CommunityPage communityPage;
    private CenterBundle[]? centerBundles;
    private int centerBundleIndex;
    private int bundleOffset;
    private int bundleEntryCount;
    private bool confirmCommunity;

    private sealed record CenterBundle(int Id, int Area, string Name, int Required, bool HasReward,
        BundleIngredientDescription[] Ingredients);

    private void DrawCommunity(SpriteBatch batch, int x, int y, int width, int row)
    {
        int column = width / 3;
        Button(batch, new Rectangle(x, y, column - 5, row - 6), T("community.center"),
            () => SelectCommunityPage(CommunityPage.Bundles), communityPage == CommunityPage.Bundles);
        Button(batch, new Rectangle(x + column, y, column - 5, row - 6), T("community.museum"),
            () => SelectCommunityPage(CommunityPage.Museum), communityPage == CommunityPage.Museum);
        Button(batch, new Rectangle(x + 2 * column, y, width - 2 * column, row - 6), T("community.quests"),
            () => SelectCommunityPage(CommunityPage.Quests), communityPage == CommunityPage.Quests);

        switch (communityPage)
        {
            case CommunityPage.Bundles: DrawBundles(batch, x, y + row, width, row); break;
            case CommunityPage.Museum: DrawMuseum(batch, x, y + row, width, row); break;
            case CommunityPage.Quests: DrawQuests(batch, x, y + row, width, row); break;
        }
    }

    private void SelectCommunityPage(CommunityPage page)
    {
        communityPage = page;
        confirmCommunity = false;
        confirmQuest = null;
        status = "";
    }

    private void DrawBundles(SpriteBatch batch, int x, int y, int width, int row)
    {
        if (Game1.player.hasOrWillReceiveMail("JojaMember"))
        {
            Text(batch, T("community.joja"), x, y + 6, Muted, width);
            return;
        }

        if (Game1.getLocationFromName("CommunityCenter") is not CommunityCenter center)
        {
            Text(batch, T("community.unavailable"), x, y + 6, Muted, width);
            return;
        }

        centerBundles ??= ReadCenterBundles(center);
        if (centerBundles.Length == 0)
        {
            Text(batch, T("community.unavailable"), x, y + 6, Muted, width);
            return;
        }

        CenterBundle bundle = centerBundles[centerBundleIndex];
        int filled = CountFilled(center, bundle);
        bool complete = filled >= bundle.Required;
        string title = T("community.bundle-title", new
        {
            name = bundle.Name,
            area = CommunityCenter.getAreaDisplayNameFromNumber(bundle.Area),
            filled = Math.Min(filled, bundle.Required),
            required = bundle.Required,
            index = centerBundleIndex + 1,
            total = centerBundles.Length
        });
        Button(batch, new Rectangle(x, y, 54, row - 6), "<", () => MoveBundle(-1));
        Button(batch, new Rectangle(x + width - 54, y, 54, row - 6), ">", () => MoveBundle(1));
        Text(batch, title, x + 66, y + 6, Gold, width - 132);

        bool[] donations = center.bundles[bundle.Id];
        var pending = complete ? Array.Empty<BundleIngredientDescription>() : bundle.Ingredients
            .Where((ingredient, index) => !donations[index]).ToArray();
        bundleEntryCount = pending.Length;
        MoveBundleEntries(0);
        if (complete)
            Text(batch, T("community.bundle-complete"), x, y + row + 6, Mint, width);
        for (int i = 0; i < ProgressRows && i + bundleOffset < pending.Length; i++)
        {
            BundleIngredientDescription requirement = pending[i + bundleOffset];
            DrawBundleRequirement(batch, x, y + row * (i + 1), width, row, bundle, requirement);
        }

        int pagerY = y + row * 4;
        if (pending.Length > ProgressRows)
            DrawProgressPager(batch, x, pagerY, width, row, bundleOffset, pending.Length, MoveBundleEntries);
        else if (!complete)
            Text(batch, T("community.donate-note"), x, pagerY + 6, Muted, width);

        int footer = y + row * 5;
        int actionWidth = width / 3;
        if (!complete)
        {
            Button(batch, new Rectangle(x, footer, actionWidth - 5, row - 6),
                T("community.spawn-required"), () => SpawnBundleRequirements(bundle, pending, bundle.Required - filled));
            Button(batch, new Rectangle(x + actionWidth, footer, actionWidth - 5, row - 6),
                T("community.complete-selected"), () => CompleteSelectedBundle(center, bundle));
        }
        Button(batch, new Rectangle(x + actionWidth * 2, footer, width - actionWidth * 2, row - 6),
            T(confirmCommunity ? "community.confirm-all" : "community.complete-all"),
            () => CompleteCenter(center), confirmCommunity);
    }

    private static CenterBundle[] ReadCenterBundles(CommunityCenter center)
    {
        var entries = new List<CenterBundle>();
        foreach ((string key, string value) in Game1.netWorldState.Value.BundleData)
        {
            string[] location = key.Split('/');
            string[] fields = value.Split('/');
            if (location.Length != 2 || !int.TryParse(location[1], out int id)
                || !center.bundles.ContainsKey(id) || fields.Length <= Bundle.IngredientsIndex)
                continue;

            int area = CommunityCenter.getAreaNumberFromName(location[0]);
            if (area < 0 || area >= center.areasComplete.Count)
                continue;

            string[] raw = ArgUtility.SplitBySpace(fields[Bundle.IngredientsIndex]);
            if (raw.Length == 0 || raw.Length % 3 != 0 || center.bundles[id].Length < raw.Length / 3)
                continue;

            var ingredients = new List<BundleIngredientDescription>(raw.Length / 3);
            for (int i = 0; i < raw.Length; i += 3)
            {
                if (!int.TryParse(raw[i + 1], out int count) || count <= 0
                    || !int.TryParse(raw[i + 2], out int quality))
                    break;

                ingredients.Add(new BundleIngredientDescription(raw[i], count, quality, false, null));
            }
            if (ingredients.Count != raw.Length / 3)
                continue;

            int required = ArgUtility.GetInt(fields, Bundle.NumberOfSlotsIndex, ingredients.Count);
            if (required <= 0 || required > ingredients.Count)
                continue;

            string name = ArgUtility.Get(fields, Bundle.DisplayNameIndex, fields[Bundle.NameIndex]);
            entries.Add(new CenterBundle(id, area, name, required, fields[Bundle.RewardIndex].Length > 0,
                ingredients.ToArray()));
        }
        return entries.OrderBy(entry => entry.Area).ThenBy(entry => entry.Id).ToArray();
    }

    private void DrawBundleRequirement(SpriteBatch batch, int x, int y, int width, int row, CenterBundle bundle,
        BundleIngredientDescription requirement)
    {
        int actionWidth = Math.Min(160, width / 3);
        var bounds = new Rectangle(x, y, width - actionWidth - 7, row - 5);
        Fill(batch, bounds, Surface);
        Rectangle button = new(x + width - actionWidth, y, actionWidth, row - 5);
        if (IsVaultRequirement(bundle, requirement))
        {
            Text(batch, T("community.gold-required", new { amount = requirement.stack.ToString("N0") }),
                x + 10, y + 5, Gold, bounds.Width - 20);
            Button(batch, button, T("player.gold-add"), () => SpawnBundleRequirement(bundle, requirement));
            return;
        }

        ParsedItemData? data = GetBundleItem(requirement);
        if (data is null)
        {
            Text(batch, T("community.item-unavailable"), x + 10, y + 5, Muted, bounds.Width - 20);
            return;
        }
        int detailsWidth = bounds.Width / 4;
        DrawItemName(batch, bounds, data, bounds.Width - detailsWidth);
        string quality = requirement.quality switch
        {
            1 => T("quality.silver"),
            2 => T("quality.gold"),
            4 => T("quality.iridium"),
            _ => T("quality.normal")
        };
        Text(batch, T("community.requirement", new { count = requirement.stack, quality }),
            bounds.Right - detailsWidth, y + 5, Muted, detailsWidth - 8);
        Button(batch, button, T("community.spawn-item"), () => SpawnBundleRequirement(bundle, requirement));
    }

    private static bool IsVaultRequirement(CenterBundle bundle, BundleIngredientDescription requirement) =>
        bundle.Area == 4 && requirement.category == -1;

    private static ParsedItemData? GetBundleItem(BundleIngredientDescription requirement)
    {
        ParsedItemData? item = ItemRegistry.GetData(JunimoNoteMenu.GetRepresentativeItemId(requirement));
        return item is not null && (requirement.category is null || item.Category == requirement.category) ? item : null;
    }

    private void SpawnBundleRequirement(CenterBundle bundle, BundleIngredientDescription requirement)
    {
        if (IsVaultRequirement(bundle, requirement))
        {
            status = T("player.gold-given", new { amount = ModEntry.AddGold(requirement.stack).ToString("N0") });
        }
        else if (GetBundleItem(requirement) is ParsedItemData data)
            Spawn(data, requirement.stack, requirement.quality);
    }

    private void SpawnBundleRequirements(CenterBundle bundle, BundleIngredientDescription[] pending, int needed)
    {
        int sets = 0;
        foreach (BundleIngredientDescription requirement in pending)
        {
            if (sets >= needed)
                break;

            if (IsVaultRequirement(bundle, requirement))
            {
                SpawnBundleRequirement(bundle, requirement);
                return;
            }
            if (GetBundleItem(requirement) is not ParsedItemData data)
                continue;
            if (AddItemsToInventory(data, requirement.stack, requirement.quality) < requirement.stack)
                break;

            sets++;
        }
        status = T(sets == needed ? "community.spawned" : "community.spawned-partial", new { count = sets, needed });
    }

    private static int CountFilled(CommunityCenter center, CenterBundle bundle) =>
        center.bundles[bundle.Id].Take(bundle.Ingredients.Length).Count(complete => complete);

    private static bool MarkBundle(CommunityCenter center, CenterBundle bundle)
    {
        if (CountFilled(center, bundle) >= bundle.Required)
            return false;

        var flags = center.bundles.FieldDict[bundle.Id];
        for (int i = 0; i < flags.Count; i++)
            flags[i] = true;
        if (bundle.HasReward)
            center.bundleRewards[bundle.Id] = true;
        return true;
    }

    private void CompleteSelectedBundle(CommunityCenter center, CenterBundle bundle)
    {
        confirmCommunity = false;
        if (!MarkBundle(center, bundle))
            return;

        RestoreCompletedRooms(center);
        status = T("community.completed-selected", new { name = bundle.Name });
    }

    private void CompleteCenter(CommunityCenter center)
    {
        var bundles = centerBundles!;
        if (bundles.All(bundle => CountFilled(center, bundle) >= bundle.Required))
        {
            status = T("community.already-complete");
            return;
        }
        if (!confirmCommunity)
        {
            confirmCommunity = true;
            status = T("community.confirm-note");
            return;
        }

        confirmCommunity = false;
        int count = bundles.Count(bundle => MarkBundle(center, bundle));
        RestoreCompletedRooms(center);
        status = center.areAllAreasComplete()
            ? T("community.completed-all", new { count })
            : T("community.unsupported");
    }

    private void RestoreCompletedRooms(CommunityCenter center)
    {
        foreach (var room in centerBundles!.GroupBy(bundle => bundle.Area))
        {
            if (center.areasComplete[room.Key] || room.Any(bundle => CountFilled(center, bundle) < bundle.Required)
                || !AllRoomBundlesSupported(room.Key, room.Count()))
                continue;

            center.areasComplete[room.Key] = true;
            center.areaCompleteReward(room.Key);
            center.loadArea(room.Key, false);
        }
        center.checkForNewJunimoNotes();
    }

    private static bool AllRoomBundlesSupported(int area, int handledCount) =>
        Game1.netWorldState.Value.BundleData.Keys.Count(key =>
        {
            int separator = key.IndexOf('/');
            return separator > 0 && CommunityCenter.getAreaNumberFromName(key[..separator]) == area;
        }) == handledCount;

    private void MoveBundle(int direction)
    {
        centerBundleIndex = (centerBundleIndex + direction + centerBundles!.Length) % centerBundles.Length;
        bundleOffset = 0;
        confirmCommunity = false;
        status = "";
    }

    private void MoveBundleEntries(int amount) =>
        bundleOffset = Math.Clamp(bundleOffset + amount, 0, Math.Max(0, bundleEntryCount - ProgressRows));

    private void DrawProgressPager(SpriteBatch batch, int x, int y, int width, int row, int offset, int total, Action<int> move)
    {
        Button(batch, new Rectangle(x, y, 54, row - 5), "<", () => move(-ProgressRows));
        Button(batch, new Rectangle(x + width - 54, y, 54, row - 5), ">", () => move(ProgressRows));
        Text(batch, T(total == 0 ? "items.zero" : "items.range", new
        {
            start = offset + 1,
            end = Math.Min(offset + ProgressRows, total),
            total
        }), x + 66, y + 5, Muted, width - 132);
    }

    private void MoveProgressEntries(int amount)
    {
        switch (communityPage)
        {
            case CommunityPage.Bundles: MoveBundleEntries(amount); break;
            case CommunityPage.Museum: MoveMuseum(amount); break;
            case CommunityPage.Quests: MoveQuests(amount); break;
        }
    }
}
