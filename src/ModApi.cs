using System.Reflection;
using HarmonyLib;

namespace Tier7Expansion
{
    public class Tier7ExpansionModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out("[Tier7Expansion] init");
            new Harmony("com.lyovi.tier7expansion").PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    /// <summary>
    /// The cap is raised from two places because neither one alone is enough.
    ///
    /// ItemClassesFromXml.CreateItems writes ItemClass.MaxQualityTier itself - from the
    /// max_quality_tier attribute on the items.xml root, or 6 when it is absent - so anything set
    /// before items load gets thrown away. The log makes the order plain: the sandbox options are
    /// applied at ~33s and items parse at ~49s.
    ///
    /// So: a postfix on ItemClass.AssignIds, which runs immediately after the parse, and a prefix
    /// on UpdateInGameValuesWithSandboxOptions, which is the only thing that reads the field.
    /// Both are Math.Max, so running twice or in either order is fine.
    /// </summary>
    [HarmonyPatch(typeof(ItemClass), nameof(ItemClass.AssignIds))]
    public static class MaxQualityTierAfterItemsPatch
    {
        public static void Postfix()
        {
            QualityPatches.RaiseMaxQualityTier();
            // ids have just been (re)assigned, so anything cached against the old ones is stale
            T7.InvalidateCache();
        }
    }

    [HarmonyPatch(typeof(SandboxOptions.SandboxOptionManager),
        nameof(SandboxOptions.SandboxOptionManager.UpdateInGameValuesWithSandboxOptions))]
    public static class MaxQualityTierPatch
    {
        public static void Prefix() => QualityPatches.RaiseMaxQualityTier();
    }
}
