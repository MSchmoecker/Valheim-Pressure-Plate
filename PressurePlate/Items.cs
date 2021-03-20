using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ValheimLib;
using ValheimLib.ODB;

namespace PressurePlate {
    [HarmonyPatch]
    public class Items {
        private static bool addedItems;

        [HarmonyPatch(typeof(ZNetScene), "Awake"), HarmonyPrefix]
        public static void Init() {
            if (!addedItems) {
                AddItems();
                addedItems = true;
            }
        }

        private static void AddItems() {
            AssetBundle assetBundle = GetAssetBundleFromResources("pressure_plate");
            AddPlateItem(assetBundle);
            assetBundle.Unload(false);
        }

        private static Piece.Requirement[] GenerateRequirements(Dictionary<string, int> requirements) {
            List<Piece.Requirement> list = new List<Piece.Requirement>();

            foreach (KeyValuePair<string, int> requirement in requirements) {
                Piece.Requirement piece = MockRequirement.Create(requirement.Key, requirement.Value);
                piece.FixReferences();
                list.Add(piece);
            }

            return list.ToArray();
        }

        private static void AddPlateItem(AssetBundle assetBundle) {
            Material woodMaterial = Prefab.Cache.GetPrefab<Material>("woodwall");
            GameObject vfxPlaceWoodFloor = Prefab.Cache.GetPrefab<GameObject>("vfx_Place_wood_floor");
            GameObject sfxBuildHammerWood = Prefab.Cache.GetPrefab<GameObject>("sfx_build_hammer_wood");
            GameObject sfxWoodDestroyed = Prefab.Cache.GetPrefab<GameObject>("sfx_wood_destroyed");
            GameObject vfxSawDust = Prefab.Cache.GetPrefab<GameObject>("vfx_SawDust");
            GameObject workbench = Prefab.Cache.GetPrefab<GameObject>("piece_workbench");

            GameObject plate = assetBundle.LoadAsset<GameObject>("pressure_plate.prefab");
            GameObject cloned = plate.InstantiateClone("pressurePlate");
            Piece piece = cloned.GetComponent<Piece>();
            WearNTear wearNTear = cloned.GetComponent<WearNTear>();

            wearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] {woodMaterial};

            wearNTear.m_destroyedEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = sfxWoodDestroyed},
                new EffectList.EffectData() {m_prefab = vfxSawDust},
            };

            wearNTear.m_hitEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxSawDust},
            };

            piece.m_placeEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxPlaceWoodFloor},
                new EffectList.EffectData() {m_prefab = sfxBuildHammerWood},
            };

            piece.m_resources = GenerateRequirements(new Dictionary<string, int> {
                {"Wood", 3},
                {"SurtlingCore", 1}
            });

            piece.m_category = Piece.PieceCategory.Building;
            piece.m_craftingStation = workbench.GetComponent<CraftingStation>();

            GameObject hammerPrefab = Prefab.Cache.GetPrefab<GameObject>("_HammerPieceTable");
            PieceTable hammerTable = hammerPrefab.GetComponent<PieceTable>();
            hammerTable.m_pieces.Add(cloned.gameObject);
        }

        public static AssetBundle GetAssetBundleFromResources(string fileName) {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }
    }
}
