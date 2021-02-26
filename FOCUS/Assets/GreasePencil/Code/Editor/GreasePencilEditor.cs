using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace GreasePencil
{
    [CustomEditor(typeof(GreasePencilCanvas))]
    public partial class GreasePencilEditor : Editor
    {

        ReorderableList _layerList;
        SerializedProperty _layers;
        SerializedProperty _drawInGameCameras;
        SerializedProperty _drawByStrokeOrder;

        void OnEnable()
        {
            EditorState.Current.Instance = target as GreasePencilCanvas;
            EditorState.Current.Instance.Retriangulate();

            _layers = serializedObject.FindProperty("_layers");
            _drawInGameCameras = serializedObject.FindProperty("_drawInGameCameras");
            _drawByStrokeOrder = serializedObject.FindProperty("_drawByStrokeOrder");

            _layerList = new ReorderableList(serializedObject, _layers);

            // restore last selection
            _layerList.index = EditorState.Current.Instance.ActiveLayerIndex;
            _layerList.drawElementCallback += DrawLayerElement;
            _layerList.elementHeightCallback += LayerElementHeight;
            _layerList.drawHeaderCallback += DrawLayerHeader;
            _layerList.onSelectCallback += SelectLayer;
            _layerList.onAddCallback += AddLayer;
            _layerList.onRemoveCallback += RemoveLayer;
#if UNITY_2017
            _layerList.onReorderCallback += ReorderLayers;
#else
            _layerList.onReorderCallbackWithDetails += ReorderLayers;
#endif

            List<GameObject> gos = new List<GameObject>(FindObjectsOfType<GameObject>());
            using (PooledList<GameObject>.Get(out gos))
            {
                gos.AddRange(FindObjectsOfType<GameObject>());
                EditorState.Current.ignoreObjects = gos.FindAll(go => go.GetComponent<MeshFilter>() == null).ToArray();
            }
        }

        void OnDisable()
        {
            EditorState.Current.ignoreObjects = null;
            EditorState.Current.drawing = false;

            if (EditorState.Current.Instance == target)
                EditorState.Current.Instance = null;

            if (Tools.hidden)
                Tools.hidden = false;
        }

        private void ReorderLayers(ReorderableList list, int oldIndex, int newIndex)
        {
            serializedObject.ApplyModifiedProperties();
            _layers.MoveArrayElement(oldIndex, newIndex);
            // why do I need to to this?
            EditorState.Current.Instance.Retriangulate();
            serializedObject.Update();
        }

        private void ReorderLayers(ReorderableList list)
        {
            EditorState.Current.Instance.Retriangulate();
            serializedObject.Update();
        }

        private void AddLayer(ReorderableList list)
        {
            serializedObject.ApplyModifiedProperties();
            EditorState.Current.Instance.AddLayer("Layer");
            serializedObject.Update();
            EditorState.Current.drawing = true;
        }

        private void RemoveLayer(ReorderableList list)
        {
            serializedObject.ApplyModifiedProperties();
            EditorState.Current.Instance.RemoveLayer(list.index);
            serializedObject.Update();
            list.index = list.serializedProperty.arraySize - 1;
            serializedObject.Update();
        }

        private void SelectLayer(ReorderableList list)
        {
            serializedObject.Update();
            serializedObject.FindProperty("_activeLayerIndex").intValue = list.index;
            serializedObject.ApplyModifiedProperties();
            EditorState.Current.StateDirty();
        }

        private void DrawLayerHeader(Rect rect)
        {
            GUI.Label(rect, "Layers");
        }

        private float LayerElementHeight(int index)
        {
            var property = _layers.GetArrayElementAtIndex(index);

            if (property.isExpanded)
                return (EditorGUIUtility.singleLineHeight + 2) * 4 + 2;
            else
                return (EditorGUIUtility.singleLineHeight + 2);
        }

        private void DrawLayerElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var property = _layers.GetArrayElementAtIndex(index);
            var lineRect = new Rect(rect);
            lineRect.height = EditorGUIUtility.singleLineHeight;

            // first line
            var temp = new Rect(lineRect);
            temp.xMax = temp.xMin + EditorGUIUtility.singleLineHeight;

            // draw foldout
            property.isExpanded = EditorGUI.Toggle(temp, property.isExpanded, EditorStyles.foldout);

            // draw visiblity
            temp.x += EditorGUIUtility.singleLineHeight;
            var prop = property.FindPropertyRelative("visible");
            prop.boolValue = EditorGUI.Toggle(temp, prop.boolValue, "VisibilityToggle");

            temp = new Rect(lineRect);
            temp.xMin += EditorGUIUtility.singleLineHeight * 2;
            EditorGUI.PropertyField(temp, property.FindPropertyRelative("name"), GUIContent.none);

            if (property.isExpanded)
            {
                lineRect.y += EditorGUIUtility.singleLineHeight + 2;

                Rect lineColor = new Rect(lineRect);
                lineColor.xMax = lineColor.xMin + lineRect.width * 0.33f;

                // second line
                EditorGUIUtility.labelWidth = 60f;
                EditorGUI.PropertyField(lineColor, property.FindPropertyRelative("_lineColor"), Styles.Content("Line"));
                lineColor.x += lineColor.width;
                EditorGUI.PropertyField(lineColor, property.FindPropertyRelative("_fillColor"), Styles.Content("Fill"));
                lineColor.x += lineColor.width;
                EditorGUI.PropertyField(lineColor, property.FindPropertyRelative("_blendMode"), Styles.Content("Mode"));

                EditorGUIUtility.labelWidth = -1f;

                // third line
                EditorGUIUtility.labelWidth = 80f;
                lineRect.y += EditorGUIUtility.singleLineHeight + 2;
                temp = new Rect(lineRect);

                temp.width = lineRect.width - 70f;
                EditorGUI.PropertyField(temp, property.FindPropertyRelative("width"));

                EditorGUIUtility.labelWidth = 40f;

                temp.xMax = lineRect.xMax;
                temp.xMin = temp.xMax - 60;
                EditorGUI.PropertyField(temp, property.FindPropertyRelative("xray"));

                // Render queue
                lineRect.y += EditorGUIUtility.singleLineHeight + 2;
                temp = new Rect(lineRect);
                EditorGUI.PropertyField(temp, property.FindPropertyRelative("_renderQueue"), GUIContent.none);
            }

            EditorGUIUtility.labelWidth = -1f;
        }

        Rect _paletteRect;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Separator();

            if (EditorState.Current.Instance == null)
                EditorState.Current.Instance = target as GreasePencilCanvas;

            if (EditorState.Current.Instance == null)
                return;

            if (EditorState.Current.Instance.gameObject.scene.IsValid())
                EditorState.Current.drawing = GUILayout.Toggle(EditorState.Current.drawing, "Draw", "Button");
            else if (EditorState.Current.drawing)
                EditorState.Current.drawing = false;

            if (EditorState.Current.drawing)
            {
                EditorState.EraserSize = EditorGUILayout.Slider("Eraser Size", EditorState.EraserSize, 10f, 100f);

                var serializedState = EditorState.CurrentSerialized;
                serializedState.Update();

                SerializedProperty fillColor = serializedState.FindProperty("fillColor");
                SerializedProperty lineColor = serializedState.FindProperty("lineColor");

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(lineColor);
                    EditorGUILayout.PropertyField(fillColor);
                }

                serializedState.ApplyModifiedProperties();

                GUILayout.Label("Palette");

                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    var palette = EditorState.CurrentPalette;
                    var colorCount = palette.colors.Count + 1;
                    var w = EditorGUIUtility.currentViewWidth;
                    var hCount = Mathf.FloorToInt(w / EditorGUIUtility.singleLineHeight);
                    var vCount = Mathf.CeilToInt((float)colorCount / hCount);

                    Rect rect = GUILayoutUtility.GetRect(hCount * EditorGUIUtility.singleLineHeight, vCount * EditorGUIUtility.singleLineHeight);

                    var evt = Event.current;

                    var colorRect = new Rect(rect);
                    colorRect.width = EditorGUIUtility.singleLineHeight;
                    colorRect.height = EditorGUIUtility.singleLineHeight;

                    for (int i = 0; i < palette.colors.Count; ++i)
                    {
                        if (i % hCount == 0)
                            colorRect.x = rect.x;

                        Rect t = new Rect(colorRect);

                        float h, s, v;
                        Color.RGBToHSV(palette.colors[i], out h, out s, out v);

                        v = (v > 0.25f) ? 0f : 1f;

                        GUI.color = new Color(v, v, v, 0.5f);
                        t.x += 1f; t.y += 1f; t.width -= 2f; t.height -= 2f;
                        GUI.DrawTexture(t, Texture2D.whiteTexture);

                        GUI.color = palette.colors[i];
                        t.x += 1f; t.y += 1f; t.width -= 2f; t.height -= 2f;
                        GUI.DrawTexture(t, Texture2D.whiteTexture);

                        if (colorRect.Contains(evt.mousePosition))
                        {
                            if (evt.type == EventType.MouseDown)
                            {
                                if (evt.button == 0)
                                {
                                    serializedState.Update();

                                    if (evt.shift)
                                        fillColor.colorValue = palette.colors[i];
                                    else
                                        lineColor.colorValue = palette.colors[i];

                                    serializedState.ApplyModifiedProperties();
                                }
                                else if (evt.button == 1)
                                    palette.RemoveColor(i);

                                evt.Use();
                            }
                        }

                        if ((i + 1) % hCount == 0)
                            colorRect.y += colorRect.height;
                        colorRect.x += colorRect.width;
                    }

                    GUI.color = Color.white;

                    if (GUI.Button(colorRect, GUIContent.none, Styles.Add))
                    {
                        if (evt.shift)
                            palette.AddColor(fillColor.colorValue);
                        else
                            palette.AddColor(lineColor.colorValue);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("LMB - Set Line Color", Styles.FooterStyle);
                        GUILayout.Label("Shift-LMB - Set Fill Color", Styles.FooterStyle);
                        GUILayout.Label("RMB - Remove Color", Styles.FooterStyle);
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    SerializedProperty drawMode = serializedState.FindProperty("drawMode");
                    var newDrawMode = EditorGUILayout.Popup("Draw Mode", drawMode.intValue, EditorState.Current.DrawModeNames.ToArray());

                    if (check.changed)
                    {
                        drawMode.intValue = newDrawMode;
                        serializedState.ApplyModifiedProperties();
                    }
                }

                EditorGUILayout.HelpBox(EditorState.Current.CurrentDrawMode.Tooltip, MessageType.Info);

                GUILayout.Label("Shift+LMB-Drag to Erase", EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_drawInGameCameras, true);
            EditorGUILayout.PropertyField(_drawByStrokeOrder, true);

            EditorGUILayout.Separator();
            _layerList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            if (EditorState.Current.drawing)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.IntSlider(serializedObject.FindProperty("_activeLayerIndex"), 0, serializedObject.FindProperty("_layers").arraySize - 1);
                    if (check.changed)
                        _layerList.index = serializedObject.FindProperty("_activeLayerIndex").intValue;
                }
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Advanced Editor >"))
                GreasePencilAdvancedEditor.Open();
        }

        public void SceneLabel(string label)
        {
            GUIContent contentLabel = new GUIContent(label);

            var size = Styles.LabelStyle.CalcSize(contentLabel);

            Handles.BeginGUI();
            GUI.contentColor = Color.white;
            GUI.Box(new Rect(Screen.width - (size.x + 20f), Screen.height - (size.y + 20f) - 37f, size.x + 10f, size.y + 10f), contentLabel, Styles.LabelStyle);
            Handles.EndGUI();
        }

        public void SceneLayerLabel(string label)
        {
            GUIContent contentLabel = new GUIContent(label);

            var size = Styles.LargeLabelStyle.CalcSize(contentLabel);

            Handles.BeginGUI();
            GUI.contentColor = Color.white;
            GUI.Box(new Rect(Screen.width - (size.x + 20f), Screen.height - (size.y + 40f) - 37f, size.x + 10f, size.y + 10f), contentLabel, Styles.LargeLabelStyle);
            Handles.EndGUI();
        }

        public void SceneLeftLabel(string label, int index)
        {
            GUIContent contentLabel = new GUIContent(label);

            var size = Styles.InfoStyle.CalcSize(contentLabel);

            Handles.BeginGUI();
            GUI.contentColor = Color.white;
            GUI.Box(new Rect(20f, Screen.height - (size.y * (index + 1) + 20f) - 37f, size.x + 10f, size.y + 10f), contentLabel, Styles.InfoStyle);
            Handles.EndGUI();
        }

        List<string> _infos = new List<string>();

        void OnSceneGUI()
        {
            Event evt = Event.current;

            if ((evt.type == EventType.KeyUp) && (evt.keyCode == KeyCode.D) && evt.shift && !evt.control && !evt.alt)
            {
                evt.Use();
                EditorState.Current.drawing = !EditorState.Current.drawing;
            }

            if (Tools.hidden != EditorState.Current.drawing)
                Tools.hidden = EditorState.Current.drawing;

            if (!EditorState.Current.drawing)
                return;

            var b = new System.Text.StringBuilder("Grease Pencil Drawing - ");

            if (EditorState.Current.erase)
            {
                if (EditorState.Current.softErase)
                    b.Append("Soft ");
                b.Append("Erase");
            }
            else
                b.Append(EditorState.Current.CurrentDrawMode.Name);

            SceneLabel(b.ToString());
            SceneLayerLabel(EditorState.Current.Instance.ActiveLayer.name);

            _infos.Clear();
            EditorState.Current.CurrentDrawMode.GetInfos(_infos);
            _infos.Reverse();

            for (int i = 0; i < _infos.Count; ++i)
                SceneLeftLabel(_infos[i], i);

            // don't hijack camera controls
            if (evt.alt)
            {
                if (!EditorState.Current.Instance.StrokeFinalized())
                    EditorState.Current.Instance.EndStroke();

                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            EditorState.Current.camera = SceneView.currentDrawingSceneView.camera;
            EditorState.Current.CurrentDrawMode.OnSceneGUI(evt);
        }

        public static void GetErasePoints(Vector3 screenPosition, UnityAction<int, Vector3, float> pointAction, UnityAction<int> strokeAction)
        {
            Stroke stroke;

            var tm = EditorState.Current.Instance.transform.localToWorldMatrix;
            Vector3 worldPosition;

            for (int s = 0; s < EditorState.Current.Instance.ActiveLayer.Strokes.Count; ++s)
            {
                stroke = EditorState.Current.Instance.ActiveLayer.Strokes[s];

                for (int i = 0; i < stroke.Points.Count; ++i)
                {
                    worldPosition = tm.MultiplyPoint(stroke.Points[i].position);
                    var p = EditorState.Current.camera.WorldToScreenPoint(worldPosition) - screenPosition;
                    p.z = 0f;

                    if ((pointAction != null) && (p.magnitude < EditorState.EraserSize))
                        pointAction(i, worldPosition, 0.04f);
                }

                if (strokeAction != null)
                    strokeAction(s);
            }
        }

        public static bool PlaneCast(Vector3 planeCenter, Vector3 screenPosition, Camera cam, out GreasePencilHit worldHit)
        {
            return PlaneCast(planeCenter, -cam.transform.forward, screenPosition, cam, out worldHit);
        }

        public static bool PlaneCast(Vector3 planeCenter, Vector3 planeNormal, Vector3 screenPosition, Camera cam, out GreasePencilHit worldHit)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);

            float distance = 0f;

            if (new Plane(planeNormal, planeCenter).Raycast(ray, out distance))
            {
                worldHit = new GreasePencilHit(ray.GetPoint(distance), planeNormal, cam.transform.position);
                return true;
            }

            worldHit = new GreasePencilHit();
            return false;
        }

        public static bool PlaneCast(Plane plane, Vector3 screenPosition, Camera cam, out GreasePencilHit worldHit)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);
            float distance = 0f;

            if (plane.Raycast(ray, out distance))
            {
                worldHit = new GreasePencilHit(ray.GetPoint(distance), plane.normal * -Mathf.Sign(Vector3.Dot(ray.direction, plane.normal)), cam.transform.position);
                return true;
            }

            worldHit = new GreasePencilHit();
            return false;
        }

        private static float GetSize(Vector3 point)
        {
            var screenPoint = EditorState.Current.camera.WorldToScreenPoint(point);
            screenPoint.y += 1f;
            var worldPoint = EditorState.Current.camera.ScreenToWorldPoint(screenPoint);

            return Vector3.Distance(point, worldPoint);
        }
    }
}