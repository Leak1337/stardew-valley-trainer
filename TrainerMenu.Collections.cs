using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Quests;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private ParsedItemData[]? museumCatalog;
    private ParsedItemData[] missingDonations = Array.Empty<ParsedItemData>();
    private int museumOffset;
    private Quest[] activeQuests = Array.Empty<Quest>();
    private int questOffset;
    private Quest? confirmQuest;

    private void DrawMuseum(SpriteBatch batch, int x, int y, int width, int row)
    {
        if (Game1.getLocationFromName("ArchaeologyHouse") is not LibraryMuseum)
        {
            Text(batch, T("museum.no-location"), x, y + 6, Muted, width);
            return;
        }

        museumCatalog ??= GetCatalog().Where(item => LibraryMuseum.IsItemSuitableForDonation(item.QualifiedItemId, false)).ToArray();
        missingDonations = museumCatalog.Where(item => !LibraryMuseum.HasDonatedArtifact(item.QualifiedItemId)).ToArray();
        MoveMuseum(0);
        Text(batch, T("museum.progress", new
        {
            count = museumCatalog.Length - missingDonations.Length,
            total = museumCatalog.Length
        }), x, y + 6, Gold, width);
        if (missingDonations.Length == 0)
            Text(batch, T("museum.complete"), x, y + row + 6, Mint, width);

        int actionWidth = Math.Min(166, width / 3);
        for (int i = 0; i < ProgressRows && i + museumOffset < missingDonations.Length; i++)
        {
            ParsedItemData item = missingDonations[i + museumOffset];
            int line = y + row * (i + 1);
            Rectangle card = new(x, line, width - actionWidth - 7, row - 5);
            Fill(batch, card, Surface);
            DrawItemName(batch, card, item, card.Width);
            Button(batch, new Rectangle(x + width - actionWidth, line, actionWidth, row - 5),
                T("gifts.add-one"), () => Spawn(item, 1, 0));
        }
        if (missingDonations.Length > ProgressRows)
            DrawProgressPager(batch, x, y + row * 4, width, row, museumOffset, missingDonations.Length, MoveMuseum);
        Text(batch, T("museum.note"), x, y + row * 5 + 6, Muted, width);
    }

    private void MoveMuseum(int offset) =>
        museumOffset = Math.Clamp(museumOffset + offset, 0, Math.Max(0, missingDonations.Length - ProgressRows));

    private void DrawQuests(SpriteBatch batch, int x, int y, int width, int row)
    {
        activeQuests = Game1.player.questLog
            .Where(quest => !quest.completed.Value && !quest.IsHidden())
            .ToArray();
        MoveQuests(0);
        Text(batch, T("quests.progress", new { count = activeQuests.Length }), x, y + 6, Gold, width);
        if (activeQuests.Length == 0)
            Text(batch, T("quests.empty"), x, y + row + 6, Muted, width);

        int actionWidth = Math.Min(198, width / 3);
        for (int i = 0; i < ProgressRows && i + questOffset < activeQuests.Length; i++)
        {
            Quest quest = activeQuests[i + questOffset];
            bool confirmed = ReferenceEquals(confirmQuest, quest);
            int line = y + row * (i + 1);
            Rectangle card = new(x, line, width - actionWidth - 7, row - 5);
            Fill(batch, card, Surface);
            Text(batch, quest.questTitle, x + 12, line + 5, Ink, card.Width - 24);
            Button(batch, new Rectangle(x + width - actionWidth, line, actionWidth, row - 5),
                T(confirmed ? "quests.confirm" : "quests.finish"), () => FinishQuest(quest), confirmed);
        }
        if (activeQuests.Length > ProgressRows)
            DrawProgressPager(batch, x, y + row * 4, width, row, questOffset, activeQuests.Length, MoveQuests);
        Text(batch, T("quests.note"), x, y + row * 5 + 6, Muted, width);
    }

    private void MoveQuests(int offset) =>
        questOffset = Math.Clamp(questOffset + offset, 0, Math.Max(0, activeQuests.Length - ProgressRows));

    private void FinishQuest(Quest quest)
    {
        if (quest.completed.Value || !Game1.player.questLog.Contains(quest))
            return;

        if (!ReferenceEquals(confirmQuest, quest))
        {
            confirmQuest = quest;
            status = T("quests.confirm-note", new { name = quest.questTitle });
            return;
        }

        confirmQuest = null;
        string name = quest.questTitle;
        quest.questComplete();
        status = T("quests.finished", new { name });
    }
}
