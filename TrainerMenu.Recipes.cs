using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyTrainer;

internal sealed partial class TrainerMenu
{
    private ContentSource recipeSource;
    private string[]? packCraftingRecipes;
    private string[]? packCookingRecipes;

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
        if (HasContentSources)
        {
            if (recipeSource == ContentSource.All)
                recipeSource = NextSource(recipeSource);
            string source = SourceName(recipeSource);
            string[] crafting = packCraftingRecipes ??= GetPackRecipes(false, recipeSource);
            string[] cooking = packCookingRecipes ??= GetPackRecipes(true, recipeSource);
            int filterWidth = mod.ExpandedVersion is not null && mod.RidgesideVersion is not null ? width / 3 : 0;
            Text(batch, T("recipes.pack-progress", new
            {
                source,
                known = CountKnownRecipes(false, crafting) + CountKnownRecipes(true, cooking),
                total = crafting.Length + cooking.Length
            }), x, y + row * 5 + 6, Mint, width - filterWidth - 8);
            if (filterWidth > 0)
            {
                Button(batch, new Rectangle(x + width - filterWidth, y + row * 5, filterWidth, row - 6),
                    T("mods.filter-source", new { source }), () =>
                    {
                        recipeSource = recipeSource == ContentSource.Expanded ? ContentSource.Ridgeside : ContentSource.Expanded;
                        packCraftingRecipes = packCookingRecipes = null;
                    });
            }
            Pair(batch, x, y + row * 6, width, row, T("recipes.pack-crafting", new { source }), () => LearnRecipes(false, crafting),
                T("recipes.pack-cooking", new { source }), () => LearnRecipes(true, cooking));
        }
    }

    private static string[] GetPackRecipes(bool cooking, ContentSource source)
    {
        var recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
        return recipes.Where(recipe => ArgUtility.SplitBySpace(
                ArgUtility.Get(recipe.Value.Split('/'), CraftingRecipe.index_output, "")).Any(id =>
                    MatchesContentItem(ItemRegistry.GetData(id)?.ItemId ?? id, source)))
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
