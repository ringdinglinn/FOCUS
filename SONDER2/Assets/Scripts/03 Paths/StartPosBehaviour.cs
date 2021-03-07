using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

[ExecuteAlways]
public class StartPosBehaviour : MonoBehaviour {

    bool snapped = false;
    public PathCreator myPath;
    float dist = 0;
    float speed = 5f;

    private void Update() {
        transform.position = myPath.path.GetPointAtDistance(dist);
        dist += speed * Time.deltaTime;
    }
}

