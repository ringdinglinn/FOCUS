using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class IntersectMesh
{
    const string INTERSECTRAYMESH = "IntersectRayMesh";
    private const System.Reflection.BindingFlags BINDING = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;

    static System.Reflection.MethodInfo _intersectMethod;
    static System.Reflection.MethodInfo IntersectMethod
    {
        get { return _intersectMethod ?? (_intersectMethod = typeof(HandleUtility).GetMethod(INTERSECTRAYMESH, BINDING)); }
    }

    public static bool Raycast(Vector2 position, out RaycastHit hit, GameObject[] ignore = null)
    {
        position.y = Screen.height - position.y - 37f;
        GameObject go = null;

        go = HandleUtility.PickGameObject(position, false, ignore);

        if (go == null)
        {
            hit = default(RaycastHit);
            return false;
        }

        var meshFilter = go.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            hit = default(RaycastHit);
            return false;
        }

        return Raycast(HandleUtility.GUIPointToWorldRay(position), meshFilter.sharedMesh, go.transform.localToWorldMatrix, out hit);
    }

    public static bool Raycast(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
    {
        return Raycast(ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, out hit);
    }

    public static bool Raycast(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
    {
        var parameters = new object[] { ray, mesh, matrix, null };
        bool result = (bool)IntersectMethod.Invoke(null, parameters);
        hit = (RaycastHit)parameters[3];
        return result;
    }

    public static GameObject PickGameObject(Vector2 position, bool updateSelection = true, bool selectPrefabRoot = false, GameObject[] ignore = null)
    {
        if (updateSelection == false && Event.current != null)
        {
            int blocking_ix = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(blocking_ix);
            GUIUtility.hotControl = blocking_ix;
        }

        return HandleUtility.PickGameObject(position, selectPrefabRoot, ignore);
    }
}
