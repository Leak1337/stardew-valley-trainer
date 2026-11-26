using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
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
    }

    private static int CountKnownRecipes(bool cooking)
    {
        var recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
        var learned = cooking ? Game1.player.cookingRecipes : Game1.player.craftingRecipes;
        int count = 0;
        foreach (string name in recipes.Keys)
        {
            if (learned.ContainsKey(name))
                count++;
        }
        return count;
    }

    private void LearnRecipes(bool cooking)
    {
        var recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
        var learned = cooking ? Game1.player.cookingRecipes : Game1.player.craftingRecipes;
        int count = 0;
        foreach (string name in recipes.Keys)
        {
            if (learned.ContainsKey(name))
                continue;

            learned.Add(name, 0);
            count++;
        }

        status = T("recipes.learned", new { count });
    }
}
