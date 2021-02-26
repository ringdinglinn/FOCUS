using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace GreasePencil
{
    public class EditorState : ScriptableObject
    {
        static EditorState _current;
        public static EditorState Current { get { return GetCurrent(); } }

        static SerializedObject _currentSerialized;
        public static SerializedObject CurrentSerialized { get { return GetCurrentSerialized(); } }

        public const HideFlags EditorHideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;

        [System.Serializable]
        public class Palette
        {
            public Palette()
            {
                string json = EditorPrefs.GetString("GP_palette", string.Empty);
                if (!string.IsNullOrEmpty(json))
                    EditorJsonUtility.FromJsonOverwrite(json, this);

                if (colors == null)
                    colors = new List<Color>() { Color.black, Color.white, Color.grey, Color.red, Color.green, Color.blue };
            }

            public void Save()
            {
                EditorPrefs.SetString("GP_palette", EditorJsonUtility.ToJson(this));
                Debug.Log(EditorPrefs.GetString("GP_palette"));
            }

            public void AddColor(Color color)
            {
                if (!colors.Contains(color))
                {
                    colors.Add(color);
                    Save();
                }
            }

            public void RemoveColor(int index)
            {
                colors.RemoveAt(index);
                Save();
            }

            public List<Color> colors;
        }

        private static EditorState GetCurrent()
        {
            if (_current == null)
                _current = CreateInstance<EditorState>();

            if (EditorHideFlags != _current.hideFlags)
                _current.hideFlags = EditorHideFlags;

            if (_currentSerialized == null)
                _currentSerialized = new SerializedObject(_current);

            return _current;
        }

        private static SerializedObject GetCurrentSerialized()
        {
            if ((_currentSerialized == null) || (_current == null))
                GetCurrent();
            return _currentSerialized;
        }

        public static float EraserSize
        {
            get { return EditorPrefs.GetFloat("GP_eraserSize", 50f); }
            set { EditorPrefs.SetFloat("GP_eraserSize", value); }
        }

        static Palette _palette;
        public static Palette CurrentPalette
        {
            get
            {
                if (_palette == null)
                    _palette = new Palette();
                return _palette;
            }
        }

        /// <summary>
        /// The list of objects not to test against because they don't have mesh filters
        /// </summary>
        public GameObject[] ignoreObjects;

        /// <summary>
        /// The current Grease Pencil canvas
        /// </summary>
        [SerializeField] GreasePencilCanvas _instance;
        public GreasePencilCanvas Instance
        {
            get { return _instance; }
            set
            {
                if ((_instance != value) || ((_instance != null) && (_serializedObject == null)))
                {
                    _instance = value;
                    if (_instance != null)
                        _serializedObject = new SerializedObject(_instance);
                    else
                        _serializedObject = null;

                    StateDirty();
                }
            }
        }

        /// <summary>
        /// The current drawing canvas method
        /// </summary>
        public int drawMode;

        public bool drawing;
        public bool softErase;
        public bool erase;
        public bool keepModifiers;

        public Camera camera;
        public Color lineColor = Color.white;
        public Color fillColor = Color.white;

        SerializedObject _serializedObject;
        public SerializedObject serializedObject
        {
            get
            {
                if ((_serializedObject == null) && (_instance != null))
                    _serializedObject = new SerializedObject(_instance);
                return _serializedObject;
            }
        }

        [System.NonSerialized]
        List<DrawModeImpl> _modes = new List<DrawModeImpl>();
        public DrawModeImpl CurrentDrawMode { get { return GetMode(drawMode); } }

        [System.NonSerialized]
        List<string> _drawModeNames = new List<string>();
        public List<string> DrawModeNames { get { return _drawModeNames; } }

        public static UnityAction onStateChanged;
        public void StateDirty() { if (onStateChanged != null) onStateChanged.Invoke(); }

        private DrawModeImpl GetMode(int newDrawMode)
        {
            drawMode = newDrawMode;

            if (_modes.Count <= drawMode)
            {
                GetDerivedClasses(_modes);
                _modes.RemoveAll(m => m.Order < 0);
            }

            drawMode = Mathf.Clamp(drawMode, 0, _modes.Count - 1);

            _drawModeNames.Clear();
            foreach (var mode in _modes)
                _drawModeNames.Add(mode.Name);

            return _modes[drawMode];
        }

        public static bool GetDerivedClasses<T>(List<T> result)
        {
            result.Clear();
            var baseType = typeof(T);

            foreach (System.Type type in System.Reflection.Assembly.GetAssembly(baseType).GetTypes())
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType))
                    result.Add((T)System.Activator.CreateInstance(type, null));

            result.Sort();
            return result.Count > 0;
        }

        void OnValidate()
        {
            if (_current == null)
            {
                //Debug.Log("Got Current");
                _current = this;
            }
        }
    }
}