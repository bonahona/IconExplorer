using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Fyrvall.IconExplorer
{
    public class IconExplorerWindow : EditorWindow
    {
        const int DefaultButtonSize = 48;

        private static GUIStyle IconButtonStyle = null;
        private static GUIStyle PreviewIconStyle = null;


        private static readonly Vector2 MinWindowSize = new Vector2(300, 400);

        private static void CreateStyles()
        {
            IconButtonStyle = new GUIStyle(EditorStyles.miniButton);
            IconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            IconButtonStyle.fixedHeight = 0;

            PreviewIconStyle = new GUIStyle(IconButtonStyle);
        }

        [MenuItem("Window/Icons Explorer")]
        public static void ShowWindow()
        {
            CreateStyles();
            var window = GetWindow<IconExplorerWindow>("Icon Explorer");
            window.ShowUtility();
            window.minSize = MinWindowSize;
        }

        public string FilterString;
        public List<GUIContent> IconContent;
        public List<GUIContent> FilteredIconContent;

        private GUIContent SelectedIconContent;
        private Vector2 ListScrollViewOffset;

        private void DisplaySearchField()
        {
            var tmpFilterString = EditorGUILayout.TextField(FilterString, EditorStyles.toolbarSearchField);

            if (tmpFilterString != FilterString) {
                UpdateFilter(tmpFilterString);
                FilterString = tmpFilterString;
            }
        }

        private void UpdateFilter(string filterString)
        {
            FilteredIconContent = FilterObjects(IconContent, filterString);
        }

        public List<GUIContent> FilterObjects(List<GUIContent> startCollection, string filter)
        {
            var result = startCollection.ToList();

            if (filter != string.Empty) {
                result = result.Where(o => o.tooltip.ToLower().Contains(filter.ToLower())).ToList();
            }

            return result;
        }

        GUIContent GetIcon(string icon_name)
        {
            GUIContent valid = null;
            Debug.unityLogger.logEnabled = false;
            if (!string.IsNullOrEmpty(icon_name)) valid = EditorGUIUtility.IconContent(icon_name);
            Debug.unityLogger.logEnabled = true;
            return valid?.image == null ? null : valid;
        }

        private void OnEnable()
        {
            InitIcons();
        }

        private void OnGUI()
        {
            if(IconButtonStyle == null) {
                CreateStyles();
            }

            var ppp = EditorGUIUtility.pixelsPerPoint;

            EditorGUILayout.LabelField("Found " + FilteredIconContent.Count());
            DisplaySearchField();

            using (var scrollScope = new GUILayout.ScrollViewScope(ListScrollViewOffset)) {
                ListScrollViewOffset = scrollScope.scrollPosition;

                // scrollbar_width = ~ 12.5
                var render_width = (Screen.width / ppp - 13f);
                var gridW = Mathf.FloorToInt(render_width / DefaultButtonSize);
                var margin_left = (render_width - DefaultButtonSize * gridW) / 2;

                int row = 0, index = 0;

                while (index < FilteredIconContent.Count) {
                    using (new GUILayout.HorizontalScope()) {
                        GUILayout.Space(margin_left);

                        for (var i = 0; i < gridW; ++i) {
                            int k = i + row * gridW;

                            var icon = FilteredIconContent[k];

                            if (GUILayout.Button(icon,
                                IconButtonStyle,
                                GUILayout.Width(DefaultButtonSize),
                                GUILayout.Height(DefaultButtonSize))) {
                                EditorGUI.FocusTextInControl("");
                                SelectedIconContent = icon;
                            }

                            index++;

                            if (index == FilteredIconContent.Count) break;
                        }
                    }

                    row++;
                }

                GUILayout.Space(10);
            }


            if (SelectedIconContent == null) return;

            GUILayout.FlexibleSpace();

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MaxHeight(120))) {
                using (new GUILayout.VerticalScope(GUILayout.Width(130))) {
                    GUILayout.Space(2);
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(10);

                using (new GUILayout.VerticalScope()) {
                    var s = $"Size: {SelectedIconContent.image.width}x{SelectedIconContent.image.height}";
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(s, MessageType.None);
                    GUILayout.Space(5);
                    EditorGUILayout.TextField($"Icon path: {SelectedIconContent.tooltip}");
                    GUILayout.Space(5);
                    if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton)) {
                        EditorGUIUtility.systemCopyBuffer = SelectedIconContent.tooltip;
                    }
                }
            }
        }

        void AllTheTEXTURES(ref GUIStyle s, Texture2D t)
        {
            s.hover.background = s.onHover.background = s.focused.background = s.onFocused.background = s.active.background = s.onActive.background = s.normal.background = s.onNormal.background = t;
            s.hover.scaledBackgrounds = s.onHover.scaledBackgrounds = s.focused.scaledBackgrounds = s.onFocused.scaledBackgrounds = s.active.scaledBackgrounds = s.onActive.scaledBackgrounds = s.normal.scaledBackgrounds = s.onNormal.scaledBackgrounds = new Texture2D[] { t };
        }

        Texture2D Texture2DPixel(Color c)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        void InitIcons()
        {
            AllTheTEXTURES(ref PreviewIconStyle, Texture2DPixel(new Color(0.15f, 0.15f, 0.15f)));

            IconContent = new List<GUIContent>();

            for (var i = 0; i < IconExploresUtils.IconPaths.Count; ++i) {
                GUIContent ico = GetIcon(IconExploresUtils.IconPaths[i]);

                if (ico == null) {
                    continue;
                }

                ico.tooltip = IconExploresUtils.IconPaths[i];
                IconContent.Add(ico);
            }

            FilteredIconContent = new List<GUIContent>(IconContent);
        }
    }
}