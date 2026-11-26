using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private bool showTimeAndWeather;

    private void DrawWorld(SpriteBatch batch, int x, int y, int width, int row)
    {
        Pair(batch, x, y, width, row, T("world.teleport-heading"),
            () => { showTimeAndWeather = false; status = ""; },
            T("world.time-heading"), () => { showTimeAndWeather = true; status = ""; },
            !showTimeAndWeather, showTimeAndWeather);

        if (showTimeAndWeather)
            DrawTimeAndWeather(batch, x, y, width, row);
        else
            DrawTeleport(batch, x, y, width, row);
    }

    private string TeleportLabel(string place) => T("world.teleport-to", new { place = T(place) });

    private void DrawTeleport(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("world.teleport-note"), x, y + row + 6, Muted, width);
        Pair(batch, x, y + row * 2, width, row, TeleportLabel("world.travel.farm"), WarpFarm,
            TeleportLabel("world.travel.town"), () => Warp("Town", 35, 57));
        Pair(batch, x, y + row * 3, width, row, TeleportLabel("world.travel.beach"), () => Warp("Beach", 20, 4),
            TeleportLabel("world.travel.mine"), () => Warp("Mine", 18, 13));
        Pair(batch, x, y + row * 4, width, row, TeleportLabel("world.travel.railroad"), WarpRailroad,
            TeleportLabel("world.travel.bus"), () => Warp("BusStop", 14, 23));
        Pair(batch, x, y + row * 5, width, row, TeleportLabel("world.travel.forest"),
            () => WarpViaMap("Forest", "Farm", "Town", "BusStop"),
            TeleportLabel("world.travel.mountain"), () => WarpViaMap("Mountain", "Town", "Backwoods", "Railroad"));
        Pair(batch, x, y + row * 6, width, row, T("world.teleport-back"), ReturnFromTeleport,
            T("world.open-waypoints"), () => { tab = 9; showModList = false; status = ""; });
    }

    private void DrawTimeAndWeather(SpriteBatch batch, int x, int y, int width, int row)
    {
        int time = Game1.timeOfDay;
        Text(batch, T("world.clock", new
        {
            day = Game1.dayOfMonth,
            year = Game1.year,
            time = $"{time / 100:00}:{time % 100:00}"
        }), x, y + row + 6, Gold, width);

        int half = width / 2;
        Button(batch, new Rectangle(x, y + row * 2, half - 5, row - 6),
            T("world.freeze", new { state = OnOff(mod.FreezeTime) }), () => mod.FreezeTime = !mod.FreezeTime, mod.FreezeTime);
        int actionWidth = (width - half) * 2 / 3;
        DrawInput(batch, clockMinutes, x + half + 5, y + row * 2, width - half - actionWidth - 10);
        Button(batch, new Rectangle(x + width - actionWidth, y + row * 2, actionWidth, row - 6),
            T("world.advance"), AdvanceClock);

        string weatherKey = Game1.weatherForTomorrow switch
        {
            "Sun" => "world.sun",
            "Rain" => "world.rain",
            "Storm" => "world.storm",
            "Snow" => "world.snow",
            _ => "world.other-weather"
        };
        Text(batch, T("world.forecast", new { weather = T(weatherKey) }), x, y + row * 3 + 6, Mint, width);
        Pair(batch, x, y + row * 4, width, row, T("world.sun"), () => SetWeather("Sun", "world.sun"),
            T("world.rain"), () => SetWeather("Rain", "world.rain"), weatherKey == "world.sun", weatherKey == "world.rain");
        Pair(batch, x, y + row * 5, width, row, T("world.storm"), () => SetWeather("Storm", "world.storm"),
            T("world.snow"), () => SetWeather("Snow", "world.snow"), weatherKey == "world.storm", weatherKey == "world.snow");
        Text(batch, T("world.weather-note"), x, y + row * 6 + 6, Muted, width);
    }

    private void AdvanceClock()
    {
        if (!TryReadAmount(clockMinutes, out int minutes))
            return;
        if (minutes < 10 || minutes % 10 != 0)
        {
            status = T("world.advance-invalid");
            return;
        }

        int advanced = 0;
        while (advanced < minutes && Game1.timeOfDay < 2550)
        {
            int before = Game1.timeOfDay;
            Game1.performTenMinuteClockUpdate();
            if (Game1.timeOfDay <= before)
                break;

            advanced += 10;
        }
        status = advanced == 0 ? T("world.advance-stop") : T("world.advanced", new { minutes = advanced });
    }

    private void SetWeather(string weather, string key)
    {
        Game1.weatherForTomorrow = weather;
        Game1.netWorldState.Value.GetWeatherForLocation("Default").WeatherForTomorrow = weather;
        status = T("world.weather-set", new { weather = T(key) });
    }

    private void ReturnFromTeleport()
    {
        if (mod.PreviousTeleport is Waypoint previous)
            TravelToWaypoint(previous);
        else
            status = T("world.teleport-back-empty");
    }

    private void WarpViaMap(string destinationName, params string[] sources)
    {
        GameLocation? destination = Game1.getLocationFromName(destinationName);
        if (destination is not null)
        {
            foreach (string source in sources)
            {
                var entrance = Game1.getLocationFromName(source)?.warps.FirstOrDefault(warp =>
                    string.Equals(warp.TargetName, destinationName, StringComparison.OrdinalIgnoreCase)
                    && IsWalkable(destination, warp.TargetX, warp.TargetY));
                if (entrance is null)
                    continue;

                Warp(destinationName, entrance.TargetX, entrance.TargetY);
                return;
            }
        }

        status = T("world.no-destination");
    }
}
