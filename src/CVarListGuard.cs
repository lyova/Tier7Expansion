using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Tier7Expansion
{
    /// <summary>
    /// Keeps a short per-tier list from throwing on a quality the game did not expect.
    ///
    /// MinEventActionModifyCVar is the one action in the game that carries a value per tier of its
    /// own - <c>value=".05,.1,.15,.2,.25,.4"</c> - and it reads it as <c>valueList[Quality - 1]</c>.
    /// It checks that the index is not negative and never that it fits, so a quality 7 item with a
    /// six entry list throws IndexOutOfRangeException; and since Equipment.Update fires these every
    /// tick, it throws for as long as the item is worn. That is what a full tier 7 armour set did
    /// before the four vanilla lists were extended in items.xml.
    ///
    /// The data fix covers vanilla. This covers everything else: another mod's armour, a list this
    /// mod's generator could not read, a tier 8 mod on top. Padding repeats the last entry, so the
    /// top tier's value plateaus - a worse number than a derived one, but the alternative is an
    /// exception storm that has to be killed from outside the game.
    /// </summary>
    [HarmonyPatch(typeof(MinEventActionModifyCVar), nameof(MinEventActionModifyCVar.Execute))]
    public static class CVarListGuard
    {
        static readonly HashSet<MinEventActionModifyCVar> padded = new HashSet<MinEventActionModifyCVar>();

        public static void Prefix(MinEventActionModifyCVar __instance, MinEventParams _params)
        {
            float[] list = __instance?.valueList;
            if (list == null || list.Length == 0) return;

            ItemValue itemValue = _params?.ItemValue;
            if (itemValue == null || itemValue.IsEmpty()) return;

            int needed = itemValue.Quality;          // the index taken is Quality - 1
            if (needed <= list.Length) return;

            int had = list.Length;
            float last = list[had - 1];
            Array.Resize(ref list, needed);
            for (int i = had; i < needed; i++) list[i] = last;
            __instance.valueList = list;

            lock (padded)
            {
                if (padded.Add(__instance))
                {
                    Log.Warning($"[Tier7Expansion] ModifyCVar '{__instance.cvarName}' had {had} tiers of values"
                                + $" for a quality {needed} item - padded with the last one to stop it throwing every tick");
                }
            }
        }
    }
}
