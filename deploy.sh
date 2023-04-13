ModName="PressurePlate"

# function for xml reading
read_dom () {
  local IFS=\>
  read -d \< ENTITY CONTENT
}

# read install folder from environment
while read_dom; do
	if [[ $ENTITY = "VALHEIM_INSTALL" ]]; then
		VALHEIM_INSTALL=$CONTENT
	fi
	if [[ $ENTITY = "R2MODMAN_INSTALL" ]]; then
		R2MODMAN_INSTALL=$CONTENT
	fi
	if [[ $ENTITY = "USE_R2MODMAN_AS_DEPLOY_FOLDER" ]]; then
		USE_R2MODMAN_AS_DEPLOY_FOLDER=$CONTENT
	fi
done < Environment.props

# set ModDir
if $USE_R2MODMAN_AS_DEPLOY_FOLDER; then
  BEPINEX_INSTALL="$R2MODMAN_INSTALL/BepInEx"
else
  BEPINEX_INSTALL="$VALHEIM_INSTALL/BepInEx"
fi

PluginFolder="$BEPINEX_INSTALL/plugins"
ModDir="$PluginFolder/$ModName"

# make mod dir if not existing
mkdir -p "$ModDir"

# copy files to mod dir
cp "./PressurePlate/bin/Debug/net472/PressurePlate.dll" "$ModDir/PressurePlate.dll"
cp "./README.md"  "$ModDir"
cp "./icon.png"  "$ModDir"
cp "./manifest.json"  "$ModDir"

# copy files to unity
cp "PressurePlate/bin/Debug/net472/PressurePlate.dll" "UnityAssets/Assets/Assemblies/PressurePlate.dll"
cp "$BEPINEX_INSTALL/core/BepInEx.dll" "UnityAssets/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/0Harmony.dll" "UnityAssets/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/Mono.Cecil.dll" "UnityAssets/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/MonoMod.Utils.dll" "UnityAssets/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/MonoMod.RuntimeDetour.dll" "UnityAssets/Assets/Assemblies"
[ -f "$PluginFolder/ValheimModding-Jotunn/Jotunn.dll" ] && cp "$PluginFolder/ValheimModding-Jotunn/Jotunn.dll" "UnityAssets/Assets/Assemblies"
[ -f "$PluginFolder/Jotunn/Jotunn.dll" ] && cp "$PluginFolder/Jotunn/Jotunn.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_valheim.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_utils.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_postprocessing.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_sunshafts.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_guiutils.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_steamworks.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_googleanalytics.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/PlayFab.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/PlayFabParty.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/Fishlabs.Core.dll" "UnityAssets/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/Fishlabs.Common.dll" "UnityAssets/Assets/Assemblies"

# make zip files
cd "$ModDir" || exit

[ -f "$ModName.zip" ] && rm "$ModName.zip"
[ -f "$ModName-Nexus.zip" ] && rm "$ModName-Nexus.zip"

mkdir -p plugins
cp "$ModName.dll" plugins

zip "$ModName.zip" "$ModName.dll" README.md manifest.json icon.png
zip -r "$ModName-Nexus.zip" plugins

rm -r plugins
