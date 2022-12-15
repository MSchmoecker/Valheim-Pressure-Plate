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
        [SerializeField] private Text title;
        [SerializeField] private InputField triggerRadiusHorizontal;
        [SerializeField] private InputField triggerRadiusVertical;
        [SerializeField] private InputField openRadiusHorizontal;
        [SerializeField] private InputField openRadiusVertical;
        [SerializeField] private InputField openTime;
        [SerializeField] private InputField triggerDelay;
        [SerializeField] private Text allowMobsText;
        [SerializeField] private Text onlyOpenNotPermittedText;
        [SerializeField] private Toggle invert;
        [SerializeField] private Toggle toggleMode;
        [SerializeField] private Toggle ignoreWards;
        [SerializeField] private Toggle allowMobs;
        [SerializeField] private Dropdown mobTameInteraction;
        [SerializeField] private Text mobTameInteractionText;
        [SerializeField] private Toggle isInvisible;
        [SerializeField] private Toggle onlyOpenNotPermitted;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button pasteButton;
        [SerializeField] private Button resetButton;
#pragma warning restore 0649

        private static GameObject uiRoot;
        private Plate target;
        private Plate copy;

        public static void Init(AssetBundle assetBundle) {
            GameObject prefab = assetBundle.LoadAsset<GameObject>("PressurePlateUI");
            PressurePlateUI ui = Instantiate(prefab, GUIManager.CustomGUIFront.transform, false).GetComponent<PressurePlateUI>();
            uiRoot = ui.transform.GetChild(0).gameObject;

            ApplyAllComponents(uiRoot);
            ApplyWoodpanel(uiRoot.GetComponent<Image>());
            ApplyText(ui.title, GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange);
            ApplyLocalization();

            uiRoot.SetActive(false);
        }

        private void Awake() {
            instance = this;

            triggerRadiusHorizontal.onValueChanged.AddListener(i => target.TriggerRadiusHorizontal.ForceSet(i));
            triggerRadiusVertical.onValueChanged.AddListener(i => target.TriggerRadiusVertical.ForceSet(i));
            openRadiusHorizontal.onValueChanged.AddListener(i => target.OpenRadiusHorizontal.ForceSet(i));
            openRadiusVertical.onValueChanged.AddListener(i => target.OpenRadiusVertical.ForceSet(i));
            openTime.onValueChanged.AddListener(i => target.OpenTime.ForceSet(i));
            triggerDelay.onValueChanged.AddListener(i => target.TriggerDelay.ForceSet(i));
            invert.onValueChanged.AddListener(i => target.Invert.ForceSet(i));
            toggleMode.onValueChanged.AddListener(i => target.ToggleMode.ForceSet(i));
            ignoreWards.onValueChanged.AddListener(i => target.IgnoreWards.ForceSet(i));
            allowMobs.onValueChanged.AddListener(i => target.AllowMobs.ForceSet(i));
            mobTameInteraction.onValueChanged.AddListener(i => target.MobTameInteraction.ForceSet(i));
            isInvisible.onValueChanged.AddListener(i => target.IsInvisible.ForceSet(i));
            onlyOpenNotPermitted.onValueChanged.AddListener(i => target.OnlyOpenNotPermitted.ForceSet(i));

            ignoreWards.onValueChanged.AddListener((_) => UpdateDeactivated());
            allowMobs.onValueChanged.AddListener((_) => UpdateDeactivated());

            mobTameInteraction.options = new List<Dropdown.OptionData>() {
                new Dropdown.OptionData(Localization.instance.Localize("$pressure_plate_tame_interaction_all")),
                new Dropdown.OptionData(Localization.instance.Localize("$pressure_plate_tame_interaction_only_tame")),
                new Dropdown.OptionData(Localization.instance.Localize("$pressure_plate_tame_interaction_only_not_tame"))
            };

            copyButton.onClick.AddListener(() => { copy = target; });
            pasteButton.onClick.AddListener(() => {
                target.PasteData(copy);
                UpdateText();
            });
            resetButton.onClick.AddListener(() => {
                target.ResetValues();
                UpdateText();
            });
        }

        private void Update() {
            IsFrameBlocked = false;

            if (IsOpen() && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("Inventory"))) {
                IsFrameBlocked = true;

                target = null;
                SetGUIState(false);
            }

            pasteButton.interactable = copy != null;
        }

        public void OpenUI(Plate plate) {
            target = plate;
            SetGUIState(true);
            UpdateText();
        }

        private void UpdateText() {
            title.text = target.GetHoverName();
            triggerRadiusHorizontal.text = target.TriggerRadiusHorizontal.Get().ToString();
            triggerRadiusVertical.text = target.TriggerRadiusVertical.Get().ToString();
            openRadiusHorizontal.text = target.OpenRadiusHorizontal.Get().ToString();
            openRadiusVertical.text = target.OpenRadiusVertical.Get().ToString();
            openTime.text = target.OpenTime.Get().ToString();
            triggerDelay.text = target.TriggerDelay.Get().ToString();
            invert.isOn = target.Invert.Get();
            toggleMode.isOn = target.ToggleMode.Get();
            ignoreWards.isOn = target.IgnoreWards.Get();
            allowMobs.isOn = target.AllowMobs.Get();
            mobTameInteraction.value = target.MobTameInteraction.Get();
            isInvisible.isOn = target.IsInvisible.Get();
            onlyOpenNotPermitted.isOn = target.OnlyOpenNotPermitted.Get();

            UpdateDeactivated();
        }

        private void UpdateDeactivated() {
            allowMobs.interactable = ignoreWards.isOn;
            allowMobsText.color = ignoreWards.isOn ? Color.white : Color.grey;
            onlyOpenNotPermitted.interactable = ignoreWards.isOn;
            onlyOpenNotPermittedText.color = ignoreWards.isOn ? Color.white : Color.grey;
            mobTameInteraction.interactable = ignoreWards.isOn && allowMobs.isOn;
            mobTameInteractionText.color = mobTameInteraction.interactable ? Color.white : Color.grey;
            mobTameInteraction.captionText.color = mobTameInteraction.interactable ? Color.white : Color.grey;
        }

        private void SetGUIState(bool active) {
            isOpen = active;
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
                GUIManager.Instance.ApplyInputFieldStyle(inputField, 16);
            }

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>()) {
                GUIManager.Instance.ApplyToogleStyle(toggle);
            }

            foreach (Button button in root.GetComponentsInChildren<Button>()) {
                GUIManager.Instance.ApplyButtonStyle(button);
            }

            foreach (Dropdown dropdown in root.GetComponentsInChildren<Dropdown>()) {
                GUIManager.Instance.ApplyDropdownStyle(dropdown);
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
