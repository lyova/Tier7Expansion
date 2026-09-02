# Implementation notes

Why the generated patches look the way they do. The player-facing description is in
[README.md](README.md); this is the record of what the game actually does, so that the next
person to touch `tools\gen-t7-config.ps1` does not have to rediscover it.

## Getting one

Tier 7 needs two things: the crafting skill for that family at its cap, and six Legendary Parts -
the same component tier 6 wants one of. There is no new crafting component: a dedicated one belongs
to the mod that adds new weapon tiers, since here tier 7 is another quality of something that
already exists.

All sixteen crafting skills go to 60, 90 and 120 instead of 50, 75 and 100 - a fifth of the ladder
added on top. Tier 7 sits on the new cap and the whole vanilla ladder below it is untouched, so
nothing you have already unlocked moves. The skill magazines need no change of their own: each adds
one level and `AddProgressionLevel` clamps to the cap, so the only effect is that the last stretch
takes more of them. The dev-only Admin Magazine is the exception, because it grants each cap written out as a
flat number, and it was raised to follow.

Crafting tier 7 relies on **Crafting Max Quality** being left on *Default* in Sandbox Options.
Setting it to an explicit 6 caps crafting at 6, and the tier is then only reachable from loot.

## Junk items

Seventeen items cost nothing at tier 7 and gain no extra mod slot: the seven primitive weapons, the
four pipe guns, the stone axe and stone shovel, and the four primitive armour pieces. Vanilla does
not charge a Legendary Part for any of them at tier 6 either, and that is the signal the mod goes
by - spending a rare component on a pipe pistol would be a trap.

The same test catches everything with no recipe at all - Tazas' axe, the two event helmets, the
whole clothing layer - since there is no cost to waive there either. For clothing it makes no
difference at all, because clothing has no mod slots to withhold.

The mod slot is the part that matters. A junk item sits a slot or two behind the real thing at tier
6 - three against four for the weapons, two against five for the armour - so handing it the extra
slot for free would put a pipe pistol on the same footing as a legendary Desert Vulture. It keeps
the count it had. The percentage steps do carry over - the same +5% damage and +10% durability
everything else gets - because those are proportional and these items have small bases to apply
them to.

The nailgun and the demo lumberjack helmet are separate cases: their mod slots are pinned at zero
with no tier ladder at all, because they take no mods, and that stays true at tier 7.

## Armour

Armour is the one category with no `<stats>` roll tables in vanilla, so there is nothing to mirror
onto quality 7 and a looted tier 7 piece has no starred band - exactly as a looted tier 6 one has
none. What it does get is the extra mod slot, the continued per-tier step on every resist and
effect, and the extended `display_value` rows behind the tooltip numbers.

The four primitive pieces are junk by the same rule the primitive weapons are, and the Santa hat
and the demo lumberjack helmet have no recipe at all - they are event drops - so they get the free
tier 7 and nothing else.

## Clothing

The layer under the armour is loot only - vanilla ships no recipe for any of the twelve pieces - and
its `ModSlots` is pinned at zero with no ladder at all, because clothing takes no mods. So tier 7
there is exactly one more step of insulation on each piece, continuing the vanilla step: the tier 3
chest goes 5.8 at six to 6.2 at seven, the tier 1 hood 2.5 to 2.8.

## Item mods

Mod recipes carry no `CraftingTier` tags anywhere, so `Recipe.GetCraftingTier` returns 1 for them
and a crafted mod is quality 1 whatever the player's skill - that is vanilla, not something this
mod changes. Mod quality comes from loot, and so does tier 7 for them: the ladders are extended and
the loot promotion does the rest, with no recipe or progression work involved.

Mods also carry a second copy of each ladder in their `display_value` rows, 43 of them, and armour
carries 59 more. Those are what the tooltip prints, and they are extended by a different rule from
the effects beside them.

`PassiveEffect.ModValue` slides along its levels one at a time, so the real effect only needs a
seventh rung. `EffectDisplayValue.GetValue` walks in steps of **two**: `1,2,3,4,5,6` is not one
ladder but three ranges - (1,2), (3,4), (5,6) - and a tier outside all of them returns zero. Append
a seventh level there and the list turns odd, the last one is never paired, and every tooltip on
every tier 7 item reads 0%. So a display value keeps its six entries and has its last range
stretched instead: `1,2,3,4,5,7` with the last value replaced by the tier 7 one. Tier 6 still lands
on exactly its old number, because the tier 7 value is one more step and that puts tier 6 precisely
halfway along the stretched range.

## Effects that are gated on the tier instead of scaled by it

Not every tiered effect is a ladder. Where an effect triggers rather than scales, vanilla writes six
sibling nodes, each gated on its exact tier: `RequirementItemTier` for the nerd boots' fall damage,
`RequirementItemModTier` for the serrated blade's bleed chance, `ArmorGroupLowestQuality` for the
sixteen armour set bonuses. A quality 7 item matches none of them, so the effect does not weaken -
it disappears. A full tier 7 Enforcer set would have lost its set bonus entirely, which is the
opposite of the point.

So each of those gets a seventh sibling too, cloned from the sixth with every attribute that steps
across the six taken one step further and every attribute that stays put left alone - 38 ladders in
all. Three cases cannot be derived and plateau instead, repeating the tier 6 values at tier 7 rather
than inventing numbers: the farmer outfit and the preacher set bonus, whose top rung carries an
extra effect so the rungs do not line up attribute for attribute, and the rad remover, whose step is
a buff *name* (`buffRadiatedRegenBlock15` through `90`) with no 105 to point at.

One kind of per-tier value is not a node at all but a list: `MinEventActionModifyCVar` takes
`value=".05,.1,.15,.2,.25,.4"` and reads it as `valueList[Quality - 1]`, checking that the index is
not negative and never that it fits. Four armour outfits carry such a list, and at quality 7 each of
them threw `IndexOutOfRangeException` - every tick, because `Equipment.Update` fires these
continuously, which is not something you can play through. All four lists are extended, and a prefix
on `Execute` pads any list that is still too short by repeating its last entry, so another mod's
armour degrades to a plateau instead of taking the game down.

One of them is not a ladder but a gate - the preacher full-set bonus applies only while every piece
is at the top quality - and that one becomes `GTE` rather than gaining a copy.

These live wherever the effect lives, which is not always on the item whose quality is being read:
the Enforcer bonus sits on the three .44 Magnum rounds, and the other fifteen sets sit in
`buffs.xml`. Hence a generated `buffs.xml` and three patched ammo items that are not themselves
tiered.

## Tier 7 in loot

Tier 7 also drops, five times less often than tier 6, and a looted one rolls its stats like any
other looted item - starred band included. A crafted one is plain, exactly as every other crafted
quality in the game is.

The obvious route, a quality 7 band in the loot quality templates, does not work: a template is
picked per lootgroup, and the vanilla groups mix categories freely. `groupToolsT2` and
`groupToolsT3` share `QLTemplateT2` and `QLTemplateT3` with the weapon groups, and those groups also
carry the anvil, the bellows and the cooking pots, so a quality 7 anvil would come out with no
tier-scaled effects at all, because its ladders stop at 6.

So the promotion is gated per item: a transpiler on `LootContainer.SpawnItem` catches the freshly
built drop and, one time in six, turns a quality 6 into a quality 7 - but only for an item that
actually supports 7. The gate asks the item itself: an owner-tiered effect group whose
`PassiveEffect.Levels` end at 7 or above, or a `RequirementItemTier`/`RequirementItemModTier` asking
for 7, which is how the mods with no ladder qualify. The answer is cached per item id and thrown
away when `ItemClass.AssignIds` runs, because item ids are assigned per world.

That is the real precondition rather than a proxy for it, an item mod could not carry a tag of its
own anyway - they all inherit `Tags` from `modGeneralMaster` - and it means another mod extending
its own ladders to 7 gets the promotion for free. One in six is what makes tier 7 five times rarer
than tier 6 while leaving the overall drop rate untouched. It runs before `SpawnItem` calls
`AddGSStats`, which is why the rolls come out for the new quality.

The promoted item also has its `Modifications` array grown. The constructor sizes it from
`CalcModSlotCount()` while the quality is still 6, so without that a looted tier 7 would show one
mod slot fewer than a crafted one.

## The creative menu

`ItemClass.CreateItemStacks` builds every creative-menu entry with the quality range hard-coded to
1..6 - which is why the menu hands out a random quality - and the same 6 bounds the `#N` search
filter, so `#7` was rejected. Both are the inlined `Constants.cItemMaxQuality`, with no field to
raise, so a transpiler swaps the two literals for the live cap. Type `#7` in the creative menu
search box to get tier 7 of everything.

`giveself <item> 7` needs no patch - that command never clamped, and it skips the stat rolls, which
makes it the clean way to compare two tiers without the roll in the way.

## Skill descriptions

Each of the sixteen crafting skills gets a line appended to its description in the skills window,
naming the level tier 7 unlocks at and what it costs. The vanilla localisation keys are not
overridden: a mod row replaces the whole vanilla row, so unless we restated their text in every
language, players on those languages would drop from their own translation to English. The line is
appended to the `groupdescription` binding instead, which leaves all twelve translations intact and
follows the vanilla text wherever The Fun Pimps take it.

## Compatibility

The mod is built to lose gracefully rather than win arguments:

- The tier cap is raised with `Math.Max` from code, not an XML `setattribute`, so load order
  cannot make this mod stomp a mod that wants tier 8.
- Every XML patch is pinned by a predicate on the exact vanilla value it expects. If another mod
  already rewrote an item's `ModSlots` or damage ladder, the predicate misses and that item keeps
  their numbers instead of ours.
- The loot promotion reads the item's own ladders, so it follows whatever the config ends up
  saying rather than a list compiled into the DLL.
- The quality colour array is grown to fit whatever key is being added, not to a hard-coded 8.
- The creative-menu transpiler counts the quality literals before touching any of them, and leaves
  the method alone if there are not exactly two - past that point a literal 6 in it is no longer
  known to be a quality.
- With Crafting Progression switched off, vanilla hands every recipe a flat tier 6; the mod only
  lifts that to 7 for recipes whose output supports 7, so the workstation tools and the car battery
  keep the six they can actually use.

## Languages

The mod adds exactly one string of its own, the tier 7 line in the skills window, in English plus
eleven translations and no Russian. `Localization.getLanguageEntry` treats an empty column as
missing and `Get` then falls back to the default language, so a blank cell shows the English text
rather than nothing - which is what a Russian client will get.

There is deliberately no name for quality 7. `QualityInfo.GetQualityLevelName` would be the place,
and it does leave anything above 6 as an empty string - but nothing calls it: not one method in any
of the game's managed assemblies, and no XUi binding either, with the `lblQuality*` keys it reads
appearing nowhere but `Localization.csv`. Quality reaches the player as a number in the tier colour,
so the gold from `qualityinfo.xml` is what carries it.

