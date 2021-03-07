using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class StreetlightPlacer : MonoBehaviourReferenced {
	private GameObject origStreetlight;
    private PathBehaviour pathBehaviour;
    private RoadMeshCreator roadMeshCreator;
    private PathCreator myPath;

    private float intervall = 40;

    private void Start() {
        origStreetlight = referenceManagement.streetlight;
        pathBehaviour = GetComponent<PathBehaviour>();
        roadMeshCreator = GetComponent<RoadMeshCreator>();
        myPath = pathBehaviour.GetPath();

        PlaceLights();
    }

    private void PlaceLights() {
        Debug.Log("place light");
        float dist = 0;
        float offset = roadMeshCreator.roadWidth;
        while (myPath.path.length > dist) {
            Vector3 offsetDir = Vector3.Cross(myPath.path.GetDirectionAtDistance(dist), Vector3.up);
            Vector3 pos = myPath.path.GetPointAtDistance(dist);
            Vector3 dir = offsetDir * (-1);
            pos = pos + offsetDir * offset;
            Instantiate(origStreetlight, pos, Quaternion.LookRotation(dir));
            dist += intervall;
        }
    }
}