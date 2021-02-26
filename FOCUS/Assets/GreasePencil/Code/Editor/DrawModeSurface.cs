using UnityEngine;

namespace GreasePencil
{
    public class DrawModeSurface : DrawModeImpl
    {
        public override int Order { get { return 200; } }
        public override string Name { get { return "Surface"; } }
        public override string Tooltip { get { return "Draws across any renderable meshes"; } }

        protected override bool DoHit(Event evt, Vector2 screenPosition, out GreasePencilHit worldPosition)
        {
            RaycastHit hit = default(RaycastHit);
            bool wasHit = IntersectMesh.Raycast(screenPosition, out hit, EditorState.Current.ignoreObjects);

            if (wasHit)
                worldPosition = new GreasePencilHit(hit.point, hit.normal, EditorState.Current.camera.transform.position);
            else
            {
                if (evt.type == EventType.MouseDown)
                {
                    wasHit = GreasePencilEditor.PlaneCast(EditorState.Current.Instance.transform.position, screenPosition, EditorState.Current.camera, out worldPosition);
                }
                else if (evt.type == EventType.MouseDrag)
                {
                    var tm = EditorState.Current.Instance.transform;

                    wasHit = GreasePencilEditor.PlaneCast(
                        tm.TransformPoint(EditorState.Current.Instance.ActiveLayer.CurrentStroke.LastPoint.position), 
                        tm.TransformDirection(EditorState.Current.Instance.ActiveLayer.CurrentStroke.LastPoint.normal), 
                        screenPosition, EditorState.Current.camera, out worldPosition);
                }
                else
                    worldPosition = new GreasePencilHit();
            }

            return wasHit;
        }
    }
}