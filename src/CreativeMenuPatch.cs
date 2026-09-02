using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Tier7Expansion
{
    /// <summary>
    /// Lets the creative menu reach tier 7.
    ///
    /// ItemClass.CreateItemStacks builds every creative-menu entry as
    /// <c>new ItemValue(id, minQuality, maxQuality, ...)</c> with the range hard-coded to 1..6, which
    /// is why the menu hands out a random quality and never goes past 6. The same 6 also bounds the
    /// <c>#N</c> search filter, so typing <c>#7</c> is rejected and falls back to the full range.
    ///
    /// Both are the literal from Constants.cItemMaxQuality, inlined by the compiler, so there is no
    /// field to raise - only the two ldc.i4.6 in this one method, swapped for the live cap.
    /// </summary>
    [HarmonyPatch(typeof(ItemClass), nameof(ItemClass.CreateItemStacks))]
    public static class CreativeMenuPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var literals = new List<int>();
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_I4_6) literals.Add(i);
            }

            // Two: the max of the default range, and the upper bound of the #N filter check. A
            // different count means the method changed shape - or another mod got here first - and
            // then a literal 6 in it is no longer known to be a quality, so leave the method alone
            // rather than rewriting something else.
            if (literals.Count != 2)
            {
                Log.Warning($"[Tier7Expansion] CreateItemStacks: expected 2 quality literals, found {literals.Count}"
                            + " - left unpatched, the creative menu will not offer tier 7");
                return code;
            }

            // Rewritten in place rather than replaced: a fresh CodeInstruction would carry the
            // labels over but drop the exception block markers, and losing those on an instruction
            // that happens to open a try block is invalid IL.
            var maxQualityTier = AccessTools.Field(typeof(ItemClass), nameof(ItemClass.MaxQualityTier));
            foreach (int i in literals)
            {
                code[i].opcode = OpCodes.Ldsfld;
                code[i].operand = maxQualityTier;
            }
            return code;
        }
    }
}
