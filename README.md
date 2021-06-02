# Pressure-Plate
## About
Adds a pressure plate, which opens and closes doors within a range if a player stands on it.
- Wooden pressure plate: 3 wood, 1 surtling core with the hammer. Requires a workbench in range
- Stone pressure plate: 3 stone, 1 surtling core with the hammer. Requires a stonecutter in range

Triggering range is configurable.

## Installation
Extract the content of `Pressure-Plate` into the `BepInEx/plugins` folder.


## Development
### Setup
Inside the repo are two folders, `PressurePlate` for the mod and `UnityAssets` for the unity files.

Place all Unity, Valheim and dependencies assemblies in the `PressurePlate/libs` folder (create the folder if it doesn't exist) and add them as references to the visual studio project.

Place the same assemblies in `UnityAssets/Assets/Assemblies` (create the folder if it doesn't exist) if you want to edit the AssetBundle.
Build the AssetBundle with the Unity toolbar "Assets/Build AssetBundles" to automatically copy the resulting file to the mod folder.

### Adding custom door config
Custom door settings can be applied for every door type. This can only be done with code and is an optional for other mods. Vanilla items are configurable, too.

Here is a quick instruction:
1. Add this mod .dll to your assembly references. The next step ensures that everything works if a user doesn't have pressure plate installed.
2. Check if the mod is loaded:
    ```
    const string pressurePlateGUID = "com.maxsch.valheim.pressure_plate";
    if (Chainloader.PluginInfos.ContainsKey(pressurePlateGUID)) {
        // next steps...
    }
    ```
    This should be done at your plugins `Start()`. If you use `Awake()` and your mod is loaded first it may not detect it properly. Any following steps can be done whenever you like, even after the loading phase.
3. Create the config: `DoorConfig config = new DoorConfig();`

    It takes two optional parameters:
    - bool openClosedInverted, default: false
    - float openTime, default: 1

    This may requires `using PressurePlate;`
4. Add the config to the gameobject:
    ```
    const prefabName = "your_door_piece_name"
    DoorConfig.AddDoorConfig(prefabName, config);
    ```
    or directly with your prefab
    ```
    GameObject door = myDoorPrefab;
    DoorConfig.AddDoorConfig(door.GetComponent<Door>(), config);
    ```

## Changelog
0.1.0
- Replaced ValheimLib with Jotunn, everything is backwards compatible

0.0.6
- Added configuration: PressurePlateRadiusHorizontal
- Added configuration: PressurePlateRadiusVertical
- Added configuration: PressurePlatePlayerRadiusHorizontal
- Added configuration: PressurePlatePlayerRadiusVertical
- Added configuration: PressurePlateOpenDelay

0.0.5
- Added stone pressure plate
- Updated wood pressure plate icon
- Changed Language tokens: $pressure_plate -> $pressure_plate_wood, added $pressure_plate_stone

0.0.4
- Updated ValheimLib to 0.0.15
- Reverted last fix as it was fixed in ValheimLib
- Removed unnecessary 'valheim.exe' restriction
- Removed HookGenPatcher dependency

0.0.3
- Fixed "Destroyed invalid prefab ZDO..."-bug when moving through portals

0.0.2
- Added sound effects for pressing and releasing pressure plate

0.0.1
- Release

## Links
Nexusmods: https://www.nexusmods.com/valheim/mods/498

Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/PressurePlate/

Github: https://github.com/MSchmoecker/Valheim-Pressure-Plate

## Credits
- Sound effects: http://www.freesfx.co.uk
