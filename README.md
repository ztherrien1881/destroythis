# Destroy This

RimWorld 1.6 quality-of-life mod for designating individual items to be destroyed or smelted at an existing electric smelter, without maintaining a bill.

## Intended behavior

- Adds **Destroy this** to the existing **Orders** Architect category; it does not create its own category.
- Adds a **Destroy this** command directly to selected eligible items.
- An already-designated item shows **Cancel destroy this** so the order can be removed from the selected item itself.
- Accepts only loose map items accepted by a smelting or destruction recipe installed on `ElectricSmelter`.
- Reads the smelter's live recipe filters, so modded items patched into those recipes work automatically.
- Blocks quest-tagged items and Ideology relics.
- Uses the Smithing work type and requires a powered, reachable electric smelter.
- Prefers material-producing smelting recipes over pure destruction recipes.
- Produces recipe outputs through `GenRecipe.MakeRecipeProducts`, so smeltable items return the same recipe-defined materials instead of simply vanishing.
- Adds no workbench. Harmony is required for the selected-item command.

## Building

On Windows, double-click `Build-Mod.bat` and follow the prompt. It automatically locates common Steam installations and uses the C# compiler included with .NET Framework.

Alternatively, build `Source/DestroyThis/DestroyThis.csproj` with `RimWorldManagedDir` set to RimWorld's `RimWorldWin64_Data/Managed` directory and `HarmonyDll` set to Harmony's `0Harmony.dll`:

```powershell
dotnet build -c Release -p:RimWorldManagedDir="C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed" -p:HarmonyDll="C:\path\to\0Harmony.dll"
```

The compiled DLL is written to `1.6/Assemblies/DestroyThis.dll`.

## Test checklist for this revision

1. Start RimWorld with Core, Harmony, and Destroy This.
2. Open **Architect -> Orders** and confirm **Destroy this** appears there with no separate Destroy This category.
3. Select an eligible loose weapon/apparel item and confirm **Destroy this** appears in its selected-item commands.
4. Click the selected-item command and confirm it changes to **Cancel destroy this**.
5. Confirm a smith carries the exact designated item to a powered electric smelter.
6. Smelt a recyclable weapon/apparel item and confirm the expected recipe materials are spawned near the smelter.
7. Destroy a non-recyclable but destructible item and confirm it produces no materials, matching its normal destruction recipe.
8. Confirm quest items and Ideology relics cannot be designated.
9. Confirm a modded item accepted by the electric smelter can be designated.
10. Save during hauling/smelting, reload, and confirm the job resolves safely.
