# Changelog

## 0.1.0 - unreleased

- Quality tier 7 for all 43 melee and ranged weapons, junk sledge, turret and drone included:
  one extra mod slot, stats continuing the vanilla per-tier step.
- Quality tier 7 for all 16 tools as well - axes, picks, shovels, wrenches, the claw hammer, the
  nailgun, the chainsaw and the auger - on the same terms.
- Quality tier 7 for all 66 armour pieces - sixteen sets of four, plus the Santa hat and the demo
  lumberjack helmet. Armour has no `<stats>` roll tables in vanilla, so there is no starred band to
  mirror; it gains the extra mod slot and the continued per-tier step.
- Quality tier 7 for the 12 clothing pieces under the armour. Clothing is loot only and takes no
  mods, so tier 7 there is one more step of insulation and nothing else.
- Quality tier 7 for all 35 item mods that have a quality at all. Mods are never crafted above
  quality 1 in vanilla, so for them tier 7 is purely a loot drop.
- Crafting a tier 7 costs six Legendary Parts, where tier 6 asks for one. No new component - a
  dedicated one belongs to the mod that adds new weapon tiers.
- Tier 7 drops as loot, five times less often than tier 6, gated per item so the workstation tools
  and parts sharing those lootgroups stay at 6.
- All sixteen crafting skills extended: 50 -> 60, 75 -> 90 and 100 -> 120, with tier 7 on the new
  cap, `craftingArmor` among them. Skill magazines need no change - each adds one level and clamps
  to the cap - but the dev-only Admin Magazine, which grants each cap as a flat number, was raised
  to follow.
- The seventeen junk items - the seven primitive weapons, the four pipe guns, the two stone tools
  and the four primitive armour pieces - get tier 7 free, and no extra mod slot with it. The same
  test catches the fifteen items with no recipe at all, where there is no cost to waive anyway.
- All sixteen crafting skills explain tier 7 and their new cap in the skills window.
- Effects gated on an exact tier rather than scaled by it - the armour set bonuses in `buffs.xml`,
  the .44 Magnum rounds carrying the Enforcer bonus, the eight item mods with no ladder at all, the
  boots' fall damage - get a seventh gated node too, 38 in all. Without it those effects vanish at
  quality 7 instead of continuing, a full tier 7 armour set losing its set bonus among them. Three
  that cannot be derived repeat the tier 6 values instead of guessing.
- Quality 7 reads as a gold number rather than a word: `QualityInfo.GetQualityLevelName` has no
  callers left in V3.20, in any managed assembly or XUi binding, so the mod ships no name for it and
  no patch pretending to supply one.
- Fixed every tooltip on every tier 7 item reading 0%: `EffectDisplayValue.GetValue` walks its
  levels two at a time, as disjoint ranges rather than a ladder, so the seventh level appended to
  `display_value` was never paired and nothing covered tier 7. The last range is stretched to end at
  7 instead, which leaves tiers 1 to 6 reading exactly what they did before. The effects themselves
  were always correct - `PassiveEffect.ModValue` slides one at a time.
- Fixed an exception storm on quality 7 armour: `MinEventActionModifyCVar` reads its per-tier value
  list as `valueList[Quality - 1]` without an upper bound check, so the four vanilla lists - on the
  Preacher boots and the Enforcer, Commando and Raider outfits - threw every tick once worn at tier
  7. The lists are extended, and a prefix pads any that is still short rather than letting it throw.
- The creative menu can reach tier 7: its quality range and its `#N` search filter were both
  capped at 6 by an inlined literal.
