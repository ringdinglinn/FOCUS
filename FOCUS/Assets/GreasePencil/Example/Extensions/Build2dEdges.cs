using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class Build2dEdges : MonoBehaviour, IGreasePencilLayerListener
    {
        [SerializeField, Range(0.001f, 1f)]
        float _simplification = 0f;

        [SerializeField]
        List<EdgeCollider2D> _edgeColliders = new List<EdgeCollider2D>();

        void OnValidate()
        {
            UpdateEdges(GetComponent<GreasePencilCanvas>());
        }

        public void OnGreasePencilLayerAdded(GreasePencilCanvas greasePencil, Layer layer)
        {
            UpdateEdges(greasePencil);
        }

        public void OnGreasePencilLayerChanged(GreasePencilCanvas greasePencil, Layer layer)
        {
            UpdateEdges(greasePencil);
        }

        public void OnGreasePencilLayerRemoved(GreasePencilCanvas greasePencil, Layer layer)
        {
            UpdateEdges(greasePencil);
        }

        void UpdateEdges(GreasePencilCanvas greasePencil)
        {
            var layers = greasePencil.Layers;
            var path = 0;

            for (int i = 0; i < layers.Count; ++i)
            {
                for (int s = 0; s < layers[i].RenderStrokes.Count; ++s)
                {
                    if (layers[i].RenderStrokes[s].Points.Count > 0)
                        ++path;
                }
            }

            if (path != _edgeColliders.Count)
            {
                if (_edgeColliders.Count > path)
                {
                    for (int i = path; i < _edgeColliders.Count; ++i)
                    {
                        DestroyImmediate(_edgeColliders[i], true);
                        _edgeColliders[i] = null;
                    }

                    _edgeColliders.RemoveAll(c => c == null);
                }
                else
                {
                    while (_edgeColliders.Count < path)
                    {
                        var collider = gameObject.AddComponent<EdgeCollider2D>();
                        _edgeColliders.Add(collider);
                    }
                }
            }

            List<Vector2> points;
            List<Vector2> simplePoints;


            using (PooledList<Vector2>.Get(out points))
            using (PooledList<Vector2>.Get(out simplePoints))
            {
                path = 0;

                for (int i = 0; i < layers.Count; ++i)
                {
                    for (int s = 0; s < layers[i].RenderStrokes.Count; ++s)
                    {
                        var collider = _edgeColliders[path];
                        collider.hideFlags = HideFlags.NotEditable;

                        points.Clear();
                        simplePoints.Clear();

                        if (layers[i].RenderStrokes[s].Points.Count > 0)
                        {
                            for (int p = 0; p < layers[i].RenderStrokes[s].Points.Count; ++p)
                                points.Add(layers[i].RenderStrokes[s].Points[p].position);

                            LineUtility.Simplify(points, _simplification, simplePoints);
                            collider.points = simplePoints.ToArray();
                        }

                        ++path;
                    }
                }
            }
        }
    }
}
