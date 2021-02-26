using UnityEditor;
using UnityEngine;

namespace GreasePencil
{
    public class DrawModeScreen : DrawModeImpl
    {
        public override int Order { get { return 0; } }
        public override string Name { get { return "Screen"; } }
        public override string Tooltip { get { return "Draws on a 'canvas' parallel to the screen"; } }

        protected override bool DoHit(Event evt, Vector2 screenPosition, out GreasePencilHit worldHit)
        {
            return GreasePencilEditor.PlaneCast(EditorState.Current.Instance.transform.position, screenPosition, EditorState.Current.camera, out worldHit);
        }

        protected override void DrawSceneGUI()
        {
            using (new Handles.DrawingScope(Handles.centerColor))
                DrawPoint(-1, EditorState.Current.Instance.transform.position, 0.05f);
        }
    }
}