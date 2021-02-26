using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_ASSERTIONS
#endif

namespace GreasePencil
{
    public interface IGreasePencilLayerListener
    {
        void OnGreasePencilLayerChanged(GreasePencilCanvas greasePencil, Layer layer);
        void OnGreasePencilLayerAdded(GreasePencilCanvas greasePencil, Layer layer);
        void OnGreasePencilLayerRemoved(GreasePencilCanvas greasePencil, Layer layer);
    }

    public interface IGreasePencilStrokeListener
    {
        void OnGreasePencilStrokeChanged(Layer layer, Stroke stroke);
    }

    [ExecuteInEditMode]
    public class GreasePencilCanvas : MonoBehaviour
    {
        int _zTestParam;
        static Material _defaultGreasePencilMaterial = null;
        static Material _defaultGreasePencilXrayMaterial = null;

        Material _greasePencilMaterial = null;
        Material GreasePencilMaterial
        {
            get
            {
                if (_greasePencilMaterial == null)
                {
                    if (_defaultGreasePencilMaterial == null)
                    {
                        _defaultGreasePencilMaterial = new Material(Shader.Find("Hidden/GreasePencil"));
                        _defaultGreasePencilMaterial.name = "Grease Pencil Material";
                        _defaultGreasePencilMaterial.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                        _defaultGreasePencilMaterial.SetFloat("_ZWrite", 1f);
                    }
                    _defaultGreasePencilMaterial.SetInt("_ZTest", (int)CompareFunction.LessEqual);
                    _greasePencilMaterial = _defaultGreasePencilMaterial;
                }

                return _greasePencilMaterial;
            }
        }

        Material _greasePencilXrayMaterial = null;
        Material GreasePencilMaterialXray
        {
            get
            {
                if (_greasePencilXrayMaterial == null)
                {
                    if (_defaultGreasePencilXrayMaterial == null)
                    {
                        _defaultGreasePencilXrayMaterial = new Material(Shader.Find("Hidden/GreasePencil"));
                        _defaultGreasePencilXrayMaterial.name = "Grease Pencil Material XRay";
                        _defaultGreasePencilXrayMaterial.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                        _defaultGreasePencilXrayMaterial.SetFloat("_ZWrite", 0f);
                        _defaultGreasePencilXrayMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
                    }

                    _greasePencilXrayMaterial = _defaultGreasePencilXrayMaterial;
                }
                return _greasePencilXrayMaterial;
            }
        }

        [SerializeField]
        List<Layer> _layers = new List<Layer>(); // init
        public ReadOnlyCollection<Layer> Layers { get { return _layers.AsReadOnly(); } }

        [SerializeField]
        int _activeLayerIndex;

        [SerializeField]
        bool _drawInGameCameras;

        [SerializeField]
        bool _drawByStrokeOrder;

        [SerializeField]
        List<LayerModifier> _inlineModifiers = new List<LayerModifier>(); // init
        public List<LayerModifier> InlineModifiers { get { return _inlineModifiers; } set { _inlineModifiers = value; } }

        List<IGreasePencilLayerListener> _layerListeners = new List<IGreasePencilLayerListener>(); // init
        private RenderPipelineAsset _pipeline;

        public Layer ActiveLayer
        {
            get
            {
                if ((_layers == null) || (_layers.Count == 0))
                    AddLayer("Default");

                _activeLayerIndex = Mathf.Clamp(_activeLayerIndex, 0, _layers.Count - 1);

                if ((_activeLayerIndex >= 0) && (_activeLayerIndex < _layers.Count))
                    return _layers[_activeLayerIndex];

                return null;
            }
        }

        public int ActiveLayerIndex { get { return _activeLayerIndex; } }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Initialize()
        {
            Camera.onPreCull -= Render;
            RenderPipelineManager.beginCameraRendering -= RenderRP;

            _pipeline = GraphicsSettings.renderPipelineAsset;

            if (_pipeline)
                RenderPipelineManager.beginCameraRendering += RenderRP;
            else
                Camera.onPreCull += Render;
        }

        private void Release()
        {
            _pipeline = null;
            RenderPipelineManager.beginCameraRendering -= RenderRP;
            Camera.onPreCull -= Render;
        }

        void UpdateListeners()
        {
            if (_layerListeners == null)
                _layerListeners = new List<IGreasePencilLayerListener>(); // init if null
            GetComponentsInChildren(_layerListeners);
        }

        void LayerChanged(Layer layer)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateListeners();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
            _layerListeners.ForEach(f => f.OnGreasePencilLayerChanged(this, layer));
        }

        void LayerAdded(Layer layer)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateListeners();
#endif
            _layerListeners.ForEach(f => f.OnGreasePencilLayerAdded(this, layer));
        }

        void LayerRemoved(Layer layer)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateListeners();
#endif
            _layerListeners.ForEach(f => f.OnGreasePencilLayerRemoved(this, layer));
        }

        public void StartStroke(Point worldPoint, float worldSize, Transform basisTransform)
        {
            StartStroke(worldPoint, worldSize, basisTransform, Color.white, Color.white);
        }

        public void StartStroke(Point worldPoint, float worldSize, Transform basisTransform, Color lineColor, Color fillColor)
        {
            if (ActiveLayer != null)
            {
                worldPoint.position = transform.InverseTransformPoint(worldPoint.position);
                ActiveLayer.StartStroke(worldPoint, worldSize, basisTransform, transform, lineColor, fillColor);
                LayerChanged(ActiveLayer);
            }
        }

        public void UpdateStroke(Point worldPoint)
        {
            if (ActiveLayer != null)
            {
                worldPoint.position = transform.InverseTransformPoint(worldPoint.position);
                ActiveLayer.UpdateStroke(worldPoint);
                LayerChanged(ActiveLayer);
            }
        }

        public void EndStroke(Point worldPoint)
        {
            if (ActiveLayer != null)
            {
                worldPoint.position = transform.InverseTransformPoint(worldPoint.position);
                ActiveLayer.EndStroke(worldPoint);
                LayerChanged(ActiveLayer);
            }
        }

        public void EndStroke()
        {
            if (ActiveLayer != null)
            {
                ActiveLayer.EndStroke();
                LayerChanged(ActiveLayer);
            }
        }

        public bool StrokeFinalized()
        {
            if (ActiveLayer != null)
                return ActiveLayer.IsStrokeFinalized;
            return true;
        }

        public void Retriangulate()
        {
            _layers.ForEach(l => l.RenderStrokes.ForEach(s => s.Triangulate()));
        }

        public void AddLayer(string name = "Layer")
        {
            if (_layers == null)
                _layers = new List<Layer>(); // init if null

            var uniqueName = name;
            var i = 0;

            while (_layers.FindIndex(l => l.name == uniqueName) >= 0)
            {
                ++i;
                uniqueName = string.Format("{0} {1:00}", name, i);
            }

            _activeLayerIndex = _layers.Count;
            var layer = Layer.New(uniqueName);
            _layers.Add(layer);
            LayerAdded(layer);
        }

        public void RemoveLayer(int index)
        {
            var layer = _layers[index];
            _layers.RemoveAt(index);
            LayerRemoved(layer);
        }

#if UNITY_EDITOR
        const string PREVIEW_SCENE_CAMERA = "Preview Scene Camera";
#endif

        public void CollapseCurrentModifierStack(bool keepModifiers = false)
        {
            ActiveLayer.CollapseModifierStack(keepModifiers);
            LayerChanged(ActiveLayer);
        }

        public void CollapseLayers()
        {
            if (_layers.Count < 1)
                return;

            var thisLayer = _layers[0];
            thisLayer.CollapseModifierStack();
            thisLayer.FlattenLayer();

            for (int i = 1; i < _layers.Count; ++i)
            {
                thisLayer.Merge(_layers[i]);
                _layers[i] = null;
            }

            _layers.RemoveAll(l => l == null);

            LayerChanged(ActiveLayer);
        }

        public void ClearAllLayers()
        {
            while (_layers.Count > 0)
                RemoveLayer(0);
        }

        [System.Serializable]
        public class SaveLayers
        {
            public List<Layer> layers = new List<Layer>();
        }

        public void Deserialize(string json)
        {
            var save = new SaveLayers();
            JsonUtility.FromJsonOverwrite(json, save);
            _layers = save.layers;

            UpdateListeners();
            _layers.ForEach(l => LayerChanged(l));
        }

        public string Serialize()
        {
            var save = new SaveLayers() { layers = _layers };
            return JsonUtility.ToJson(save);
        }

        public static void ResizeList<T>(List<T> list, int minSize)
        {
            if (list.Capacity < minSize) list.Capacity = minSize;
        }

        public static void Subtract(Vector3 v1, Vector3 v2, ref Vector3 result)
        {
            result.x = v1.x - v2.x;
            result.y = v1.y - v2.y;
            result.z = v1.z - v2.z;

            if ((result.x == 0) && (result.y == 0) && (result.z == 0))
                result = Vector3.right;
        }

        private void RenderRP(ScriptableRenderContext context, Camera camera)
        {
            Render(camera);
        }

        private void Render(Camera cam)
        {
            if ((GreasePencilMaterial == null))// || (cam.tag != "MainCamera"))
                return;

#if UNITY_EDITOR
            if (cam.name.Equals(PREVIEW_SCENE_CAMERA))
                return;

            if (!_drawInGameCameras && ((UnityEditor.SceneView.currentDrawingSceneView == null) || (UnityEditor.SceneView.currentDrawingSceneView.camera != cam)))
                return;
#endif

            var drawLine = false;
            var drawFill = false;

            Stroke currentStroke;

            var layers = _layers;

            if (layers == null)
                return;

            var tm = transform.localToWorldMatrix;

            var camMatrix = cam.transform.worldToLocalMatrix;
            var pixelUnitRatio = GetSize(cam.transform.TransformPoint(Vector3.forward), cam);

            var up = cam.transform.up.normalized;
            var right = cam.transform.right.normalized;
            var forward = cam.transform.forward.normalized;

            for (int i = 0; i < layers.Count; ++i)
            {
                Layer currentLayer = layers[i];
                if (!currentLayer.visible || currentLayer.RenderStrokes == null)
                    continue;

                // stroke data
                using (PooledList<Vector3>.Get(out List<Vector3> lineVerts))
                using (PooledList<Color>.Get(out List<Color> lineCols))
                using (PooledList<int>.Get(out List<int> lineTris))
                // fill data
                using (PooledList<Vector3>.Get(out List<Vector3> fillVerts))
                using (PooledList<Color>.Get(out List<Color> fillCols))
                using (PooledList<int>.Get(out List<int> fillTris))
                // final data
                using (PooledList<Vector3>.Get(out List<Vector3> vertices))
                using (PooledList<Color>.Get(out List<Color> colors))
                using (PooledList<int>.Get(out List<int> tris))
                using (PooledList<Vector3>.Get(out List<Vector3> tempVerts))
                {
                    int currentStrokesCount = currentLayer.RenderStrokes.Count;
                    int verticesCount = vertices.Count;

                    for (int strokeIndex = 0; strokeIndex < currentStrokesCount; ++strokeIndex)
                    {
                        currentStroke = currentLayer.RenderStrokes[strokeIndex];
                        var currentStrokePoints = currentStroke.Points;
                        int currentStrokePointsCount = currentStrokePoints.Count;

                        var fillColor = currentLayer.ColorOp(currentLayer.FillColor, currentStroke.FillColor);
                        drawFill = fillColor.a > 0;

                        var lineColor = currentLayer.ColorOp(currentLayer.LineColor, currentStroke.LineColor);
                        drawLine = lineColor.a > 0;

                        if (!drawFill && !drawLine)
                            continue;

                        currentStroke.GetVertices(tempVerts);
                        int tempVertsCount = tempVerts.Count;

                        // first we draw the fills
                        if (drawFill)
                        {
                            int vertexOffset = verticesCount;
                            int fillVertsCount = fillVerts.Count;

                            if (!_drawByStrokeOrder)
                                vertexOffset += fillVertsCount;

                            using (PooledList<int>.Get(out List<int> tempTris))
                            {
                                ResizeList(fillCols, fillVertsCount + currentStrokePointsCount);
                                for (int c = 0; c < currentStrokePointsCount; ++c)
                                    fillCols.Add(fillColor);

                                ResizeList(fillVerts, tempVertsCount + fillVertsCount);
                                if (!currentLayer.xray)
                                {
                                    var size = 0f;

                                    for (int v = 0; v < tempVertsCount; ++v)
                                    {
                                        Vector3 currentVert = tempVerts[v];
                                        size = GetSize(tm.MultiplyPoint(currentVert), camMatrix, cam, pixelUnitRatio) * currentLayer.width * 0.5f;
                                        fillVerts.Add(currentVert + currentStrokePoints[v].normal * (size * 0.5f));
                                    }
                                }
                                else
                                    for (int v = 0; v < tempVertsCount; ++v)
                                        fillVerts.Add(tempVerts[v]);

                                tempTris.Clear();
                                tempTris.AddRange(currentStroke.GetTriangles());
                                int tempTrisCount = tempTris.Count;

                                var array = ListTools.GetItems(tempTris);
                                for (int t = 0; t < tempTrisCount; ++t)
                                    array[t] += vertexOffset;
                                //tempTris[t] += vertexOffset;


                                fillTris.AddRange(tempTris);

                                vertexOffset = verticesCount + fillVertsCount;
                            }
                        }

                        if (_drawByStrokeOrder)
                        {
                            vertices.AddRange(fillVerts);
                            verticesCount = vertices.Count;
                            colors.AddRange(fillCols);
                            tris.AddRange(fillTris);

                            fillVerts.Clear();
                            fillCols.Clear();
                            fillTris.Clear();
                        }

                        if (drawLine)
                        {
                            int vertexOffset = verticesCount;

                            if (!_drawByStrokeOrder)
                                vertexOffset += lineVerts.Count;

                            Vector3 vec = Vector3.zero, tangent = Vector3.zero, position;
                            float size;

                            if (currentStrokePointsCount > 1)
                            {
                                ResizeList(lineVerts, lineVerts.Count + currentStrokePointsCount * 2);
                                ResizeList(lineCols, lineCols.Count + currentStrokePointsCount * 2);
                                ResizeList(lineTris, lineTris.Count + (currentStrokePointsCount - 1) * 6);

                                for (int pointIndex = 0; pointIndex < currentStrokePointsCount; ++pointIndex)
                                {
                                    Point currentPoint = currentStrokePoints[pointIndex];
                                    Vector3 currentPosition = currentPoint.position;

                                    if (pointIndex == 0)
                                        Subtract(tempVerts[pointIndex + 1], currentPosition, ref vec);
                                    else if (pointIndex == currentStrokePointsCount - 1)
                                        Subtract(currentPosition, tempVerts[pointIndex - 1], ref vec);
                                    else
                                    {
                                        Subtract(tempVerts[pointIndex + 1], currentPosition, ref vec);
                                        Subtract(currentPosition, tempVerts[pointIndex - 1], ref tangent);

                                        float t = tangent.magnitude;
                                        float v = vec.magnitude;

                                        vec.x = tangent.x / t + vec.x / v;
                                        vec.y = tangent.y / t + vec.y / v;
                                        vec.z = tangent.z / t + vec.z / v;
                                    }

                                    var mag = vec.magnitude;

                                    vec.x /= mag;
                                    vec.y /= mag;
                                    vec.z /= mag;

                                    tangent = Vector3.Cross(vec, forward);// / crossSectionWidth;

                                    position = currentPosition;
                                    size = GetSize(tm.MultiplyPoint(position), camMatrix, cam, pixelUnitRatio) * currentLayer.width * 0.5f;

                                    tangent.x *= size;
                                    tangent.y *= size;
                                    tangent.z *= size;

                                    if (!currentLayer.xray)
                                        position += currentPoint.normal * (size * 0.5f);

                                    lineVerts.Add(position - tangent);
                                    lineVerts.Add(position + tangent);

                                    lineCols.Add(lineColor);
                                    lineCols.Add(lineColor);
                                }

                                for (int pointIndex = 0; pointIndex < currentStrokePointsCount - 1; ++pointIndex)
                                {
                                    lineTris.Add(vertexOffset + pointIndex * 2);
                                    lineTris.Add(vertexOffset + pointIndex * 2 + 2);
                                    lineTris.Add(vertexOffset + pointIndex * 2 + 1);

                                    lineTris.Add(vertexOffset + pointIndex * 2 + 1);
                                    lineTris.Add(vertexOffset + pointIndex * 2 + 2);
                                    lineTris.Add(vertexOffset + pointIndex * 2 + 3);
                                }

                                vertexOffset = verticesCount + lineVerts.Count;
                            }
                            else
                            {
                                //TODO: Draw Circles
                                if (currentLayer.width > 1f)
                                {
                                    if (currentStrokePointsCount > 0)
                                    {
                                        ResizeList(lineVerts, lineVerts.Count + currentStrokePointsCount * 4);
                                        ResizeList(lineCols, lineCols.Count + currentStrokePointsCount * 4);
                                        ResizeList(lineTris, lineTris.Count + currentStrokePointsCount * 6);

                                        for (int pointIndex = 0; pointIndex < currentStrokePointsCount; ++pointIndex)
                                        {
                                            position = tempVerts[pointIndex];
                                            size = GetSize(tm.MultiplyPoint(position), camMatrix, cam, pixelUnitRatio) * currentLayer.width * 0.5f;

                                            vec = up * size;
                                            tangent = right * size;

                                            lineVerts.Add(position - tangent + vec);
                                            lineVerts.Add(position - tangent - vec);
                                            lineVerts.Add(position + tangent + vec);
                                            lineVerts.Add(position + tangent - vec);

                                            lineCols.Add(lineColor);
                                            lineCols.Add(lineColor);
                                            lineCols.Add(lineColor);
                                            lineCols.Add(lineColor);
                                        }

                                        for (int pointIndex = 0; pointIndex < currentStrokePointsCount; ++pointIndex)
                                        {
                                            lineTris.Add(vertexOffset + pointIndex * 2);
                                            lineTris.Add(vertexOffset + pointIndex * 2 + 2);
                                            lineTris.Add(vertexOffset + pointIndex * 2 + 1);

                                            lineTris.Add(vertexOffset + pointIndex * 2 + 1);
                                            lineTris.Add(vertexOffset + pointIndex * 2 + 2);
                                            lineTris.Add(vertexOffset + pointIndex * 2 + 3);
                                        }
                                    }

                                    vertexOffset = verticesCount + lineVerts.Count;
                                }
                            }

                            if (_drawByStrokeOrder)
                            {
                                vertices.AddRange(lineVerts);
                                verticesCount = vertices.Count;
                                colors.AddRange(lineCols);
                                tris.AddRange(lineTris);
                                lineVerts.Clear();
                                lineCols.Clear();
                                lineTris.Clear();
                            }
                        }
                    }

                    // fill out the mesh data
                    if (!_drawByStrokeOrder)
                    {
                        if (drawFill)
                        {
                            vertices.AddRange(fillVerts);
                            colors.AddRange(fillCols);
                            tris.AddRange(fillTris);
                        }

                        if (drawLine)
                        {
                            int vertexCount = vertices.Count;
                            int lineTrisCount = lineTris.Count;
                            var array = ListTools.GetItems(lineTris);

                            for (int t = 0; t < lineTrisCount; ++t)
                                array[t] += vertexCount;
                            //lineTris[t] += vertexCount;

                            vertices.AddRange(lineVerts);
                            colors.AddRange(lineCols);
                            tris.AddRange(lineTris);
                        }
                    }


                    if (currentLayer.Mesh == null)
                    {
                        currentLayer.Mesh = new Mesh { hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor };
                        currentLayer.Mesh.name = currentLayer.name;
                        currentLayer.Mesh.MarkDynamic();
                    }
                    else if (currentLayer.Mesh.name != currentLayer.name)
                        currentLayer.Mesh.name = currentLayer.name;

                    currentLayer.Mesh.Clear();

                    if (vertices.Count > 0)
                    {
                        currentLayer.Mesh.SetVertices(vertices);
                        currentLayer.Mesh.SetColors(colors);
                        currentLayer.Mesh.SetTriangles(tris, 0);
                        currentLayer.Mesh.RecalculateBounds();

                        //GreasePencilMaterial.SetFloat(ZTest, (float)(currentLayer.xray ? CompareFunction.Always : CompareFunction.LessEqual));
                        //GreasePencilMaterial.SetFloat(ZWrite, 1);

                        MaterialPropertyBlock block = new MaterialPropertyBlock();

                        var mat = currentLayer.xray ? GreasePencilMaterialXray : GreasePencilMaterial;

                        if (currentLayer.xray)
                            mat.renderQueue = (int)RenderQueue.Transparent;
                        else
                            mat.renderQueue = currentLayer.RenderQueue;

                        Graphics.DrawMesh(currentLayer.Mesh, tm, mat, gameObject.layer, cam, 0, block);
                    }
                }
            }
        }

        static int? _ztest;
        public static int ZTest
        {
            get
            {
                if (!_ztest.HasValue)
                    _ztest = Shader.PropertyToID("_ZTest");
                return _ztest.Value;
            }
        }

        static int? _zwrite;
        public static int ZWrite
        {
            get
            {
                if (!_zwrite.HasValue)
                    _zwrite = Shader.PropertyToID("_ZWrite");
                return _zwrite.Value;
            }
        }

        private static float GetSize(Vector3 worldPoint, Camera camera)
        {
            var screenPoint = camera.WorldToScreenPoint(worldPoint);
            screenPoint.y += 1f;
            var resultWorldPoint = camera.ScreenToWorldPoint(screenPoint);

            if (camera.orthographic)
                return Vector3.Distance(worldPoint, resultWorldPoint) / screenPoint.z;
            return Vector3.Distance(worldPoint, resultWorldPoint);
        }

        private static float GetSize(Vector3 worldPoint, Matrix4x4 invCameraMatrix, Camera camera, float ratio)
        {
            if (camera.orthographic)
                return ratio;
            return invCameraMatrix.MultiplyPoint(worldPoint).z * ratio;
        }

        private static void BeginLineDrawing(Matrix4x4 matrix, int mode, Material material)
        {
            Color c = Color.white;
            material.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(matrix);
            GL.Begin(mode);
            GL.Color(c);
        }

        private static void EndLineDrawing()
        {
            GL.End();
            GL.PopMatrix();
        }

#if UNITY_EDITOR
        static List<GreasePencilCanvas> _allGreasePencils = new List<GreasePencilCanvas>(); // init static

        public static void UpdateWithModifier(GreasePencilModifier modifier)
        {
            foreach (var pencil in _allGreasePencils)
                foreach (var layer in pencil.Layers)
                    foreach (var layerModifier in layer.Modifiers)
                        if (layerModifier.modifier == modifier)
                            layer.RefreshStrokes();
        }

        void OnValidate()
        {
            _activeLayerIndex = Mathf.Clamp(_activeLayerIndex, 0, _layers.Count - 1);
            if (!_allGreasePencils.Contains(this))
                _allGreasePencils.Add(this);

            foreach (var layer in _layers)
                layer.RefreshStrokesIfDirty();

            if (GraphicsSettings.renderPipelineAsset != _pipeline)
                Initialize();
        }
#endif
    }
}