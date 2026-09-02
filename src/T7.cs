using System.Collections.Generic;

namespace Tier7Expansion
{
    /// <summary>
    /// Shared constants and the "can this item legitimately be quality 7" test.
    ///
    /// Only the items the mod extended have tier ladders that reach 7 - the 43 weapons, the 16
    /// tools, the 66 armour pieces, the 12 clothing pieces and the 35 item mods. What is left still
    /// stops at 6: the workstation tools, car batteries, solar cells. A quality 7 one of those would
    /// lose every tier-scaled effect it has, mod slots included, because PassiveEffect.ModValue
    /// applies nothing outside its level range. So anything that can hand out quality 7 has to be
    /// gated per item.
    ///
    /// The gate asks the item itself rather than reading a tag off it: the ladders reaching 7 *is*
    /// the precondition, an item mod cannot carry a tag of its own anyway - they all inherit Tags
    /// from modGeneralMaster - and this way a config change is all it takes to bring another item
    /// in, ours or another mod's.
    /// </summary>
    public static class T7
    {
        public const int Tier = 7;

        /// <summary>Looted tier 7 is this much rarer than looted tier 6.</summary>
        public const int LootRarityVsTier6 = 5;

        static readonly Dictionary<int, bool> cache = new Dictionary<int, bool>();

        public static bool ReachesTier7(ItemValue itemValue)
        {
            return itemValue != null && !itemValue.IsEmpty() && ReachesTier7(itemValue.type);
        }

        public static bool ReachesTier7(int itemId)
        {
            lock (cache)
            {
                if (cache.TryGetValue(itemId, out bool known)) return known;
                ItemClass itemClass = ItemClass.GetForId(itemId);
                bool reaches = itemClass != null && LaddersReachTier7(itemClass);
                cache[itemId] = reaches;
                return reaches;
            }
        }

        /// <summary>
        /// Item ids are assigned per world - a save carrying a NameIdMapping keeps its own numbering,
        /// and the next world will hand the same number to something else - so answers cached
        /// against one world's ids are wrong for the next. Called from the ItemClass.AssignIds
        /// postfix, which is exactly the moment the numbering changes.
        /// </summary>
        public static void InvalidateCache()
        {
            lock (cache) { cache.Clear(); }
        }

        /// <summary>
        /// Two ways an item can carry a seventh tier.
        ///
        /// The usual one is a ladder: an owner-tiered effect group whose tier= list is parsed into
        /// PassiveEffect.Levels. Untiered effects in the same group - the nailgun's ModSlots of
        /// zero, for one - have no levels and simply do not answer.
        ///
        /// The other is one node per tier, each gated on its exact number, which is how vanilla
        /// writes an effect that triggers rather than scales - the serrated blade's bleed chance and
        /// seven mods like it have no ladder at all. Those are found by the tier the requirement
        /// asks for. ArmorGroupLowestQuality is deliberately not among them: it reads the quality of
        /// a whole armour set, so it says nothing about the item holding it - that is how the .44
        /// rounds carrying the Enforcer bonus stay out of this.
        /// </summary>
        static bool LaddersReachTier7(ItemClass itemClass)
        {
            MinEffectController effects = itemClass.Effects;
            if (effects?.EffectGroups == null) return false;

            foreach (MinEffectGroup group in effects.EffectGroups)
            {
                if (group == null) continue;

                if (group.OwnerTiered && group.PassiveEffects != null)
                {
                    foreach (PassiveEffect effect in group.PassiveEffects)
                    {
                        float[] levels = effect?.Levels;
                        if (levels != null && levels.Length > 0 && levels[levels.Length - 1] >= Tier) return true;
                    }
                }

                if (AsksForTier7(group.Requirements)) return true;
                if (group.PassiveEffects != null)
                {
                    foreach (PassiveEffect effect in group.PassiveEffects)
                    {
                        if (effect != null && AsksForTier7(effect.Requirements)) return true;
                    }
                }
            }
            return false;
        }

        static bool AsksForTier7(RequirementGroup requirements)
        {
            if (requirements == null) return false;

            if (requirements.reqs != null)
            {
                foreach (RequirementBase req in requirements.reqs)
                {
                    // An inverted requirement means the opposite, so it proves nothing. The
                    // operation is deliberately not tested: OperationTypes stores every spelling of
                    // equality as its own value - Equals, EQ and E are 1, 2 and 3 - so testing for
                    // one would miss a config that wrote another, and vanilla only ever uses Equals
                    // on these two anyway.
                    if (req == null || req.invert || req.value < Tier) continue;
                    if (req is RequirementItemTier || req is RequirementItemModTier) return true;
                }
            }
            if (requirements.groups != null)
            {
                foreach (RequirementGroup nested in requirements.groups)
                {
                    if (AsksForTier7(nested)) return true;
                }
            }
            return false;
        }
    }
}
