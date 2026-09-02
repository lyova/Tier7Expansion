using System;
using HarmonyLib;

namespace Tier7Expansion
{
    /// <summary>
    /// Everything needed to make quality level 7 exist at all.
    ///
    /// The tier cap itself is data driven - ItemClassesFromXml reads max_quality_tier off the
    /// items.xml root into ItemClass.MaxQualityTier - but two places around it assume six: the
    /// colour table is a fixed array, and the crafting tier falls back to a literal 6 when crafting
    /// progression is switched off.
    /// </summary>
    [HarmonyPatch]
    public static class QualityPatches
    {
        public const int Tier = 7;

        /// <summary>
        /// Raise the cap without stomping a mod that wants it higher. This runs after items.xml
        /// has been parsed, and nothing reads MaxQualityTier until the sandbox options are
        /// applied, so setting it here is order independent - unlike an XML setattribute, which
        /// would depend on which mod loads last.
        /// </summary>
        public static void RaiseMaxQualityTier()
        {
            if (ItemClass.MaxQualityTier >= Tier) return;
            ItemClass.MaxQualityTier = Tier;
            Log.Out($"[Tier7Expansion] ItemClass.MaxQualityTier -> {ItemClass.MaxQualityTier}");
        }

        /// <summary>
        /// QualityInfo.qualityColors is allocated as new Color[7], so Add(7, ...) from our
        /// qualityinfo.xml would go out of bounds. Grow on demand instead of to a fixed 8, so a
        /// mod adding an eighth tier still works.
        /// </summary>
        [HarmonyPatch(typeof(QualityInfo), nameof(QualityInfo.Add))]
        [HarmonyPrefix]
        public static void Add_Prefix(int _key)
        {
            if (_key < QualityInfo.qualityColors.Length) return;
            Array.Resize(ref QualityInfo.qualityColors, _key + 1);
            Array.Resize(ref QualityInfo.hexColors, _key + 1);
        }

        // There is no patch for QualityInfo.GetQualityLevelName, which would be the place to name
        // tier 7, because nothing calls it: no method in any of the game's managed assemblies, and
        // no XUi binding either - the lblQuality* keys it reads appear nowhere but Localization.csv.
        // Quality is shown as a number in the tier colour, which is why qualityinfo.xml and the
        // array growth above are what actually matter.

        /// <summary>
        /// With crafting progression off, Recipe.GetCraftingTier returns a literal 6 for every
        /// recipe and the magazine ladder in progression.xml never gets a say. Follow the cap
        /// instead - but only for recipes whose output can actually be quality 7. Vanilla's flat 6
        /// covers everything craftable, the workstation tools and the car battery included, and
        /// those still stop at 6.
        /// </summary>
        [HarmonyPatch(typeof(Recipe), nameof(Recipe.GetCraftingTier))]
        [HarmonyPostfix]
        public static void GetCraftingTier_Postfix(Recipe __instance, ref int __result)
        {
            if (XUiM_Recipes.CraftingProgression) return;
            if (__result >= ItemClass.MaxQualityTier) return;
            if (__instance == null || !T7.ReachesTier7(__instance.itemValueType)) return;
            __result = ItemClass.MaxQualityTier;
        }
    }
}
