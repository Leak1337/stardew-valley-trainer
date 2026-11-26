# Stardew Valley Trainer

[Deutsche Anleitung](README.md)

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

Under **Gifts**, use the top arrows to switch villagers and the bottom arrows to browse items. **Loves** shows their best gifts. Ratings come from the game. Select **+1 to inventory** to get an item you can give to the villager.

The clock resumes the next morning. Special days can override the selected weather. Permanent changes take effect in your save when the game next saves normally.

## Build and install

1. Install [SMAPI](https://smapi.io/) for your Stardew Valley version. This trainer was built against Stardew Valley 1.6.15 and SMAPI 4.1.10.
2. With a .NET SDK installed, build the trainer and set `-GamePath` to your game directory.

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1 -GamePath 'C:\Path\to\Stardew Valley'
   ```

3. Copy `dist\StardewValleyTrainer` into the game's `Mods` directory.
4. Start `StardewModdingAPI.exe` and load a single-player save.

Stardew Valley Expanded support is planned for a future update.
