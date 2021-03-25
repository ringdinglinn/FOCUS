using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColliderExtensions {
	public static Vector3[] GetVerticesOfBoxCollider(this BoxCollider col) {
        var trans = col.transform;
        var min = col.center - col.size * 0.5f;
        var max = col.center + col.size * 0.5f;
        var P0 = trans.TransformPoint(new Vector3(min.x, min.y, min.z));
        var P1 = trans.TransformPoint(new Vector3(min.x, min.y, max.z));
        var P2 = trans.TransformPoint(new Vector3(min.x, max.y, min.z));
        var P3 = trans.TransformPoint(new Vector3(min.x, max.y, max.z));
        var P4 = trans.TransformPoint(new Vector3(max.x, min.y, min.z));
        var P5 = trans.TransformPoint(new Vector3(max.x, min.y, max.z));
        var P6 = trans.TransformPoint(new Vector3(max.x, max.y, min.z));
        var P7 = trans.TransformPoint(new Vector3(max.x, max.y, max.z));

        return new Vector3[] { P0, P1, P2, P3, P4, P5, P6, P7 };
    }
}