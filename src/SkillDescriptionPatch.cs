using System.Collections.Generic;
using HarmonyLib;

namespace Tier7Expansion
{
    /// <summary>
    /// Appends a line about tier 7 to the description of every crafting skill whose ladder
    /// the mod extended.
    ///
    /// Overriding the vanilla localisation keys instead would mean restating their text in all
    /// thirteen languages, and any player on a language we did not fill in would get a blank
    /// description. Appending to the binding leaves the vanilla text alone, follows it when The
    /// Fun Pimps reword it, and only runs while the skills window is actually open.
    /// </summary>
    [HarmonyPatch(typeof(XUiC_SkillCraftingInfoWindow), nameof(XUiC_SkillCraftingInfoWindow.GetBindingValueInternal))]
    public static class SkillDescriptionPatch
    {
        /// <summary>
        /// Must match $CraftingSkills in tools\gen-t7-config.ps1 - verify-t7-patches.ps1 checks that
        /// the two lists have not drifted apart.
        /// </summary>
        public static readonly HashSet<string> CraftingSkills = new HashSet<string>
        {
            "craftingKnuckles", "craftingBlades", "craftingClubs", "craftingSledgehammers",
            "craftingBows", "craftingSpears", "craftingHandguns", "craftingShotguns",
            "craftingRifles", "craftingMachineGuns", "craftingExplosives", "craftingRobotics",
            "craftingHarvestingTools", "craftingRepairTools", "craftingSalvageTools",
            "craftingArmor"
        };

        public static void Postfix(XUiC_SkillCraftingInfoWindow __instance, ref string _value, string _bindingName, bool __result)
        {
            if (!__result || _bindingName != "groupdescription") return;

            ProgressionValue skill = __instance.CurrentSkill;
            ProgressionClass cls = skill?.ProgressionClass;
            if (cls == null || !CraftingSkills.Contains(cls.Name)) return;

            string note = Localization.Get("t7SkillDescNote");
            if (string.IsNullOrEmpty(note) || note == "t7SkillDescNote") return;
            _value += "\n" + string.Format(note, cls.MaxLevel);
        }
    }
}
