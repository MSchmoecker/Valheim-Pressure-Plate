using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle : MonoBehaviour {
    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles() {
        BuildPipeline.BuildAssetBundles("AssetBundles/StandaloneWindows", BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
        FileUtil.ReplaceFile("AssetBundles/StandaloneWindows/pressure_plate", "../PressurePlate/pressure_plate");
    }
}
