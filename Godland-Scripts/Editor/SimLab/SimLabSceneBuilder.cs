using Godland.Game.Presentation.Settings;
using Godland.Game.SimLab;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Godland.Editor.SimLab
{
    public static class SimLabSceneBuilder
    {
        public const string ScenePath = "Assets/Godland/Scenes/SimLab.unity";
        private const string RowPath = "Assets/Godland/Prefabs/UI/SimLabRow.prefab";
        public const string SettingsPath = "Assets/Godland/Configs/GameSettings.asset";
        private const string FontSource = "Assets/Godland/Prefabs/UI/OverviewTab.prefab";

        private static TMP_FontAsset _font;

        [MenuItem("Godland/SimLab/Собрать сцену")]
        public static void Build()
        {
            _font = AssetDatabase.LoadAssetAtPath<GameObject>(FontSource).GetComponentInChildren<TMP_Text>(true).font;
            var row = BuildRow();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.07f, 0.08f);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var canvas = (RectTransform)canvasObject.transform;

            var left = Panel("Variables", canvas, new Vector2(0, 0), new Vector2(0.36f, 1), new Vector4(16, 16, 8, 16));
            var title = Text("Title", left, "Godland · SimLab", 34, TextAlignmentOptions.Left);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector4(16, -60, 16, -12));
            var buttons = Rect("Buttons", left);
            Anchor(buttons, new Vector2(0, 1), new Vector2(1, 1), new Vector4(16, -120, 16, -68));
            var layout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            var start = Button("Start", buttons, "Старт", new Color(0.25f, 0.45f, 0.25f));
            var stop = Button("Stop", buttons, "Стоп", new Color(0.5f, 0.25f, 0.22f));
            var reset = Button("Reset", buttons, "Сброс", new Color(0.3f, 0.3f, 0.34f));
            var download = Button("Download", buttons, "Скачать лог", new Color(0.45f, 0.36f, 0.16f));
            var fields = Scroll("Fields", left, new Vector4(16, 16, 16, 128), out _);

            var right = Panel("Log", canvas, new Vector2(0.36f, 0), new Vector2(1, 1), new Vector4(8, 16, 16, 16));
            var status = Text("Status", right, "", 22, TextAlignmentOptions.Left);
            status.color = new Color(1f, 0.82f, 0.48f);
            Anchor(status.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector4(16, -52, 16, -12));
            var logContent = Scroll("LogScroll", right, new Vector4(16, 16, 16, 60), out var logScroll);
            var log = Text("LogText", logContent, "", 17, TextAlignmentOptions.TopLeft);
            log.richText = false;
            log.gameObject.AddComponent<LayoutElement>();

            var view = canvasObject.AddComponent<SimLabView>();
            var so = new SerializedObject(view);
            so.FindProperty("_fields").objectReferenceValue = fields;
            so.FindProperty("_rowPrefab").objectReferenceValue = row;
            so.FindProperty("_start").objectReferenceValue = start;
            so.FindProperty("_stop").objectReferenceValue = stop;
            so.FindProperty("_reset").objectReferenceValue = reset;
            so.FindProperty("_download").objectReferenceValue = download;
            so.FindProperty("_status").objectReferenceValue = status;
            so.FindProperty("_log").objectReferenceValue = log;
            so.FindProperty("_logScroll").objectReferenceValue = logScroll;
            so.ApplyModifiedPropertiesWithoutUndo();

            var scope = new GameObject("SimLabScope").AddComponent<SimLabLifetimeScope>();
            var scopeObject = new SerializedObject(scope);
            scopeObject.FindProperty("_settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameSettings>(SettingsPath);
            scopeObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static SimLabFieldView BuildRow()
        {
            var root = new GameObject("SimLabRow", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
            root.GetComponent<Button>().targetGraphic = root.GetComponent<Image>();
            root.GetComponent<LayoutElement>().preferredHeight = 38;
            var rect = (RectTransform)root.transform;

            var label = Text("Label", rect, "", 18, TextAlignmentOptions.Left);
            Anchor(label.rectTransform, new Vector2(0, 0), new Vector2(0.62f, 1), new Vector4(10, 0, 4, 0));

            var input = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            input.name = "Input";
            input.transform.SetParent(rect, false);
            Anchor((RectTransform)input.transform, new Vector2(0.62f, 0), new Vector2(1, 1), new Vector4(4, 3, 6, 3));
            var field = input.GetComponent<TMP_InputField>();
            foreach (var text in input.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = _font;
                text.fontSize = 18;
            }
            input.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.18f);
            var area = (RectTransform)input.transform.Find("Text Area");
            Anchor(area, Vector2.zero, Vector2.one, new Vector4(8, 2, 8, -2));
            foreach (var text in area.GetComponentsInChildren<TMP_Text>(true))
            {
                Anchor(text.rectTransform, Vector2.zero, Vector2.one, Vector4.zero);
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.enableWordWrapping = false;
            }
            area.Find("Placeholder").GetComponent<TMP_Text>().text = string.Empty;
            field.textComponent.color = Color.white;

            var dropdown = AddDropdown(rect);

            var view = root.AddComponent<SimLabFieldView>();
            var so = new SerializedObject(view);
            so.FindProperty("_label").objectReferenceValue = label;
            so.FindProperty("_input").objectReferenceValue = field;
            so.FindProperty("_button").objectReferenceValue = root.GetComponent<Button>();
            so.FindProperty("_dropdown").objectReferenceValue = dropdown;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, RowPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(RowPath, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<GameObject>(RowPath).GetComponent<SimLabFieldView>();
        }

        public static TMP_Dropdown AddDropdown(RectTransform row)
        {
            var font = _font != null ? _font : AssetDatabase.LoadAssetAtPath<GameObject>(FontSource).GetComponentInChildren<TMP_Text>(true).font;
            var dropdownObject = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
            dropdownObject.name = "Dropdown";
            dropdownObject.transform.SetParent(row, false);
            Anchor((RectTransform)dropdownObject.transform, new Vector2(0.62f, 0), new Vector2(1, 1), new Vector4(4, 3, 6, -3));
            dropdownObject.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.18f);
            foreach (var text in dropdownObject.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = font;
                text.fontSize = 18;
                text.color = Color.white;
            }
            foreach (var image in dropdownObject.transform.Find("Template").GetComponentsInChildren<Image>(true))
                if (image.name != "Item Checkmark") image.color = new Color(0.12f, 0.12f, 0.14f);
            dropdownObject.SetActive(false);
            return dropdownObject.GetComponent<TMP_Dropdown>();
        }

        private static RectTransform Panel(string name, RectTransform parent, Vector2 min, Vector2 max, Vector4 inset)
        {
            var panel = Rect(name, parent);
            panel.gameObject.AddComponent<Image>().color = new Color(0.11f, 0.1f, 0.09f, 0.96f);
            panel.anchorMin = min;
            panel.anchorMax = max;
            panel.offsetMin = new Vector2(inset.x, inset.y);
            panel.offsetMax = new Vector2(-inset.z, -inset.w);
            return panel;
        }

        private static RectTransform Scroll(string name, RectTransform parent, Vector4 inset, out ScrollRect scroll)
        {
            var area = Rect(name, parent);
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(inset.x, inset.y);
            area.offsetMax = new Vector2(-inset.z, -inset.w);
            scroll = area.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;

            var viewport = Rect("Viewport", area);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        private static Button Button(string name, RectTransform parent, string label, Color color)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = Text("Label", rect, label, 22, TextAlignmentOptions.Center);
            Anchor(text.rectTransform, Vector2.zero, Vector2.one, Vector4.zero);
            return button;
        }

        private static TMP_Text Text(string name, RectTransform parent, string value, float size, TextAlignmentOptions alignment)
        {
            var rect = Rect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(0.92f, 0.9f, 0.85f);
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector4 offsets)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = new Vector2(offsets.x, offsets.y);
            rect.offsetMax = new Vector2(-offsets.z, offsets.w);
        }
    }
}
