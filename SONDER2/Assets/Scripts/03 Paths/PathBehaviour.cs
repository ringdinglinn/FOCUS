using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class PathBehaviour : MonoBehaviourReferenced {
	private PathCreator path;
    private PathManagement pathManagement;
    private StartPosBehaviour startPosBehaviour;
    private RoadMeshCreator roadMeshCreator;

    public int id;

    [SerializeField] float speedLimit;

    public TunnelBehaviour startTunnel;
    public TunnelBehaviour endTunnel;
    public TunnelBehaviour alternateEndTunnel;

    public int tunnelPoints = 0;

    public bool autoSetUpTriggers;

    private void OnEnable() {
        path = GetComponent<PathCreator>();
        startPosBehaviour = GetComponent<StartPosBehaviour>();
        roadMeshCreator = GetComponent<RoadMeshCreator>();
        pathManagement = referenceManagement.pathManagement;
        pathManagement.AddToPaths(this);
        if (startTunnel != null && endTunnel != null) {
            MakeStartAndEndIdentical();
            roadMeshCreator.PathAdjusted();
        }

        if (gameObject.name == "CarPath2 (7)") {
            Debug.Log("OnEnable, points:");
            for (int i = 0; i < path.bezierPath.NumPoints; i++) {
                Debug.Log(path.bezierPath.GetPoint(i));
            }
        }
    }

    private void Start() {
        if (gameObject.name == "CarPath2 (7)") {
            Debug.Log("Start, points:");
            for (int i = 0; i < path.bezierPath.NumPoints; i++) {
                Debug.Log(path.bezierPath.GetPoint(i));
            }
        }
    }

    private void SetUpTriggers() {
     
    }

    private void PlaceTrigger(GameObject obj, Vector3 pos, Vector3 dir) {
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.LookRotation(dir);
    }

    private void MakeStartAndEndIdentical() {
        path.EditorData.bezierPath.MakeStartAndEndIdentical(tunnelPoints, startTunnel.transform, endTunnel.transform);
    }

    public void SetID(int newID) {
        id = newID;
    }

    public PathCreator GetPath() {
        return path;
    }

    GameObject GetChildWithName(GameObject obj, string name) {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null) {
            return childTrans.gameObject;
        }
        else {
            return null;
        }
    }

    public float GetSpeedLimit() {
        return speedLimit;
    }

    public StartPosBehaviour GetStartPosBehaviour() {
        return startPosBehaviour;
    }

    public void TransitionToNextLevel() {
        startTunnel = endTunnel;
        endTunnel = alternateEndTunnel;
    }

}

[ExecuteInEditMode]
public class TriggerSetter : MonoBehaviourReferenced {
    public GameObject startTrigger;
    public GameObject endTrigger;
    PathCreator path;

    private void OnEnable() {
        path = GetComponent<PathCreator>();
    }

    private void Update() {
        SetUpTriggers();
    }

    private void SetUpTriggers() {
        PlaceTrigger(startTrigger, path.path.GetPointAtDistance(0), path.path.GetDirectionAtDistance(0));
        PlaceTrigger(endTrigger, path.path.GetPointAtDistance(path.path.length - 0.1f), path.path.GetDirectionAtDistance(path.path.length - 0.1f));
    }

    private void PlaceTrigger(GameObject obj, Vector3 pos, Vector3 dir) {
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.LookRotation(dir);
    }
}