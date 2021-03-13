using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;
using PathCreation;

public class CarAI : MonoBehaviourReferenced {

    private WheelVehicle carConroller;
    private PathBehaviour pathBehaviour;
    private PathCreator myPath;
    private SwitchingBehaviour switchingBehaviour;
    private Rigidbody rb;

    public bool autopilotEnabled = true;

    private Vector3 startPos;
    private Vector3 startDir;
    private Vector3 offset = new Vector3(0, 2f, 0);

    private float distOnPath;

    private Vector3 prevPos;

    public bool started = false;

    private float steerTowardsDist = 2;

    [SerializeField] private int pathID;

    private Transform startTunnel;
    private Transform endTunnel;

    private Camera1stPerson cam;


    private void Start() {
        carConroller = GetComponent<WheelVehicle>();
        switchingBehaviour = GetComponent<SwitchingBehaviour>();
        rb = GetComponent<Rigidbody>();
        GetPathInfo();
        SetToStartConfig();
        startTunnel = pathBehaviour.startTunnel.transform;
        endTunnel = pathBehaviour.endTunnel.transform;
        cam = referenceManagement.cam;
    }

    public void CreateStartConfig(Vector3 pos, Vector3 dir) {
        startPos = pos;
        startPos += offset;
        startDir = dir;
    }

    public void SetToStartConfig() {
        distOnPath = myPath.path.GetClosestDistanceAlongPath(startPos);
        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation(startDir);
        prevPos = transform.position;
        started = true;
    }

    private void FixedUpdate() {
        if (autopilotEnabled) {
            carConroller.Throttle = 0.5f;
            distOnPath += (transform.position - prevPos).magnitude;
            prevPos = transform.position;
            float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
            carConroller.InstantSetWheelAngle(angle);
        }
    }

    private void SetPath(PathCreator p) {
        myPath = p;
    }

    public void SetPathID(int id) {
        pathID = id;
    }

    public void SwitchOnAutopilot() {
        autopilotEnabled = true;
    }

    public void SwitchOffAutopilot() {
        autopilotEnabled = false;
    }

    public void EndReached() {
        if (started) Loop();
    }

    private void Loop() {
        Vector3 pos = endTunnel.InverseTransformPoint(transform.position);
        pos = startTunnel.TransformPoint(pos);

        Vector3 dir = endTunnel.InverseTransformDirection(transform.forward);
        dir = startTunnel.TransformDirection(dir);

        Vector3 vel = endTunnel.InverseTransformDirection(rb.velocity);
        vel = startTunnel.TransformDirection(vel);

        if (!autopilotEnabled)
        cam.Loop(transform.position, pos);

        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = vel;

        float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
        carConroller.InstantSetWheelAngle(angle);

        distOnPath = myPath.path.GetClosestDistanceAlongPath(pos);
    }

    private void GetPathInfo() {
        pathBehaviour = referenceManagement.pathManagement.GetMyPath(pathID);
        SetPath(pathBehaviour.GetPath());
    }
}