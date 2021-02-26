using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GreasePencil
{
    public class GreasePencilAdvancedEditor : EditorWindow
    {
        private ReorderableList _modifierList;
        SerializedProperty _modifiers;

        public static void Open()
        {
            var window = GetWindow<GreasePencilAdvancedEditor>();
            window.titleContent = Styles.Content("Advanced Editor");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorState.onStateChanged += OnEditorStateChanged;
            GreasePencilModifier.onModifierUpdated += Repaint;

            Refresh();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorState.onStateChanged -= OnEditorStateChanged;
            GreasePencilModifier.onModifierUpdated -= Repaint;
        }

        private void OnGUI()
        {
            if (EditorState.Current.Instance == null)
            {
                GUI.contentColor = new Color(1f, 1f, 1f, 0.5f);
                GUILayout.Label("No active Grease Pencil. Select one to continue.", Styles.LargeLabelStyle);
                return;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Label(EditorState.Current.Instance.name, Styles.LargeLabelStyle);
                GUILayout.Label(EditorState.Current.Instance.ActiveLayer.name, Styles.LabelStyle);

                if (EditorState.Current.serializedObject == null)
                    return;

                EditorState.Current.serializedObject.Update();

                if (_modifierList != null)
                    _modifierList.DoLayoutList();
                else
                    GUILayout.Label("No modifier list", Styles.LabelStyle);

                if (check.changed)
                    EditorState.Current.serializedObject.ApplyModifiedProperties();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Collapse Modifier Stack"))
                    EditorState.Current.Instance.CollapseCurrentModifierStack(EditorState.Current.keepModifiers);

                EditorState.Current.keepModifiers = EditorGUILayout.ToggleLeft("Keep Modifiers", EditorState.Current.keepModifiers, GUILayout.Width(105f));
            }

            if (GUILayout.Button("Collapse Layers"))
                EditorState.Current.Instance.CollapseLayers();

            //Debug.Log(typeof(IInlineModifier).IsAssignableFrom(typeof(SmoothModifier)));
        }

        private void OnSelectionChanged()
        {
            Refresh();
            Repaint();
        }

        private void OnEditorStateChanged()
        {
            Refresh();
            Repaint();
        }

        private void Refresh()
        {
            if (Selection.activeGameObject != null)
            {
                var greasePencil = Selection.activeGameObject.GetComponent<GreasePencilCanvas>();
                if (greasePencil != null)
                    EditorState.Current.Instance = greasePencil;
            }

            if (EditorState.Current.serializedObject == null)
                return;

            EditorState.Current.serializedObject.Update();

            _modifiers = EditorState.Current.serializedObject.FindProperty("_layers")
                .GetArrayElementAtIndex(EditorState.Current.Instance.ActiveLayerIndex)
                .FindPropertyRelative("_modifiers");

            _modifierList = new ReorderableList(EditorState.Current.serializedObject, _modifiers)
            {
                index = EditorState.Current.Instance.ActiveLayerIndex // restore last selection
            };

            _modifierList.drawElementCallback += DrawModifierElement;
            _modifierList.elementHeightCallback += ModifierElementHeight;
            _modifierList.drawHeaderCallback += DrawModifierHeader;
            _modifierList.onSelectCallback += SelectModifier;
            //_modifierList.onAddCallback += AddModifier;
            _modifierList.onAddDropdownCallback += AddCallbackModifier;
            _modifierList.onRemoveCallback += RemoveModifier;
#if UNITY_2017
            _modifierList.onReorderCallback += ReorderModifier;
#else
            _modifierList.onReorderCallbackWithDetails += ReorderModifier;
#endif

        }

        public static bool GetDerivedClasses<T>(List<System.Type> result)
        {
            result.Clear();
            var baseType = typeof(T);

            var types = baseType.Assembly.GetTypes();
            foreach (var type in types)
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType))
                    result.Add(type);

            result.Sort((a, b) => a.Name.CompareTo(b.Name));
            return result.Count > 0;
        }

        private void AddCallbackModifier(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            var types = new List<System.Type>();
            if (GetDerivedClasses<GreasePencilModifier>(types))
            {
                for (int i = 0; i < types.Count; ++i)
                {
                    var type = types[i];
                    AddMenuItem(menu, i, type);
                }

                menu.ShowAsContext();
            }
        }

        private void AddMenuItem(GenericMenu menu, int index, System.Type type)
        {
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, o =>
            {
                _modifiers.serializedObject.Update();

                ++_modifiers.arraySize;
                var property = _modifiers.GetArrayElementAtIndex(_modifiers.arraySize - 1);

                property.FindPropertyRelative("_enabled").boolValue = true;
                property.FindPropertyRelative("_modifier._modifier").objectReferenceValue = null;
                property.FindPropertyRelative("_modifier._modifierType").stringValue = type.Name;
                property.FindPropertyRelative("_modifier._modifierName").stringValue = ObjectNames.NicifyVariableName(type.Name);

                _modifiers.serializedObject.ApplyModifiedProperties();
            }, index);
        }

        private void DrawModifierElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _modifiers.GetArrayElementAtIndex(index);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                rect.y += 3f;
                rect.height -= 6f;

                var prop = element.FindPropertyRelative("_enabled");

                var toggle = new Rect(rect);
                toggle.width = EditorGUIUtility.singleLineHeight;
                toggle.height = EditorGUIUtility.singleLineHeight;
                rect.xMin += EditorGUIUtility.singleLineHeight + 4;

                prop.boolValue = EditorGUI.Toggle(toggle, prop.boolValue);

                var modifierObject = element.FindPropertyRelative("_modifier._modifier").objectReferenceValue;

                if (modifierObject == null) return;

                var modifierName = element.FindPropertyRelative("_modifier._modifierName");
                var modifierType = ObjectNames.NicifyVariableName(element.FindPropertyRelative("_modifier._modifierType").stringValue);

                var ser = new SerializedObject(modifierObject);

                ser.Update();

                SerializedProperty iterator = ser.GetIterator();
                bool enterChildren = true;
                float h = 0;
                Rect propRect = new Rect(rect);

                using (var checkModifier = new EditorGUI.ChangeCheckScope())
                {
                    while (iterator.NextVisible(enterChildren))
                    {
                        h = EditorGUI.GetPropertyHeight(iterator, iterator.isExpanded);
                        propRect.height = h;
                        if (iterator.name == "m_Script")
                        {
                            var nameRect = new Rect(propRect);
                            nameRect.width -= 110f;

                            var typeRect = new Rect(propRect);
                            typeRect.xMin = typeRect.xMax - 100f;

                            EditorGUI.PropertyField(nameRect, modifierName, GUIContent.none, iterator.isExpanded);
                            GUI.Label(typeRect, modifierType, EditorStyles.miniBoldLabel);
                        }
                        else
                            EditorGUI.PropertyField(propRect, iterator, iterator.isExpanded);

                        propRect.y = propRect.yMax;
                        enterChildren = false;
                    }

                    if (checkModifier.changed)
                        ser.ApplyModifiedProperties();
                }
            }
        }

        private float ModifierElementHeight(int index)
        {
            var element = _modifiers.GetArrayElementAtIndex(index);

            var modifierObject = element.FindPropertyRelative("_modifier._modifier").objectReferenceValue;

            if (modifierObject == null) return 0;

            var ser = new SerializedObject(modifierObject);

            SerializedProperty iterator = ser.GetIterator();
            bool enterChildren = true;
            float h = 0;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                h += EditorGUI.GetPropertyHeight(iterator, iterator.isExpanded);
            }

            return h + 10;
        }

        private void DrawModifierHeader(Rect rect) { }

        private void SelectModifier(ReorderableList list) { }

        private void AddModifier(ReorderableList list)
        {
            EditorState.Current.serializedObject.ApplyModifiedProperties();
            EditorState.Current.Instance.ActiveLayer.AddModifier();
            EditorState.Current.serializedObject.Update();
            _modifierList.serializedProperty = _modifiers;
        }

        private void RemoveModifier(ReorderableList list)
        {
            EditorState.Current.serializedObject.ApplyModifiedProperties();
            EditorState.Current.Instance.ActiveLayer.RemoveModifier(list.index);
            EditorState.Current.serializedObject.Update();
            _modifierList.serializedProperty = _modifiers;
        }

        private void ReorderModifier(ReorderableList list, int oldIndex, int newIndex)
        {
            EditorState.Current.serializedObject.ApplyModifiedProperties();
            _modifiers.MoveArrayElement(oldIndex, newIndex);
            EditorState.Current.serializedObject.Update();
        }

        private void ReorderModifier(ReorderableList list)
        {
            EditorState.Current.Instance.Retriangulate();
            list.serializedProperty.serializedObject.Update();
        }
    }
}