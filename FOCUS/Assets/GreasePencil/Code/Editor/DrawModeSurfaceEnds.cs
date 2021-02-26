using UnityEngine;
using UnityEditor;

namespace GreasePencil
{
    public class DrawModeSurfaceEnds : DrawModeImpl
    {
        public override int Order { get { return -202; } }
        public override string Name { get { return "Surface Ends"; } }
        public override string Tooltip { get { return "Draws on a 'canvas' parallel to the screen starting from the first surface point hit"; } }

        protected override bool DoHit(Event evt, Vector2 screenPosition, out GreasePencilHit worldPosition)
        {
            bool wasHit = false;
            if ((evt.type == EventType.MouseDown) || (evt.type == EventType.MouseUp))
            {
                var hit = default(RaycastHit);
                wasHit = IntersectMesh.Raycast(screenPosition, out hit, EditorState.Current.ignoreObjects);
                if (wasHit)
                    worldPosition = new GreasePencilHit(hit.point, hit.normal, EditorState.Current.camera.transform.position);
                else
                    wasHit = GreasePencilEditor.PlaneCast(EditorState.Current.Instance.transform.position, screenPosition, EditorState.Current.camera, out worldPosition);
            }
            else if (evt.type == EventType.MouseDrag)
            {
                var tm = EditorState.Current.Instance.transform;

                return GreasePencilEditor.PlaneCast(
                    tm.TransformPoint(EditorState.Current.Instance.ActiveLayer.CurrentStroke.FirstPoint.position), 
                    screenPosition, EditorState.Current.camera, out worldPosition);
            }
            else
                worldPosition = new GreasePencilHit();

            return wasHit;
        }

        protected override void DoMouseUp(Event evt, GreasePencilHit worldPosition)
        {
            GreasePencilCanvas instance = EditorState.Current.Instance;
            Camera camera = EditorState.Current.camera;
            Transform transform = instance.transform;

            instance.EndStroke(new Point(worldPosition));

            // we lerp between the first and last points in the points list
            var points = instance.ActiveLayer.CurrentStroke.Points;

            var first = camera.WorldToScreenPoint(transform.TransformPoint(instance.ActiveLayer.CurrentStroke.FirstPoint.position));
            var last = camera.WorldToScreenPoint(transform.TransformPoint(instance.ActiveLayer.CurrentStroke.LastPoint.position));

            float s = 0f;//1f / (points.Count - 1);

            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];

                var position = camera.WorldToScreenPoint(transform.TransformPoint(point.position));
                position.z = Mathf.Lerp(first.z, last.z, s * i);

                point.position = transform.InverseTransformPoint(camera.ScreenToWorldPoint(position));
                points[i] = point;
            }

            instance.ActiveLayer.CurrentStroke.Points = points;

            evt.Use();
            EditorUtility.SetDirty(instance);
        }
    }
}