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
            // wood pressure plate
            AddPressurePlate(assetBundle,
                             "pressure_plate Wood.prefab",
                             "pressurePlate_PressurePlate_Items_AddPlateItem",
                             "$pressure_plate_wood",
                             "pressure_plate_wood_icon.png",
                             "woodwall",
                             new RequirementConfig[] {
                                 new RequirementConfig() { Item = "Wood", Amount = 3, Recover = true },
                                 new RequirementConfig() { Item = "SurtlingCore", Amount = 1, Recover = true },
                             },
                             new Vector2(-0.56f, 0.12f),
                             new Vector2(0, 0.014f)
                            );

            // stone pressure plate
            AddPressurePlate(assetBundle,
                             "pressure_plate Stone.prefab",
                             "pressurePlate_stone_PressurePlate_Items_AddPlateItem",
                             "$pressure_plate_stone",
                             "pressure_plate_stone_icon.png",
                             "stonefloor",
                             new RequirementConfig[] {
                                 new RequirementConfig() { Item = "Stone", Amount = 3, Recover = true },
                                 new RequirementConfig() { Item = "SurtlingCore", Amount = 1, Recover = true },
                             },
                             new Vector2(0.14f, 0.14f),
                             new Vector2(0.28f, 0.08f)
                            );

            // crystal pressure plate
            AddPressurePlate(assetBundle,
                             "pressure_plate Crystal.prefab",
                             "CrystalPressurePlate",
                             "$pressure_plate_crystal",
                             "pressure_plate_crystal_icon.png",
                             "crystal_window",
                             new RequirementConfig[] {
                                 new RequirementConfig() { Item = "Crystal", Amount = 1, Recover = true },
                                 new RequirementConfig() { Item = "SurtlingCore", Amount = 1, Recover = true },
                             },
                             new Vector2(1.5f, 1.5f),
                             new Vector2(0f, 0f)
                            );
        }

        private static void AddPressurePlate(AssetBundle assetBundle, string assetName, string newName, string tokenName, string iconName,
            string materialName, RequirementConfig[] requirements, Vector2 textureScale, Vector2 textureOffset) {
            GameObject basePlate = assetBundle.LoadAsset<GameObject>(assetName);
            GameObject plate = PrefabManager.Instance.CreateClonedPrefab(newName, basePlate);
            Sprite icon = assetBundle.LoadAsset<Sprite>(iconName);

            PieceConfig pieceConfig = new PieceConfig() {
                CraftingStation = "piece_workbench",
                Requirements = requirements,
                PieceTable = "Hammer",
                Icon = icon,
                Category = Piece.PieceCategory.Misc.ToString(),
                Name = tokenName
            };

            CustomPiece customPiece = new CustomPiece(plate, true, pieceConfig);

            GameObject prefab = customPiece.PiecePrefab;
            WearNTear wearNTear = prefab.GetComponent<WearNTear>();

            PrefabManager.OnPrefabsRegistered += ApplyEffects;

            void ApplyEffects() {
                PrefabManager.OnPrefabsRegistered -= ApplyEffects;
                Material material = PrefabManager.Cache.GetPrefab<Material>(materialName);

                material = new Material(material);
                material.SetTextureScale("_MainTex", textureScale);
                material.SetTextureOffset("_MainTex", textureOffset);
                wearNTear.m_new.GetComponent<MeshRenderer>().materials = new[] { material };
            }

            PieceManager.Instance.AddPiece(customPiece);
        }
    }
}
