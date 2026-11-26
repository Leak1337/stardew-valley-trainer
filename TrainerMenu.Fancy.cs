using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private const string FancyLuckId = "Leak1337.StardewValleyTrainer.FancyLuck";
    private const string FancyMagnetId = "Leak1337.StardewValleyTrainer.FancyMagnet";
    private static readonly int[] PartyEmotes = { 8, 12, 16, 20, 28, 32 };

    private void DrawFancy(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("fancy.cosmetic"), x, y + 6, Mint, width);
        Pair(batch, x, y + row, width, row, T("fancy.emote"), RollEmote, T("fancy.lights"), ShowLights);
        Button(batch, new Rectangle(x, y + row * 2, width, row - 6), T("fancy.villagers"), StartVillagerParty);
        Text(batch, T("fancy.temporary"), x, y + row * 3 + 6, Mint, width);

        int half = width / 2;
        int labelWidth = half / 2;
        int inputY = y + row * 4;
        Text(batch, T("fancy.strength"), x, inputY + 7, Muted, labelWidth - 8);
        DrawInput(batch, fancyStrength, x + labelWidth, inputY, half - labelWidth - 8);
        Text(batch, T("fancy.duration"), x + half + 4, inputY + 7, Muted, labelWidth - 8);
        DrawInput(batch, fancyDuration, x + half + labelWidth, inputY, width - half - labelWidth);

        Pair(batch, x, y + row * 5, width, row, T("fancy.luck"), () => ApplyFancyBuff(false),
            T("fancy.magnet"), () => ApplyFancyBuff(true));
        Button(batch, new Rectangle(x, y + row * 6, width, row - 6), T("fancy.reset"), () =>
        {
            Game1.player.buffs.Remove(FancyLuckId);
            Game1.player.buffs.Remove(FancyMagnetId);
            status = T("fancy.reset-done");
        });
    }

    private void RollEmote()
    {
        Close();
        Game1.player.doEmote(PartyEmotes[Random.Shared.Next(PartyEmotes.Length)]);
    }

    private void ShowLights()
    {
        Close();
        Game1.screenGlowOnce(new Color(112, 64, 165), false, 0.008f, 0.015f);
        Game1.playSound("newArtifact");
    }

    private void StartVillagerParty()
    {
        int count = 0;
        foreach (NPC villager in Game1.currentLocation.characters)
        {
            if (!villager.IsVillager || Vector2.DistanceSquared(villager.Tile, Game1.player.Tile) > 100)
                continue;

            villager.doEmote(PartyEmotes[Random.Shared.Next(PartyEmotes.Length)]);
            count++;
        }

        if (count > 0)
            Close();
        else
            status = T("fancy.no-villagers");
    }

    private void ApplyFancyBuff(bool magnet)
    {
        if (!TryReadAmount(fancyStrength, out int strength) || !TryReadAmount(fancyDuration, out int seconds))
            return;
        if (strength > 20 || seconds > 600)
        {
            status = T("fancy.limits");
            return;
        }

        BuffEffects effects = new();
        if (magnet)
            effects.MagneticRadius.Value = strength * Game1.tileSize;
        else
            effects.LuckLevel.Value = strength;

        string label = T(magnet ? "fancy.magnet" : "fancy.luck");
        string id = magnet ? FancyMagnetId : FancyLuckId;
        Game1.player.buffs.Remove(id);
        Game1.player.applyBuff(new Buff(id, duration: seconds * 1000, effects: effects, displayName: label));
        status = T("fancy.activated", new { effect = label, seconds });
    }
}
