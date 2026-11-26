using System.Linq;
using StardewValley;
using StardewValley.Tools;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private void RefillWateringCans()
    {
        int count = 0;
        foreach (WateringCan can in Game1.player.Items.OfType<WateringCan>())
        {
            if (can.WaterLeft >= can.waterCanMax)
                continue;

            can.WaterLeft = can.waterCanMax;
            count++;
        }

        status = T("farm.refilled", new { count });
    }

    private void TendAnimals(bool pet)
    {
        GameLocation location = Game1.currentLocation;
        var animals = location switch
        {
            Farm farm => farm.animals.Values.AsEnumerable(),
            AnimalHouse house => house.animals.Values.AsEnumerable(),
            _ => Enumerable.Empty<FarmAnimal>()
        };
        int count = 0;
        foreach (FarmAnimal animal in animals)
        {
            if (pet)
            {
                if (animal.wasPet.Value)
                    continue;

                animal.pet(Game1.player);
            }
            else
            {
                if (animal.fullness.Value == byte.MaxValue)
                    continue;

                animal.fullness.Value = byte.MaxValue;
            }

            count++;
        }

        status = T(pet ? "farm.petted" : "farm.fed", new { count });
    }
}
