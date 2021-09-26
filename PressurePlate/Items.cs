using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace PressurePlate {
    public class Items {
        public static void Init(AssetBundle assetBundle) {
            AddWoodPressurePlate(assetBundle);
            AddStonePressurePlate(assetBundle);
        }

        private static void AddWoodPressurePlate(AssetBundle assetBundle) {
            GameObject basePlate = assetBundle.LoadAsset<GameObject>("pressure_plate.prefab");
            GameObject plate = PrefabManager.Instance.CreateClonedPrefab("pressurePlate_PressurePlate_Items_AddPlateItem", basePlate);
            Sprite icon = assetBundle.LoadAsset<Sprite>("pressure_plate_wood_icon.png");

            PieceConfig pieceConfig = new PieceConfig() {
                CraftingStation = "piece_workbench",
                Requirements = new RequirementConfig[] {
                    new RequirementConfig() { Item = "Wood", Amount = 3, Recover = true },
                    new RequirementConfig() { Item = "SurtlingCore", Amount = 1, Recover = true },
                },
                PieceTable = "Hammer",
                Icon = icon,
            };

            CustomPiece customPiece = new CustomPiece(plate, false, pieceConfig);

            Piece piece = customPiece.Piece;
            GameObject prefab = customPiece.PiecePrefab;
            WearNTear wearNTear = prefab.GetComponent<WearNTear>();

            piece.m_category = Piece.PieceCategory.Building;
            piece.m_name = "$pressure_plate_wood";

            PrefabManager.OnPrefabsRegistered += ApplyEffects;

            void ApplyEffects() {
                PrefabManager.OnPrefabsRegistered -= ApplyEffects;
                Material woodMaterial = PrefabManager.Cache.GetPrefab<Material>("woodwall");
                GameObject vfxPlaceWoodFloor = PrefabManager.Cache.GetPrefab<GameObject>("vfx_Place_wood_floor");
                GameObject sfxBuildHammerWood = PrefabManager.Cache.GetPrefab<GameObject>("sfx_build_hammer_wood");
                GameObject sfxWoodDestroyed = PrefabManager.Cache.GetPrefab<GameObject>("sfx_wood_destroyed");
                GameObject vfxSawDust = PrefabManager.Cache.GetPrefab<GameObject>("vfx_SawDust");

                wearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] { woodMaterial };
                wearNTear.m_destroyedEffect.m_effectPrefabs = new[] {
                    new EffectList.EffectData() { m_prefab = sfxWoodDestroyed },
                    new EffectList.EffectData() { m_prefab = vfxSawDust },
                };
                wearNTear.m_hitEffect.m_effectPrefabs = new[] {
                    new EffectList.EffectData() { m_prefab = vfxSawDust },
                };
                piece.m_placeEffect.m_effectPrefabs = new[] {
                    new EffectList.EffectData() { m_prefab = vfxPlaceWoodFloor },
                    new EffectList.EffectData() { m_prefab = sfxBuildHammerWood },
                };
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static void AddStonePressurePlate(AssetBundle assetBundle) {
            GameObject basePlate = assetBundle.LoadAsset<GameObject>("pressure_plate.prefab");
            GameObject plate = PrefabManager.Instance.CreateClonedPrefab("pressurePlate_stone_PressurePlate_Items_AddPlateItem", basePlate);
            Sprite icon = assetBundle.LoadAsset<Sprite>("pressure_plate_stone_icon.png");

            PieceConfig pieceConfig = new PieceConfig() {
                CraftingStation = "piece_stonecutter",
                Requirements = new RequirementConfig[] {
                    new RequirementConfig() { Item = "Stone", Amount = 3, Recover = true },
                    new RequirementConfig() { Item = "SurtlingCore", Amount = 1, Recover = true },
                },
                PieceTable = "Hammer",
                Icon = icon,
            };

            CustomPiece customPiece = new CustomPiece(plate, false, pieceConfig);

            Piece piece = customPiece.Piece;
            GameObject prefab = customPiece.PiecePrefab;
            WearNTear wearNTear = prefab.GetComponent<WearNTear>();

            piece.m_category = Piece.PieceCategory.Building;
            piece.m_name = "$pressure_plate_stone";

            PrefabManager.OnPrefabsRegistered += ApplyEffects;

            void ApplyEffects() {
                PrefabManager.OnPrefabsRegistered -= ApplyEffects;
                Material stoneMaterial = PrefabManager.Cache.GetPrefab<Material>("stonefloor");
                GameObject vfxPlaceStoneFloor = PrefabManager.Cache.GetPrefab<GameObject>("vfx_Place_stone_wall_2x1");
                GameObject sfxBuildHammerStone = PrefabManager.Cache.GetPrefab<GameObject>("sfx_build_hammer_stone");
                GameObject vfxRockDestroyed = PrefabManager.Cache.GetPrefab<GameObject>("vfx_RockDestroyed");
                GameObject sfxRockDestroyed = PrefabManager.Cache.GetPrefab<GameObject>("sfx_rock_destroyed");
                GameObject vfxRockHit = PrefabManager.Cache.GetPrefab<GameObject>("vfx_RockHit");
                GameObject sfxRockHit = PrefabManager.Cache.GetPrefab<GameObject>("sfx_rock_hit");

                stoneMaterial = new Material(stoneMaterial);
                stoneMaterial.SetTextureScale("_MainTex", new Vector2(0.14f, 0.14f));
                stoneMaterial.SetTextureOffset("_MainTex", new Vector2(0.28f, 0.08f));
                wearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] { stoneMaterial };

                wearNTear.m_destroyedEffect.m_effectPrefabs = new[] {
                    new EffectList.EffectData() { m_prefab = vfxRockDestroyed },
                    new EffectList.EffectData() { m_prefab = sfxRockDestroyed },
                };
                wearNTear.m_hitEffect.m_effectPrefabs = new[] {
                    new EffectList.EffectData() { m_prefab = vfxRockHit },
                    new EffectList.EffectData() { m_prefab = sfxRockHit },
                };
                piece.m_placeEffect.m_effectPrefabs = new[] {
                    new EffectList.EffectData() { m_prefab = vfxPlaceStoneFloor },
                    new EffectList.EffectData() { m_prefab = sfxBuildHammerStone },
                };
            }

            PieceManager.Instance.AddPiece(customPiece);
        }
    }
}
