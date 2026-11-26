# Stardew Valley Trainer

[Deutsche Anleitung](README.de.md)

An in-game trainer for Stardew Valley 1.6.15 on Windows. Press **F5** in a single-player save to open the menu. The trainer follows your game language and supports English and German.

| Tab | Features |
| --- | --- |
| Player | Gold, health, energy, and backpack slots |
| World | Freeze the clock, set tomorrow's weather to sunny or rainy, and travel to base-game locations |
| Items | Search by name or item ID, then select quantity and quality |
| Farm | Water planted soil, mature crops, refill watering cans, and feed or pet animals at your current location |
| Skills | Add XP to the five base-game skills |
| Friendship | Add friendship points for villagers you've already met |
| Gifts | See each villager's location, birthday, hearts, gift counts, and gift preferences. Add individual items to your inventory |
| Recipes | Count learned recipes and unlock cooking or crafting recipes |
| Mods | See which add-ons are loaded, save waypoints, and open Expanded's item filter when installed |

Under **Gifts**, use the top arrows to switch villagers and the bottom arrows to browse items. **Loves** shows their best gifts. Ratings come from the game. Select **+1 to inventory** to get an item you can give to the villager.

Enter your own amounts for gold, items, XP, and friendship points.

## Vanilla and modded saves

SMAPI is the only required game add-on. Items, villagers, gift tastes, and recipes are read from the game's loaded data, including content added or edited by compatible mods.

If [Stardew Valley Expanded](https://github.com/FlashShifter/StardewValleyExpanded) is loaded, **Mods** shows its version and adds an item filter for Expanded's namespaced items. The **All** filter continues to show the full loaded catalog. Other installed add-ons appear by name and version under **Detected mods**.

You can mark up to three tiles you've reached on persistent maps, including mod maps. Waypoints are stored per save when you save the game. If the map disappears or the tile gets blocked, travel is unavailable.

The clock resumes the next morning. Special days can override the selected weather. Permanent changes take effect in your save when the game next saves normally.

## Build and install

1. Install [SMAPI](https://smapi.io/) for your Stardew Valley version. This trainer was built against Stardew Valley 1.6.15 and SMAPI 4.1.10.
2. With a .NET SDK installed, build the trainer and set `-GamePath` to your game directory.

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1 -GamePath 'C:\Path\to\Stardew Valley'
   ```

3. Copy `dist\StardewValleyTrainer` into the game's `Mods` directory.
4. Start `StardewModdingAPI.exe` and load a single-player save.
