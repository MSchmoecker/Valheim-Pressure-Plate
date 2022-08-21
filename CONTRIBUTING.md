## Development
Inside the repo are two folders, `PressurePlate` for the mod and `UnityAssets` for the Unity files.


### Mod Setup
Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman you can set the path too, but this is optional.

````
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>
        <!-- Optional, needs to be the path to a r2modmanPlus profile folder -->
        <R2MODMAN_INSTALL>C:\Users\[user]\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>
        <USE_R2MODMAN_AS_DEPLOY_FOLDER>false</USE_R2MODMAN_AS_DEPLOY_FOLDER>
    </PropertyGroup>
</Project>
````

This requires the publicized Valheim dlls. See here for example: https://github.com/CabbageCrow/AssemblyPublicizer


### Unity Setup
This step is only needed if you want to compile the AssetBundle.

Place all needed Unity, Valheim and dependencies assemblies in the `UnityAssets/Assets/Assemblies` folder.
See `PressurePlate/PressurePlate.csproj` for reference. If some are still missing, Unity prints the name of a missing dependencies to the console.

Build the AssetBundle with the Unity toolbar "Assets/Build AssetBundles" to automatically copy the resulting file to the mod folder.
