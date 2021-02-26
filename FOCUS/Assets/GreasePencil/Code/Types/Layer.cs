using UnityEngine;
using System.Collections.Generic;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace GreasePencil
{
    [System.Serializable]
    public class Layer : ISerializationCallbackReceiver
    {
        public const int CURRENT_VERSION = 1;
        public static Layer New(string name)
        {
            var layer = new Layer
            {
                name = name
            };

            layer.version = CURRENT_VERSION;
            return layer;
        }

        Layer()
        {
            _strokes = new List<Stroke>(); // init
            visible = true;
            xray = true;
            width = 4f;
        }

        [SerializeField] [HideInInspector] ulong _id;
        public ulong Id { get { return _id; } }

        public string name;
        public bool visible;

        [SerializeField, RenderQueue()]
        int _renderQueue = -1;
        public int RenderQueue { get { return _renderQueue; } }

        //[HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("strokes")]
        [SerializeField]
        List<Stroke> _strokes;
        public List<Stroke> Strokes { get { return _strokes; } }

        [System.NonSerialized]
        List<Stroke> _modifiedStrokes;

        public List<Stroke> RenderStrokes { get { return _modifiedStrokes ?? _strokes; } }

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("strokeColor")]
        Color _lineColor = Color.black;
        public Color LineColor { get { return _lineColor; } }

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("fillColor")]
        Color _fillColor = Color.clear;
        public Color FillColor { get { return _fillColor; } }

        [SerializeField]
        Mesh _mesh;
        public Mesh Mesh { get { return _mesh; } set { _mesh = value; } }

        public enum BlendOp
        {
            UseStrokeColor,
            UseLayerColor,
            Multiply,
            Add
        }

        [SerializeField]
        BlendOp _blendMode = BlendOp.Multiply;

        [Range(1f, 15f)]
        public float width = 1f;
        public bool xray;

        [SerializeField]
        List<LayerModifier> _modifiers = new List<LayerModifier>(); // init
        public List<LayerModifier> Modifiers { get { return _modifiers; } set { _modifiers = value; } }

        [SerializeField]
        int version = 0;

        bool _dirty;

        Stroke currentStroke;
        public Stroke CurrentStroke { get { return currentStroke; } }

        void ValidateId()
        {
            if (_id == 0)
                _id = System.BitConverter.ToUInt64(System.Guid.NewGuid().ToByteArray(), 0);

            if (version < CURRENT_VERSION)
            {
                Debug.LogFormat("Updating Version {0} to {1}", version, CURRENT_VERSION);

                // v1 Adds per stroke coloration
                if (version == 0)
                {
                    foreach (var s in _strokes)
                        s.SetColors(Color.white, Color.white);

                    _dirty = true;
                }

                version = CURRENT_VERSION;
            }
        }

        public Color ColorOp(Color layerColor, Color strokeColor)
        {
            switch (_blendMode)
            {
            case BlendOp.UseStrokeColor:
                return strokeColor;
            case BlendOp.UseLayerColor:
                return layerColor;
            case BlendOp.Multiply:
                return strokeColor * layerColor;
            case BlendOp.Add:
                return strokeColor + layerColor;
            }

            return strokeColor * layerColor;
        }

        public void StartStroke(Point worldPoint, float worldSize, Transform basisTransform, Transform mainTransform, Color lineColor, Color fillColor)
        {
            currentStroke = Stroke.New(worldPoint,
                mainTransform.InverseTransformDirection(basisTransform.right),
                mainTransform.InverseTransformDirection(basisTransform.up),
                lineColor, fillColor);
            _strokes.Add(currentStroke);
        }

        public void UpdateStroke(Point worldPoint)
        {
            currentStroke.LastPoint = worldPoint;

            if (currentStroke.LastDistance() > 0.03f)
                currentStroke.Add(worldPoint, false);

            currentStroke.Triangulate();
            ModifyStrokes();
        }

        public void EndStroke(Point worldPoint)
        {
            if (!currentStroke.Finalized)
            {
                currentStroke.LastPoint = worldPoint;
                currentStroke.Triangulate();
                currentStroke.CloseStroke();
                StrokeClosed();
            }
        }

        public void EndStroke()
        {
            if (!currentStroke.Finalized)
            {
                currentStroke.Triangulate();
                currentStroke.CloseStroke();
                StrokeClosed();
            }
        }

        void StrokeClosed()
        {
            ModifyStrokes();
        }

        void ModifyStrokes()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Grease Pencil Modify strokes");

            if (_modifiedStrokes == null)
                _modifiedStrokes = new List<Stroke>(); // init if null

            // reset the modified strokes to the original strokes
            Stroke.ReleaseStrokes(_modifiedStrokes);
            _strokes.RemoveAll(Stroke.RemoveEmptyStrokes);

            foreach (var stroke in _strokes)
                _modifiedStrokes.Add(Stroke.New(stroke));

            if (_modifiers == null)
                return;

            foreach (var modifier in _modifiers)
                modifier.ModifyStroke(this, _modifiedStrokes);

            _modifiedStrokes.RemoveAll(Stroke.RemoveEmptyStrokes);

            TriangulateStroke();

            _dirty = false;

            UnityEngine.Profiling.Profiler.EndSample();
        }

        private void TriangulateStroke()
        {
            foreach (var stroke in _modifiedStrokes)
                stroke.Triangulate();
        }

        public void RefreshStrokes()
        {
            ModifyStrokes();
        }

        public void RefreshStrokesIfDirty()
        {
            if (_dirty)
                ModifyStrokes();
        }

        public bool IsStrokeFinalized { get { return (currentStroke == null) || currentStroke.Finalized; } }

        public void CollapseModifierStack(bool keepModifiers = false)
        {
            _strokes.Clear();
            _strokes.AddRange(_modifiedStrokes);
            _modifiedStrokes.Clear();

            if (!keepModifiers)
            {
                if (Application.isPlaying)
                    _modifiers.ForEach(m => Object.Destroy(m.modifier));
                else
                    _modifiers.ForEach(m => Object.DestroyImmediate(m.modifier));

                _modifiers.Clear();
            }

            RefreshStrokes();
        }

        public void FlattenLayer()
        {
            for (int i = 0; i < _strokes.Count; ++i)
            {
                _strokes[i].SetColors(ColorOp(LineColor, _strokes[i].LineColor),
                    ColorOp(FillColor, _strokes[i].FillColor));
            }

            _fillColor = Color.white;
            _lineColor = Color.white;
        }

        public void Merge(Layer otherLayer)
        {
            otherLayer.CollapseModifierStack();
            otherLayer.FlattenLayer();
            _strokes.AddRange(otherLayer._strokes);
        }

        public void Remove(int stroke, List<int> points)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(stroke >= 0 && stroke < _strokes.Count);
#endif
            Stroke currentStroke = _strokes[stroke];

            points.Sort();
            points.Reverse();

            foreach (var p in points)
                currentStroke.MarkPointForRemoval(p);

            currentStroke.Points.RemoveAll(p => p.markedForRemoval);
            RefreshStrokes();

            _strokes.RemoveAll(Stroke.RemoveEmptyStrokes);
            _strokes.ForEach(s => s.CloseStroke());
        }

        public void Erase(int stroke, List<int> points)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(stroke >= 0 && stroke < _strokes.Count);
#endif
            Stroke oldStroke = _strokes[stroke];
            Stroke newStroke = null;
            List<Stroke> newStrokes;

            using (PooledList<Stroke>.Get(out newStrokes))
            {
                for (int i = 0; i < oldStroke.Points.Count; ++i)
                {
                    if (!points.Contains(i))
                    {
                        if (newStroke == null)
                        {
                            newStroke = Stroke.New(oldStroke.Right, oldStroke.Up);
                            newStrokes.Add(newStroke);
                            newStroke.SetColors(oldStroke.LineColor, oldStroke.FillColor);
                        }

                        newStroke.Add(oldStroke.Points[i], false);
                    }
                    else if (newStroke != null)
                        newStroke = null;
                }

                if (newStrokes.Count > 0)
                {
                    _strokes[stroke] = newStrokes[0];

                    for (int i = 1; i < newStrokes.Count; ++i)
                        _strokes.Add(newStrokes[i]);
                }
                else
                    _strokes[stroke].Clear();
            }

            _strokes.RemoveAll(Stroke.RemoveEmptyStrokes);
            _strokes.ForEach(s => s.CloseStroke());

            RefreshStrokes();
        }

        public void RemoveModifier(int index)
        {
            Object.DestroyImmediate(_modifiers[index].modifier);
            _modifiers.RemoveAt(index);
        }

        public void AddModifier()
        {
            _modifiers.Add(new LayerModifier());
        }

        public void OnBeforeSerialize()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Serialize Grease Pencil Layer");
            ValidateId();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void OnAfterDeserialize()
        {
            ValidateId();
            _dirty = true;
        }
    }
}