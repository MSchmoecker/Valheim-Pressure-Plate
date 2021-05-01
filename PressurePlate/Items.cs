using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ValheimLib;
using ValheimLib.ODB;

namespace PressurePlate {
    public class Items {
        public static void Init() {
            ObjectDBHelper.OnAfterInit += AddItems;
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
            // load wood assets
            Material woodMaterial = Prefab.Cache.GetPrefab<Material>("woodwall");
            GameObject vfxPlaceWoodFloor = Prefab.Cache.GetPrefab<GameObject>("vfx_Place_wood_floor");
            GameObject sfxBuildHammerWood = Prefab.Cache.GetPrefab<GameObject>("sfx_build_hammer_wood");
            GameObject sfxWoodDestroyed = Prefab.Cache.GetPrefab<GameObject>("sfx_wood_destroyed");
            GameObject vfxSawDust = Prefab.Cache.GetPrefab<GameObject>("vfx_SawDust");
            GameObject workbench = Prefab.Cache.GetPrefab<GameObject>("piece_workbench");

            // load stone assets
            Material stoneMaterial = Prefab.Cache.GetPrefab<Material>("stonefloor");
            GameObject vfxPlaceStoneFloor = Prefab.Cache.GetPrefab<GameObject>("vfx_Place_stone_wall_2x1");
            GameObject sfxBuildHammerStone = Prefab.Cache.GetPrefab<GameObject>("sfx_build_hammer_stone");
            GameObject vfxRockDestroyed = Prefab.Cache.GetPrefab<GameObject>("vfx_RockDestroyed");
            GameObject sfxRockDestroyed = Prefab.Cache.GetPrefab<GameObject>("sfx_rock_destroyed");
            GameObject vfxRockHit = Prefab.Cache.GetPrefab<GameObject>("vfx_RockHit");
            GameObject sfxRockHit = Prefab.Cache.GetPrefab<GameObject>("sfx_rock_hit");
            GameObject stonecutter = Prefab.Cache.GetPrefab<GameObject>("piece_stonecutter");

            // load plate GameObject
            GameObject plate = assetBundle.LoadAsset<GameObject>("pressure_plate.prefab");

            // create wood pressure plate
            GameObject woodPressurePlate = plate.InstantiateClone("pressurePlate");
            Piece woodPiece = woodPressurePlate.GetComponent<Piece>();
            WearNTear woodWearNTear = woodPressurePlate.GetComponent<WearNTear>();

            woodWearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] {woodMaterial};
            woodWearNTear.m_destroyedEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = sfxWoodDestroyed},
                new EffectList.EffectData() {m_prefab = vfxSawDust},
            };
            woodWearNTear.m_hitEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxSawDust},
            };
            woodPiece.m_placeEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxPlaceWoodFloor},
                new EffectList.EffectData() {m_prefab = sfxBuildHammerWood},
            };
            woodPiece.m_resources = GenerateRequirements(new Dictionary<string, int> {
                {"Wood", 3},
                {"SurtlingCore", 1}
            });
            woodPiece.m_category = Piece.PieceCategory.Building;
            woodPiece.m_craftingStation = workbench.GetComponent<CraftingStation>();
            woodPiece.m_name = "$pressure_plate_wood";

            // create stone pressure plate
            GameObject stonePressurePlate = plate.InstantiateClone("pressurePlate_stone");
            Piece stonePiece = stonePressurePlate.GetComponent<Piece>();
            WearNTear stoneWearNTear = stonePressurePlate.GetComponent<WearNTear>();

            stoneMaterial = new Material(stoneMaterial);
            stoneMaterial.SetTextureScale("_MainTex", new Vector2(0.14f, 0.14f));
            stoneMaterial.SetTextureOffset("_MainTex", new Vector2(0.28f, 0.08f));
            stoneWearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] {stoneMaterial};
            stoneWearNTear.m_destroyedEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxRockDestroyed},
                new EffectList.EffectData() {m_prefab = sfxRockDestroyed},
            };
            stoneWearNTear.m_hitEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxRockHit},
                new EffectList.EffectData() {m_prefab = sfxRockHit},
            };
            stonePiece.m_placeEffect.m_effectPrefabs = new[] {
                new EffectList.EffectData() {m_prefab = vfxPlaceStoneFloor},
                new EffectList.EffectData() {m_prefab = sfxBuildHammerStone},
            };
            stonePiece.m_resources = GenerateRequirements(new Dictionary<string, int> {
                {"Stone", 3},
                {"SurtlingCore", 1}
            });
            stonePiece.m_category = Piece.PieceCategory.Building;
            stonePiece.m_craftingStation = stonecutter.GetComponent<CraftingStation>();
            stonePiece.m_name = "$pressure_plate_stone";

            // add pressure plates to hammer
            GameObject hammerPrefab = Prefab.Cache.GetPrefab<GameObject>("_HammerPieceTable");
            PieceTable hammerTable = hammerPrefab.GetComponent<PieceTable>();
            hammerTable.m_pieces.Add(woodPressurePlate.gameObject);
            hammerTable.m_pieces.Add(stonePressurePlate.gameObject);
        }

        public static AssetBundle GetAssetBundleFromResources(string fileName) {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }
    }
}
