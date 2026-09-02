# Tier 7 Expansion

A modlet for **7 Days to Die V 3.2.0** that adds a seventh quality tier to everything in the game
that has a quality.

Once a crafting skill is capped, the game stops giving you anything: quality 6 is the ceiling and
every drop from then on is a repeat. Tier 7 is one more step past it - a late-game goal to work
towards, not a new power level.

## What it covers

| | |
|---|---|
| Weapons | 43 - melee and ranged, junk sledge, turret and drone included |
| Tools | 16 - axes, picks, shovels, wrenches, claw hammer, nailgun, chainsaw, auger |
| Armour | 66 - every piece that has a quality |
| Clothing | 12 - the layer under the armour |
| Item mods | 35 - every mod that has a quality at all |

## What tier 7 gives

- **Every tier-scaled number continues** - damage, block damage, durability, damage falloff,
  physical and elemental resists, stamina, harvest counts, whatever that item scales.
- **One more mod slot** than tier 6.
- **Armour set bonuses continue.** A full tier 7 set gets the next step of its set bonus.
- **Item mod effects continue** - bleed and stun chances, magazine size, fuel capacity, the lot.
- **Tooltips read correctly**, so the number you see is the number you get.
- **A gold quality band**, one step past legendary purple.
- **Crafting skills go to 60, 90 and 120** instead of 50, 75 and 100, so the magazines you find
  after capping out have somewhere to go.

## Balance

Tier 7 is not a new power level - it is the same size step tier 6 was.

Every ladder gains one more rung of exactly the width vanilla was already using. An AK47 gains
+40 durability going to tier 7, the same +40 a tier that carried it from 200 at tier 1 to 400 at
tier 6. A Raider outfit gains +0.6 resist, the same +0.6 a tier it had all along. So the gap from
6 to 7 feels like the gap from 5 to 6, and nothing below tier 7 moves at all.

What makes it endgame is the cost, not the numbers:

- The crafting skill has to sit at its new cap - a fifth more magazines than tier 6 needed.
- A craft takes **six Legendary Parts** where tier 6 takes one.
- A drop is **five times rarer** than tier 6, and no trader ever stocks it.

## Where it comes from

Craft it at the new cap, or find it in loot. A looted one rolls its stats like any looted item,
starred band included; a crafted one is plain, exactly like every other crafted quality.

Item mods are loot only - vanilla gives mod recipes no crafting tier at all, so a crafted mod is
quality 1 whatever your skill.

Twenty items are treated as junk - pipe guns, primitive weapons, stone tools, primitive armour and
the few things with no recipe at all. They get tier 7 free and gain no extra mod slot, because
vanilla does not charge a Legendary Part for them at tier 6 either.

## Requirements

- 7 Days to Die **V 3.2.0**. Built and tested against b9.
- Launch **without EasyAntiCheat** - the mod ships a DLL, and EAC blocks those.
- No other mods needed.

## Installation

1. Extract the archive into `<game folder>\Mods\` so you end up with
   `<game folder>\Mods\Tier7Expansion\ModInfo.xml`.
2. Start the game without EAC.

Works on an existing save. To uninstall, delete the folder - but a quality 7 item you are holding
keeps that quality with no tier 7 defined for it any more, so it loses the bonuses and the slot the
tier gave it until the mod is back.

## Multiplayer

Not client side: the mod changes item definitions, and both ends read those from their own files, so
the server and every client need the same version. Only tested in single player so far.

## Compatibility

Every config change is an XPath patch pinned to the exact vanilla value it expects, so if another
mod already rewrote a weapon's ladder, the patch finds nothing and their numbers survive. The
quality cap is raised with a maximum rather than a fixed value, so load order cannot make this mod
stomp one that wants tier 8.

Overhauls that rebalance weapons and armour wholesale - Darkness Falls, Undead Legacy - replace the
ladders this mod extends, so expect most of it not to apply there.

## Building

Needs the .NET SDK 8.0 or newer. Adjust `GameDir` in `Directory.Build.props` if your game lives
elsewhere.

```powershell
.\build.ps1
```

The config is generated from the vanilla XML rather than hand written, and verified against it:

```powershell
..\..\tools\gen-t7-config.ps1
..\..\tools\verify-t7-patches.ps1
```

`verify-t7-patches.ps1` replays every generated patch the way the game's own patcher would and
reports any that match nothing - which is how a wrong predicate would otherwise fail: silently,
with the tier simply never appearing.

[NOTES.md](NOTES.md) explains why the patches look the way they do.

## License

MIT - see [LICENSE](LICENSE).
