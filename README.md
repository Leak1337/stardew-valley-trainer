# Stardew Valley Trainer

**[Direct download – StardewValleyTrainer.zip](https://github.com/Leak1337/stardew-valley-trainer/releases/latest/download/StardewValleyTrainer.zip)**

Extract the ZIP into `Mods` and start Stardew Valley through SMAPI.

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
| Community | Browse actual Community Center bundles, spawn required items at their required quality, complete a bundle or the center, find missing museum donations, and finish journal quests |
| Mods | See which add-ons are loaded, save waypoints, and open Expanded's item filter when installed |
| Equipment | Change your selected tool tier, teleport to a chosen mine floor, or place a ladder down |
| Fancy | Random emotes, a light show, nearby villager emotes, and temporary luck and magnetism boosts |

Under **Gifts**, use the top arrows to switch villagers and the bottom arrows to browse items. **Loves** shows their best gifts. Ratings come from the game. Select **+1 to inventory** to get an item you can give to the villager.

Enter your own amounts for gold, items, XP, and friendship points.

Open **Community → Community Center** to browse bundles and get the missing items. Item spawns leave the donation for you. **Complete this bundle** fills the selected bundle immediately, keeps its unclaimed reward available, and unlocks the room when all of its bundles are finished. **Complete the center** asks for confirmation before filling all remaining Community Center bundles. The tracker uses the loaded save's bundle definitions, including remixed bundles. The Joja route does not use these actions.

**Community → Museum** lists missing donations so you can get an item and give it to Gunther. **Community → Quests** can complete a visible journal quest after confirmation, with any available reward left in the quest journal. Under **Equipment**, select a supported tool on your toolbar before changing its tier. The mine controls accept an exact floor and require the Skull Key for Skull Cavern.

Open **World → Fast travel** and choose a **Teleport to …** button to move there immediately. **Return to your last departure point** takes you back if that tile is still accessible. Use **World → Time & weather** to set tomorrow's valley forecast or advance the clock in multiples of 10 minutes. Clock advance stops before the forced end of the day.

Under **Player**, choose a movement speed bonus from 1 to 20 and apply it. The bonus affects only your character and lasts for the current play session until you reset it. Under **Fancy**, you can set the strength and duration of the temporary luck or magnetism boosts.

## Vanilla and modded saves

SMAPI is the only required game add-on. Items, villagers, gift tastes, and recipes are read from the game's loaded data, including content added or edited by compatible mods.

Search **Gifts** by a villager's display name or internal name. If [Stardew Valley Expanded](https://github.com/FlashShifter/StardewValleyExpanded) is loaded, **Mods** shows its version and provides shortcuts to its namespaced items and supported Expanded villagers. Both screens have an **All** filter for the full loaded catalog or villager list. Other installed add-ons appear by name and version under **Detected mods**.

With Expanded loaded, **World → Expanded travel** uses entrances from the live maps for Blue Moon Vineyard, Shearwater Bridge, and Adventurer Summit. **Recipes** can unlock just the cooking or crafting recipes whose outputs belong to Expanded.

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
