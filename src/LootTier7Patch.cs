using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Tier7Expansion
{
    /// <summary>
    /// Lets tier 7 drop as loot, five times less often than tier 6.
    ///
    /// The obvious route - adding a quality 7 band to the loot quality templates - does not work
    /// here, because a template is picked per lootgroup and the vanilla groups mix categories
    /// freely: <c>groupToolsT2</c> and <c>groupToolsT3</c> share <c>QLTemplateT2</c> and
    /// <c>QLTemplateT3</c> with the weapon groups, and those groups also carry the anvil, the
    /// bellows and the cooking pots. A quality 7 anvil would come out with none of its tier-scaled
    /// effects, since its ladders stop at 6.
    ///
    /// So the promotion is gated per item instead: a drop that vanilla decided would be quality 6
    /// becomes quality 7 one time in six, and only for an item whose ladders actually reach 7. One
    /// in six is what makes tier 7 five times rarer than tier 6 while leaving the overall drop rate
    /// exactly as it was.
    ///
    /// It runs on the freshly built ItemValue, before SpawnItem calls AddGSStats, so a looted tier 7
    /// rolls its stats the same way any other looted item does - including the starred band. Crafted
    /// tier 7 stays plain, exactly like every other crafted quality.
    /// </summary>
    [HarmonyPatch(typeof(LootContainer), nameof(LootContainer.SpawnItem))]
    public static class LootTier7Patch
    {
        /// <summary>
        /// SpawnItem builds the drop with <c>new ItemValue(type, minQuality, maxQuality, ...)</c> in
        /// two places. A call goes in after each: ItemValue and the method's own GameRandom in,
        /// ItemValue out, so the stack is unchanged.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo hook = AccessTools.Method(typeof(LootTier7Patch), nameof(MaybePromote));
            int injected = 0;

            foreach (CodeInstruction ins in instructions)
            {
                yield return ins;
                if (ins.opcode == OpCodes.Newobj
                    && ins.operand is ConstructorInfo c
                    && c.DeclaringType == typeof(ItemValue)
                    && c.GetParameters().Length == 6)
                {
                    injected++;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);   // GameRandom random
                    yield return new CodeInstruction(OpCodes.Call, hook);
                }
            }

            if (injected == 2) Log.Out("[Tier7Expansion] loot hook installed in SpawnItem");
            else
            {
                Log.Warning($"[Tier7Expansion] SpawnItem: expected 2 ItemValue constructions, hooked {injected}"
                            + " - tier 7 may not appear in loot");
            }
        }

        public static ItemValue MaybePromote(ItemValue itemValue, GameRandom random)
        {
            if (itemValue == null || itemValue.Quality != T7.Tier - 1) return itemValue;
            if (!T7.ReachesTier7(itemValue)) return itemValue;
            if (random == null) return itemValue;

            // one in six of what would have been tier 6, which is tier 6 five times over
            if (random.RandomRange(T7.LootRarityVsTier6 + 1) != 0) return itemValue;

            itemValue.Quality = T7.Tier;

            // ctor sized the mod slots for quality 6; grow only, it may already hold rolled mods
            int slots = itemValue.CalcModSlotCount();
            ref ItemValue[] mods = ref modifications(itemValue);
            if (mods == null) mods = new ItemValue[slots];
            else if (mods.Length < slots) Array.Resize(ref mods, slots);

            return itemValue;
        }

        // 3.3 made the array private and exposes no way to resize it
        static readonly AccessTools.FieldRef<ItemValue, ItemValue[]> modifications =
            AccessTools.FieldRefAccess<ItemValue, ItemValue[]>("modifications");
    }
}
