using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ValheimLib;
using ValheimLib.ODB;
using Newtonsoft.Json;

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
            Material woodMaterial = Prefab.Cache.GetPrefab<Material>("woodwall");

            GameObject plate = assetBundle.LoadAsset<GameObject>("pressure_plate.prefab");
            GameObject cloned = plate.InstantiateClone("pressurePlate");

            cloned.GetComponent<Plate>().plate.GetComponent<MeshRenderer>().materials = new[] {woodMaterial};
            Piece piece = cloned.GetComponent<Piece>();

            piece.m_category = Piece.PieceCategory.Misc;
            piece.m_resources = GenerateRequirements(new Dictionary<string, int> {
                {"Wood", 4}
            });

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
