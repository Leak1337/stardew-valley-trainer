using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private string[]? expandedCraftingRecipes;
    private string[]? expandedCookingRecipes;

    private void DrawRecipes(SpriteBatch batch, int x, int y, int width, int row)
    {
        Text(batch, T("recipes.crafting-count", new
        {
            known = CountKnownRecipes(false),
            total = CraftingRecipe.craftingRecipes.Count
        }), x, y + 6, Gold, width);
        Button(batch, new Rectangle(x, y + row, width, row - 6), T("recipes.learn-crafting"), () => LearnRecipes(false));
        Text(batch, T("recipes.cooking-count", new
        {
            known = CountKnownRecipes(true),
            total = CraftingRecipe.cookingRecipes.Count
        }), x, y + row * 2 + 6, Gold, width);
        Button(batch, new Rectangle(x, y + row * 3, width, row - 6), T("recipes.learn-cooking"), () => LearnRecipes(true));
        Text(batch, T("recipes.note"), x, y + row * 4 + 6, Muted, width);
        if (mod.ExpandedVersion is not null)
        {
            string[] crafting = expandedCraftingRecipes ??= GetExpandedRecipes(false);
            string[] cooking = expandedCookingRecipes ??= GetExpandedRecipes(true);
            Text(batch, T("recipes.expanded-progress", new
            {
                known = CountKnownRecipes(false, crafting) + CountKnownRecipes(true, cooking),
                total = crafting.Length + cooking.Length
            }), x, y + row * 5 + 6, Mint, width);
            Pair(batch, x, y + row * 6, width, row, T("recipes.expanded-crafting"), () => LearnRecipes(false, crafting),
                T("recipes.expanded-cooking"), () => LearnRecipes(true, cooking));
        }
    }

    private static string[] GetExpandedRecipes(bool cooking)
    {
        var recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
        return recipes.Where(recipe => ArgUtility.SplitBySpace(
                ArgUtility.Get(recipe.Value.Split('/'), CraftingRecipe.index_output, "")).Any(IsExpandedItem))
            .Select(recipe => recipe.Key).ToArray();
    }

    private static int CountKnownRecipes(bool cooking, string[]? filter = null)
    {
        var recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
        var learned = cooking ? Game1.player.cookingRecipes : Game1.player.craftingRecipes;
        int count = 0;
        foreach (string name in filter ?? recipes.Keys.AsEnumerable())
        {
            if (learned.ContainsKey(name))
                count++;
        }
        return count;
    }

    private void LearnRecipes(bool cooking, string[]? filter = null)
    {
        var recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
        var learned = cooking ? Game1.player.cookingRecipes : Game1.player.craftingRecipes;
        int count = 0;
        foreach (string name in filter ?? recipes.Keys.AsEnumerable())
        {
            if (learned.ContainsKey(name))
                continue;

            learned.Add(name, 0);
            count++;
        }

        status = T("recipes.learned", new { count });
    }
}
