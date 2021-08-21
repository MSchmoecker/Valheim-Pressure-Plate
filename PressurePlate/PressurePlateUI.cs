using System;
using System.Collections.Generic;
using System.Globalization;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PressurePlate {
    public class PressurePlateUI : MonoBehaviour {
        public static PressurePlateUI instance;
        public static bool IsOpen() => isOpen;

        public bool IsFrameBlocked { get; private set; }

        private static bool isOpen;

        // Disable Field XYZ is never assigned to, and will always have its default value XX
#pragma warning disable 0649
        [SerializeField] private InputField triggerRadiusHorizontal;
        [SerializeField] private InputField triggerRadiusVertical;
        [SerializeField] private InputField openRadiusHorizontal;
        [SerializeField] private InputField openRadiusVertical;
        [SerializeField] private InputField openTime;
        [SerializeField] private InputField triggerDelay;
        [SerializeField] private Toggle invert;
        [SerializeField] private Toggle ignoreWards;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button pasteButton;
        [SerializeField] private Button resetButton;
#pragma warning restore 0649

        private static GameObject uiRoot;
        private Plate target;
        private PlateData copyData;

        public static void Init(AssetBundle assetBundle) {
            GameObject prefab = assetBundle.LoadAsset<GameObject>("PressurePlateUI");
            uiRoot = Instantiate(prefab, GUIManager.PixelFix.transform, false).transform.GetChild(0).gameObject;

            ApplyAllComponents(uiRoot);
            ApplyWoodpanel(uiRoot.GetComponent<Image>());
            Text title = uiRoot.transform.Find("Title").GetComponent<Text>();
            ApplyText(title, GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange);
            ApplyLocalization();

            uiRoot.SetActive(false);
        }

        private void Awake() {
            instance = this;

            triggerRadiusHorizontal.onValueChanged.AddListener(i => SetSettingFloat(Plate.KeyTriggerRadiusHorizontal, i));
            triggerRadiusVertical.onValueChanged.AddListener(i => SetSettingFloat(Plate.KeyTriggerRadiusVertical, i));
            openRadiusHorizontal.onValueChanged.AddListener(i => SetSettingFloat(Plate.KeyOpenRadiusHorizontal, i));
            openRadiusVertical.onValueChanged.AddListener(i => SetSettingFloat(Plate.KeyOpenRadiusVertical, i));
            openTime.onValueChanged.AddListener(i => SetSettingFloat(Plate.KeyOpenTime, i));
            triggerDelay.onValueChanged.AddListener(i => SetSettingFloat(Plate.KeyTriggerDelay, i));
            invert.onValueChanged.AddListener(i => SetSettingBool(Plate.KeyInvert, i));
            ignoreWards.onValueChanged.AddListener(i => SetSettingBool(Plate.KeyIgnoreWards, i));
            copyButton.onClick.AddListener(() => {
                copyData ??= new PlateData();
                copyData.GetData(target);
            });
            pasteButton.onClick.AddListener(() => {
                copyData.SetData(target);
                UpdateText();
            });
            resetButton.onClick.AddListener(() => {
                target.ResetValues();
                UpdateText();
            });
        }

        private void Update() {
            Enum.TryParse(ZInput.instance.GetBoundKeyString("Use"), out KeyCode useKey);
            IsFrameBlocked = false;

            if (IsOpen() && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(useKey))) {
                IsFrameBlocked = true;

                target = null;
                SetGUIState(false);
            }

            pasteButton.interactable = copyData != null;
        }

        public void OpenUI(Plate plate) {
            target = plate;
            SetGUIState(true);
            UpdateText();
        }

        private void UpdateText() {
            triggerRadiusHorizontal.text = target.GetTriggerRadiusHorizontal().ToString();
            triggerRadiusVertical.text = target.GetTriggerRadiusVertical().ToString();
            openRadiusHorizontal.text = target.GetOpenRadiusHorizontal().ToString();
            openRadiusVertical.text = target.GetOpenRadiusVertical().ToString();
            openTime.text = target.GetOpenTime().ToString();
            triggerDelay.text = target.GetTriggerDelay().ToString();
            invert.isOn = target.GetInvert();
            ignoreWards.isOn = target.GetIgnoreWards();
        }

        private void SetGUIState(bool active) {
            isOpen = active;
            uiRoot.SetActive(active);
            GUIManager.BlockInput(active);
        }

        private void SetSettingFloat(string key, string input) {
            ZDO zdo = target.zNetView.GetZDO();
            if (float.TryParse(input, out float value)) {
                zdo.Set(key, value);
            }
        }

        private void SetSettingBool(string key, bool input) {
            ZDO zdo = target.zNetView.GetZDO();
            zdo.Set(key, input);
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

            foreach (Button button in root.GetComponentsInChildren<Button>()) {
                GUIManager.Instance.ApplyButtonStyle(button);
            }

            ApplyAllDarken(root);
            ApplyAllSunken(root);
        }

        public static void ApplyLocalization() {
            foreach (Text text in uiRoot.GetComponentsInChildren<Text>()) {
                text.text = Localization.instance.Localize(text.text);
            }
        }
    }
}
