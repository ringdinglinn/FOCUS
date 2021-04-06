using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class StreetlightPlacer : MonoBehaviourReferenced {
	private GameObject origStreetlight;
    private PathBehaviour pathBehaviour;
    private StartPosBehaviour startPosBehaviour;
    private RoadMeshCreator roadMeshCreator;
    private PathCreator myPath;

    private float intervall = 40;

    private List<Vector3> streetlightMarkers;

    private GameObject parentObject;

    private void Start() {
        origStreetlight = referenceManagement.streetlight;
        pathBehaviour = GetComponent<PathBehaviour>();
        roadMeshCreator = GetComponent<RoadMeshCreator>();
        startPosBehaviour = GetComponent<StartPosBehaviour>();
        myPath = pathBehaviour.GetPath();

        streetlightMarkers = startPosBehaviour.GetStreetlightMarkerList();

        parentObject = GameObject.Find("Streetlights");

        PlaceLights();
    }

    private void PlaceLights() {
        float start = 0;
        float end = myPath.path.length;

        if (streetlightMarkers.Count != 0) {
            start = myPath.path.GetClosestDistanceAlongPath(streetlightMarkers[0]);         // currently system only handles start and end, but can be easily adapted
        }
        if (streetlightMarkers.Count > 1) {
            end = myPath.path.GetClosestDistanceAlongPath(streetlightMarkers[1]);
        }

        float dist = start;
        float offset = roadMeshCreator.roadWidth;
        while (end > dist) {
            Vector3 offsetDir = Vector3.Cross(myPath.path.GetDirectionAtDistance(dist), Vector3.up);
            Vector3 pos = myPath.path.GetPointAtDistance(dist);
            Vector3 dir = offsetDir * (-1);
            pos = pos + offsetDir * offset;
            GameObject obj = Instantiate(origStreetlight, pos, Quaternion.LookRotation(dir));
            obj.transform.SetParent(parentObject.transform);
            dist += intervall;
        }
    }
}