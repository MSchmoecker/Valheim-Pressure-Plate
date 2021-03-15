# Pressure-Plate
## About
Adds a pressure plate, which opens and closes doors within a range if a player stands on it.
Craftable with 3 wood and 1 surtling core with the Hammer.


## Installation
Extract the content of `Pressure-Plate` into the `BepInEx/plugins` folder.


## Developer
Inside the repo are two folders, `PressurePlate` for the mod and `UnityAssets` for the unity files.


Place all Unity, Valheim and dependencies assemblies in the `PressurePlate/libs` folder (create the folder if it doesn't exist) and add them as references to the visual studio project.

Place the same assemblies in `UnityAssets/Assets/Assemblies` (create the folder if it doesn't exist) if you want to edit the AssetBundle.
Build the AssetBundle with the Unity toolbar "Assets/Build AssetBundles" to automatically copy the resulting file to the mod folder.


## Changelog
0.0.2
- Added sound effects for pressing and releasing pressure plate

0.0.1
- Release


## Credits
- Sound effects: http://www.freesfx.co.uk
