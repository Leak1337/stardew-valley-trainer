# Stardew Valley Trainer

[Deutsche Anleitung](README.de.md)

An in-game trainer for Stardew Valley 1.6.15 on Windows. Press **F5** in a single-player save to open the menu. The trainer follows your game language and supports English and German.

| Tab | Features |
| --- | --- |
| Player | Gold, health, energy, backpack slots, a player-only movement speed bonus, and base-game debuff removal |
| World | Teleport to named destinations, return to your last departure point, advance or freeze time, and set tomorrow's weather |
| Items | Search by name or item ID, then select quantity and quality |
| Farm | Water soil, mature crops, refill watering cans, care for animals, repair fences, and finish running machines at your current location |
| Skills | Add XP to the five base-game skills |
| Friendship | Add friendship points for villagers you've already met |
| Gifts | See each villager's location, birthday, hearts, gift counts, and gift preferences. Add individual items to your inventory |
| Recipes | Count learned recipes and unlock cooking or crafting recipes |
| Mods | See which add-ons are loaded, save waypoints, and open Expanded's item filter when installed |
| Fancy | Random emotes, a light show, nearby villager emotes, and temporary luck and magnetism boosts |

Under **Gifts**, use the top arrows to switch villagers and the bottom arrows to browse items. **Loves** shows their best gifts. Ratings come from the game. Select **+1 to inventory** to get an item you can give to the villager.

Enter your own amounts for gold, items, XP, and friendship points.

Open **World → Fast travel** and choose a **Teleport to …** button to move there immediately. **Return to your last departure point** takes you back if that tile is still accessible. Use **World → Time & weather** to set tomorrow's valley forecast or advance the clock in multiples of 10 minutes. Clock advance stops before the forced end of the day.

Under **Player**, choose a movement speed bonus from 1 to 20 and apply it. The bonus affects only your character and lasts for the current play session until you reset it. Under **Fancy**, you can set the strength and duration of the temporary luck or magnetism boosts.

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
