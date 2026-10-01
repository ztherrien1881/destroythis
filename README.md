# Minisplits

RimWorld 1.6 source prototype by Zach. Adds exactly two temperature buildings:
small and large wall-mounted electric minisplits. No DLC or Harmony dependency.

**Status: uncompiled, not yet tested in RimWorld.** This branch is a standalone
Minisplits mod. It is temporarily hosted in the `destroythis` repository because
the connected GitHub tools cannot create repositories. The default branch is
unmodified; this branch can later be copied to a dedicated Minisplits repository.

## Design

| | Small | Large |
|---|---:|---:|
| Wall footprint | 1 x 1 | 2 x 1 |
| Active power | 350 W | 700 W |
| Standby power | 35 W | 70 W |
| Cooling energy / second | 21 | 42 |
| Heating energy / second | 21 | 42 |
| Steel | 100 | 180 |
| Components | 4 | 8 |

Numbers are initial balance proposals, not guaranteed room sizes. Room size,
insulation, ambient temperature, open doors, and exhaust temperature affect output.
Air conditioning research and Construction 4 unlock both buildings.

- Installs over completed walls without replacing them. The existing wall still
  provides insulation, strength, and roof support. Supports constructed walls
  marked place-overable by the game and smoothed rock walls; not doors or raw rock.
- Blue placement outline: conditioned room. Red: cooling exhaust. Rotate before
  placing to put the blue side indoors. Large units need two adjacent wall tiles
  with both indoor ports in one room and both exhaust ports in one room/area.
- One vanilla temperature setting defaults to 21 C. Automatic heat/cool selection
  uses a +/- 0.5 C deadband, checked every 250 ticks. Target temperature is saved
  by vanilla CompTempControl; operating mode is recalculated after loading.
- Vanilla cooler efficiency curve and 1.25x heat exhaust model. No cooling through
  blocked ports and no exhaust deleted into a wall. Heating follows the vanilla
  heater efficiency falloff. Heating does not extract outdoor heat in this draft.
- Normal flick switch, electrical power loss and breakdown behavior.
- A removed supporting wall disables the unit. The draft leaves the unsupported
  unit in place so it can be deconstructed or its wall restored.
- Original vector placeholder art is included alongside the game PNG textures.

## Build

Install the .NET SDK (8 or newer). Supply your own legally installed RimWorld 1.6
Managed folder. No game assemblies are committed. The project restores the .NET
Framework reference pack from NuGet for cross-platform compilation.

Windows PowerShell example (adjust the Steam path):

```powershell
dotnet build Source/Minisplits/Minisplits.csproj -c Release "-p:RimWorldManagedDir=C:/Program Files (x86)/Steam/steamapps/common/RimWorld/RimWorldWin64_Data/Managed"
```

macOS example (adjust the path to your installation):

```sh
dotnet build Source/Minisplits/Minisplits.csproj -c Release '-p:RimWorldManagedDir=/path/to/RimWorldMac.app/Contents/Resources/Data/Managed'
```

If your macOS installation uses a different bundle layout, locate
`Assembly-CSharp.dll` and use its containing directory. Linux uses
`RimWorldLinux_Data/Managed` inside the game installation.

The build writes `1.6/Assemblies/Minisplits.dll`. Copy the whole mod directory
to `RimWorld/Mods/Minisplits`, enable **Minisplits**, and restart the game.
Downloading this source branch alone is not an install-ready release.

## Checks and first playtest

Run `python Tools/validate.py` for XML, texture, balance, class-name, and localization
checks. It does not compile C# or simulate RimWorld.

Before release, compile against RimWorld 1.6 and use a disposable dev-mode save:

1. Check the startup log for XML errors, missing types, or texture errors.
2. Construct both units on walls in all four rotations. Verify the walls remain
   in place through blueprints, frames, completed construction and deconstruction.
   Test constructed walls, smoothed rock, wall conduits, duplicate units, and doors.
3. Heat a cold sealed room and cool a hot sealed room at 21 C; watch power draw
   switch from active to standby. Change target and save/reload.
4. Exhaust into another sealed room and confirm its temperature rises. Let it
   overheat and compare cooling with a vanilla cooler under the same conditions.
5. Block either side, remove one supporting wall, cause a breakdown, flick the
   unit off, and disconnect power: conditioning must stop.
6. Verify the large unit has double capacity, not double output per exhaust port.
   Check ports on map edges and across room boundaries.
7. Confirm insulation/roof support still comes from the wall and the attachment
   does not interfere with rebuilding that wall or selecting the unit.

Full mod-list compatibility, including Replace Stuff and other wall-attachment
mods, remains untested. No combat behavior is changed, but CE compatibility has
not yet been verified in game.

## Implementation references

API and temperature behavior were checked against the public decompiled game
sources below. Custom implementation is in `Source/Minisplits`.

- https://github.com/Chillu1/RimWorldDecompiled/blob/master/RimWorld/Building_Cooler.cs
- https://github.com/Chillu1/RimWorldDecompiled/blob/master/RimWorld/Building_Heater.cs
- https://github.com/Chillu1/RimWorldDecompiled/blob/master/RimWorld/Building_TempControl.cs
- https://github.com/Chillu1/RimWorldDecompiled/blob/master/RimWorld/GenConstruct.cs
- https://github.com/Chillu1/RimWorldDecompiled/blob/master/Verse/GenSpawn.cs
