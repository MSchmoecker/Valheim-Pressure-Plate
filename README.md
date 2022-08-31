# Pressure-Plate

## About
Adds multiple variation of pressure plates which open and close doors.
- Wooden pressure plate: 3 wood, 1 surtling core with the hammer. Requires a workbench in range
- Stone pressure plate: 3 stone, 1 surtling core with the hammer. Requires a workbench in range
- Crystal pressure plate, 1 crystal, 1 surtling core with the hammer. Requires a workbench in range

![showcase](https://raw.githubusercontent.com/MSchmoecker/Valheim-Pressure-Plate/master/Docs/Showcase.gif)


## Ingame settings


### Plate settings
![config](https://raw.githubusercontent.com/MSchmoecker/Valheim-Pressure-Plate/master/Docs/ExampleGUI.png)
- Trigger Radius: if a player is inside this range, the plate is pressed
- Door Radius: all doors inside this radius are opened/closed
- Activation Time: duration in which the plate is pressed
- Trigger Delay: duration it takes before the plate is pressed
- Invert Doors: inverts open/closed state
- Ignore wards: allows other players to open doors even they have no access
- Only open if not permitted: The plate only interacts with players that don't have permission inside the ward area. Requires 'Ignore wards' to be active
- Allow creatures to trigger: monster and creatures can also press the plate. Requires 'Ignore wards' to be active
- Tame interaction: decides what certain tame status is needed in order to interact with the plate. Requires both 'Ignore wards' and 'Allow creatures to trigger' to be active

### Global settings

Config file at `BepInEx/config/com.maxsch.valheim.pressure_plate.cfg`
- Plate Volume: set the volume level of all plates. Can be changed while ingame


## Manual Installation
1. Install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) and [Jötunn, the Valheim Library](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/)
2. Extract the content of `Pressure-Plate` into the `BepInEx/plugins` folder.

The mod can be installed at a server to enforce the installation on all clients.

## Development
See [contributing](https://github.com/MSchmoecker/Valheim-Pressure-Plate/blob/master/CONTRIBUTING.md).


## Changelog
0.7.0
- Added Spanish translation (thanks ErDu!)
- Added the option to specify the tame interaction when the plate allows creatures
- Added enforcement of mod version if the mod is installed on the server

0.6.4
- Fixed compatibility with WardIsLove 3.0.1
- Removed own menu patch for blocking the game menu after closing the UI with Escape, this is handled by Jotunn now

0.6.3
- Fixed an issue with the current beta, it's still compatible with the stable Valheim version

0.6.2
- Added compatibility with WardIsLove, only active with WardIsLove-2.3.3 or greater

0.6.1
- Added option to make a plate invisible
- Added option to allow only not permitted players
- Fixed doors could sometimes be opened even if a player has no access (and ignore wards was set to false)

0.6.0
- Added crystal pressure plate
- Added an option inside the config to set the volume of plate sounds
- All plates only require a workbench to be build

0.5.1
- Fixed MoreGates drawbridge open/close
- removed min. open time of doors

0.5.0
- Added an option to allow mobs to trigger a plate
- Improved networking code

0.4.3
- Fixed incompatibility with H&H update

0.4.2
- Added german translation
- Show plate name in UI and mouse hover
- Fixed input field text was not visible because of Jötunn 2.2.8

0.4.1
- Fixed trigger delay was ignored when the player left the plate, resulting in pressing the plate for a fraction of time
- Fixed trigger delay was not reset when the player left the plate

0.4.0
- Added UI to configure each plate individually
- Renamed "public/private" to "ignore wards"
- Removed global player settings

0.3.1
- Only show public/private option when the plate is inside an active ward
- Limited radius of configurable radius to 8 units
- Fixed a null error when destroying doors while standing on a pressure plate

0.3.0
- Added option to set plates public, this allows plates to bypass wards
- Fixed multiplayer async, resulting in opening a door after it was closed

0.2.1
- Fixed null error when placing a new door, as no ZNetView exists yet
- Fixed not opening a door if the player presses the plate while the door is closing

0.2.0
- Reworked internally how a pressure plate opens doors
- Modders can now set custom configurations for doors. This includes if the open/close is reversed and opening time
- A pressure plate no longer interrupts a open or close animation

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

Discord: Margmas#9562


## Credits
- Sound effects: http://www.freesfx.co.uk
