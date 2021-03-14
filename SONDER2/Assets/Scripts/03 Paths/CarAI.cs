using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;
using PathCreation;

public class CarAI : MonoBehaviourReferenced {

    private WheelVehicle wheelVehicle;
    private PathBehaviour pathBehaviour;
    private PathCreator myPath;
    private SwitchingBehaviour switchingBehaviour;
    private Rigidbody rb;

    public bool autopilotEnabled = true;

    private bool isClone = false;

    private WheelVehicle originalCarWV;
    private Transform originalCarTransform;
    private Rigidbody originalCarRB;

    private CarAI clone;

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

    private bool inTunnel;
    public bool InTunnel {
        get {
                return inTunnel;
        } set {
                TunnelStateChange(value);
                inTunnel = value;
        }
    }

    private void OnEnable() {
        wheelVehicle = GetComponent<WheelVehicle>();
        switchingBehaviour = GetComponent<SwitchingBehaviour>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start() {
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
            wheelVehicle.Throttle = 0.5f;
            distOnPath += (transform.position - prevPos).magnitude;
            prevPos = transform.position;
            float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
            wheelVehicle.InstantSetWheelAngle(angle);
        } else if (isClone) {
            CloneMovement();
        }
    }

    private void SetPath(PathCreator p) {
        myPath = p;
    }

    public void SetPathID(int id) {
        pathID = id;
        GetPathInfo();
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
        Vector3 pos = TransformPointToStart(transform.position);
        Vector3 dir = TransformDirectionToStart(transform.forward);
        Vector3 vel = TransformDirectionToStart(rb.velocity);

        if (!autopilotEnabled) {
            Vector3 camTargetPos = switchingBehaviour.camTranslateTarget.transform.position;
            camTargetPos = TransformPointToStart(camTargetPos);
            Vector3 currentCamPos = cam.transform.position;
            currentCamPos = TransformPointToStart(currentCamPos);
            cam.Loop(currentCamPos);
        }

        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = vel;

        float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
        wheelVehicle.InstantSetWheelAngle(angle);

        distOnPath = myPath.path.GetClosestDistanceAlongPath(pos);
    }

    private Vector3 TransformPointToStart(Vector3 pos) {
        pos = endTunnel.InverseTransformPoint(pos);
        pos = startTunnel.TransformPoint(pos);
        return pos;
    }

    private Vector3 TransformDirectionToStart(Vector3 dir) {
        dir = endTunnel.InverseTransformDirection(dir);
        dir = startTunnel.TransformDirection(dir);
        return dir;
    }

    private void TunnelStateChange(bool b) {
        if (b) {
            EnterTunnel();
        } else {
            ExitTunnel();
        }
    }

    public void StartClone(WheelVehicle wv, Transform transform, Rigidbody rb) {
        autopilotEnabled = false;
        originalCarWV = wv;
        originalCarTransform = transform;
        originalCarRB = rb;
        isClone = true;
    }

    private void CloneMovement() {
        Vector3 pos = TransformPointToStart(originalCarTransform.position);
        Vector3 dir = TransformDirectionToStart(originalCarTransform.forward);
        Vector3 vel = TransformDirectionToStart(originalCarRB.velocity);
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = vel;
    }

    private void CreateClone() {
        clone = referenceManagement.carManagement.CreateCarClone(pathID);
        clone.StartClone(wheelVehicle, transform, rb);
        CreateCloneCam();
    }

    private void CreateCloneCam() {
        GameObject cloneCamObj = Instantiate(referenceManagement.camPrefab, Vector3.zero, Quaternion.identity);
        Camera1stPerson cloneCam = cloneCamObj.GetComponent<Camera1stPerson>();
        cloneCam.SetAsCloneCam();
        cloneCam.SwitchCar(clone.switchingBehaviour.camTranslateTarget.transform, clone.switchingBehaviour.camRotTarget.transform);
    }

    private void EnterTunnel() {
        Debug.Log($"{switchingBehaviour.id} has entered tunnel");
        if (!autopilotEnabled) CreateClone();
    }

    private void ExitTunnel() {
        Debug.Log($"{switchingBehaviour.id} has exited tunnel");
    }

    private void GetPathInfo() {
        pathBehaviour = referenceManagement.pathManagement.GetMyPath(pathID);
        SetPath(pathBehaviour.GetPath());
    }
}