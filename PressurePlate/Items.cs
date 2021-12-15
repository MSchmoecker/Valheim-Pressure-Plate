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
            GameObject basePlate = assetBundle.LoadAsset<GameObject>("pressure_plate Wood.prefab");
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
                Category = Piece.PieceCategory.Building.ToString(),
                Name = "$pressure_plate_wood"
            };

            CustomPiece customPiece = new CustomPiece(plate,true, pieceConfig);

            GameObject prefab = customPiece.PiecePrefab;
            WearNTear wearNTear = prefab.GetComponent<WearNTear>();

            PrefabManager.OnPrefabsRegistered += ApplyEffects;

            void ApplyEffects() {
                PrefabManager.OnPrefabsRegistered -= ApplyEffects;
                Material woodMaterial = PrefabManager.Cache.GetPrefab<Material>("woodwall");
                wearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] { woodMaterial };
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static void AddStonePressurePlate(AssetBundle assetBundle) {
            GameObject basePlate = assetBundle.LoadAsset<GameObject>("pressure_plate Stone.prefab");
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
                Category = Piece.PieceCategory.Building.ToString(),
                Name = "$pressure_plate_stone"
            };

            CustomPiece customPiece = new CustomPiece(plate, true, pieceConfig);

            GameObject prefab = customPiece.PiecePrefab;
            WearNTear wearNTear = prefab.GetComponent<WearNTear>();

            PrefabManager.OnPrefabsRegistered += ApplyEffects;

            void ApplyEffects() {
                PrefabManager.OnPrefabsRegistered -= ApplyEffects;
                Material stoneMaterial = PrefabManager.Cache.GetPrefab<Material>("stonefloor");

                stoneMaterial = new Material(stoneMaterial);
                stoneMaterial.SetTextureScale("_MainTex", new Vector2(0.14f, 0.14f));
                stoneMaterial.SetTextureOffset("_MainTex", new Vector2(0.28f, 0.08f));
                wearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] { stoneMaterial };
            }

            PieceManager.Instance.AddPiece(customPiece);
        }
    }
}
