using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PressurePlate {
    public class PressurePlateUI : MonoBehaviour {
        private static GameObject uiRoot;

        public static void Init(AssetBundle assetBundle) {
            GameObject prefab = assetBundle.LoadAsset<GameObject>("PressurePlateUI");
            uiRoot = Instantiate(prefab, GUIManager.PixelFix.transform, false);

            ApplyAllComponents(uiRoot);
            ApplyWoodpanel(uiRoot.GetComponent<Image>());
            ApplyText(uiRoot.transform.Find("Title").GetComponent<Text>(), GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange);
        }

        public static void ApplyWoodpanel(Image image) {
            image.sprite = GUIManager.Instance.GetSprite("woodpanel_trophys");
        }

        public static void ApplyText(Text text, Font font, Color color) {
            text.font = font;
            text.color = color;
        }

        public static void ApplyAllDarken(GameObject root) {
            foreach (Image image in root.GetComponentsInChildren<Image>()) {
                if (image.gameObject.name == "Darken") {
                    image.sprite = GUIManager.Instance.GetSprite("darken_blob");
                    image.color = Color.white;
                }
            }
        }

        public static void ApplyAllSunken(GameObject root) {
            foreach (Image image in root.GetComponentsInChildren<Image>()) {
                if (image.gameObject.name == "Sunken") {
                    image.sprite = GUIManager.Instance.GetSprite("sunken");
                    image.color = Color.white;
                    image.type = Image.Type.Sliced;
                    image.pixelsPerUnitMultiplier = 1;
                }
            }
        }

        public static void ApplyAllComponents(GameObject root) {
            foreach (Text text in root.GetComponentsInChildren<Text>()) {
                ApplyText(text, GUIManager.Instance.AveriaSerif, new Color(219f / 255f, 219f / 255f, 219f / 255f));
            }

            foreach (InputField inputField in root.GetComponentsInChildren<InputField>()) {
                GUIManager.Instance.ApplyInputFieldStyle(inputField);
            }

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>()) {
                GUIManager.Instance.ApplyToogleStyle(toggle);
            }

            ApplyAllDarken(root);
            ApplyAllSunken(root);
        }
    }
}
