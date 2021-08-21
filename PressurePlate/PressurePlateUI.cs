using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PressurePlate {
    public class PressurePlateUI : MonoBehaviour {
        public static PressurePlateUI instance;
        public bool IsOpen { get; private set; }
        public bool IsFrameBlocked { get; private set; }

        private static GameObject uiRoot;
        private Plate target;

        private void Awake() {
            Log.LogInfo(instance == null);
            instance = this;
        }

        public static void Init(AssetBundle assetBundle) {
            GameObject prefab = assetBundle.LoadAsset<GameObject>("PressurePlateUI");
            uiRoot = Instantiate(prefab, GUIManager.PixelFix.transform, false).transform.GetChild(0).gameObject;

            ApplyAllComponents(uiRoot);
            ApplyWoodpanel(uiRoot.GetComponent<Image>());
            ApplyText(uiRoot.transform.Find("Title").GetComponent<Text>(), GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange);

            uiRoot.SetActive(false);
        }

        public void OpenUI(Plate plate) {
            target = plate;
            SetGUIState(true);
        }

        private void Update() {
            Enum.TryParse(ZInput.instance.GetBoundKeyString("Use"), out KeyCode useKey);
            IsFrameBlocked = false;

            if (IsOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(useKey))) {
                IsFrameBlocked = true;

                target = null;
                SetGUIState(false);
            }
        }

        private void SetGUIState(bool active) {
            IsOpen = active;
            uiRoot.SetActive(active);
            GUIManager.BlockInput(active);
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
