using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;
using PathCreation;

public class CarAI : MonoBehaviourReferenced {

    private CarManagement carManagement;

    private WheelVehicle wheelVehicle;
    private PathBehaviour pathBehaviour;
    private PathCreator myPath;
    private SwitchingBehaviour switchingBehaviour;
    private Rigidbody rb;

    public bool autopilotEnabled = true;

    private bool isClone = false;
    private bool hasClone = false;
    public bool HasClone {
        get { return hasClone; }
    }

    private WheelVehicle originalCarWV;
    private Transform originalCarTransform;
    private Rigidbody originalCarRB;
    private CarAI originalCarAI;
    private Transform origCamTransform;

    private CarAI clone;

    private Vector3 startPos;
    private Vector3 startDir;
    private Vector3 offset = new Vector3(0, 2f, 0);

    private float distOnPath;

    private Vector3 prevPos;

    public bool started = false;

    private float steerTowardsDist = 2;

    private int pathID;

    public int id;
    public int PathID {
        get { return pathID; }
        set { pathID = value;
            GetPathInfo(); }
    }

    private Transform startTunnel;
    private Transform endTunnel;

    public Camera1stPerson cam;

    private bool inTunnel;
    public bool InTunnel {
        get {
                return inTunnel;
        } set {
                inTunnel = value;
        }
    }

    private bool inTunnelWithActiveCar;

    private void OnEnable() {
        wheelVehicle = GetComponent<WheelVehicle>();
        switchingBehaviour = GetComponent<SwitchingBehaviour>();
        id = switchingBehaviour.id;
        rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        if (pathBehaviour.startTunnel != null) startTunnel = pathBehaviour.startTunnel.transform;
        if (pathBehaviour.endTunnel != null) endTunnel = pathBehaviour.endTunnel.transform;
        carManagement = referenceManagement.carManagement;
        if (isClone) {
            SetCloneTransform();
        }
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
        } 
    }

    private void SetPath(PathCreator p) {
        myPath = p;
    }

    public void SwitchOnAutopilot() {
        autopilotEnabled = true;
    }

    public void SwitchOffAutopilot() {
        autopilotEnabled = false;
    }

    public void ActiveCarHasReachedPortal() {
        StartCoroutine(WaitToChangeToClone());
    }

    private IEnumerator WaitToChangeToClone() {
        yield return new WaitForEndOfFrame();
        ChangeToClone();
    }

    public void PortalReached() {
        if (!autopilotEnabled) {
            carManagement.ActiveCarHasReachedPortal(this);
        } else if (!inTunnelWithActiveCar){
            Loop();
        }
    }

    private void Loop() {
        Vector3 pos = TransformPointToStart(transform.position);
        Vector3 dir = TransformDirectionToStart(transform.forward);
        Vector3 vel = TransformDirectionToStart(rb.velocity);
        Vector3 angVel = TransformDirectionToStart(rb.angularVelocity);

        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = vel;
        rb.angularVelocity = angVel;

        float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
        wheelVehicle.InstantSetWheelAngle(angle);

        distOnPath = myPath.path.GetClosestDistanceAlongPath(pos);
        prevPos = TransformPointToStart(prevPos);
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

    private void ChangeToClone() {
        Debug.Log($"{gameObject.name} changes to clone");
        carManagement.ChangeToClone(!autopilotEnabled, this, clone);
    }

    public void MakeMainCar(bool isActiveCar) {
        isClone = false;
        if (isActiveCar) cam.MakeMainCarCamera();
    }

    public void StartClone(bool isActiveCar, WheelVehicle wv, Transform transform, Rigidbody rb, CarAI carAI) {
        autopilotEnabled = !isActiveCar;
        wheelVehicle.IsPlayer = isActiveCar;
        originalCarWV = wv;
        originalCarTransform = transform;
        originalCarRB = rb;
        originalCarAI = carAI;
        if (isActiveCar) {
            origCamTransform = carAI.cam.transform;
            switchingBehaviour.SetHeadlightsActiveCar();
        } else {
            switchingBehaviour.SetHeadlightsInactiveCar();
        }
        isClone = true;
    }

    private void SetCloneTransform() {
        Vector3 pos = TransformPointToStart(originalCarTransform.position);
        Vector3 dir = TransformDirectionToStart(originalCarTransform.forward);
        Vector3 vel = TransformDirectionToStart(originalCarRB.velocity);
        Vector3 angVel = TransformDirectionToStart(originalCarRB.angularVelocity);
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = vel;
        rb.angularVelocity = angVel;
        distOnPath = myPath.path.GetClosestDistanceAlongPath(transform.position);
        prevPos = TransformPointToStart(originalCarAI.GetPrevPos());
    }

    private void CreateClone() {
        Debug.Log($"{gameObject.name} creates clone");
        hasClone = true;
        clone = referenceManagement.carManagement.CreateCarClone(pathID);
        clone.StartClone(!autopilotEnabled, wheelVehicle, transform, rb, this);
        if (!autopilotEnabled) CreateCloneCam();
    }

    private void CreateCloneCam() {
        Vector3 pos = TransformPointToStart(cam.transform.position);
        Vector3 dir = TransformDirectionToStart(cam.transform.forward);
        GameObject cloneCamObj = Instantiate(referenceManagement.camPrefab, pos, Quaternion.LookRotation(dir));
        Camera1stPerson cloneCam = cloneCamObj.GetComponent<Camera1stPerson>();
        cloneCam.SetAsCloneCam();
        cloneCam.SwitchCar(clone.switchingBehaviour.camTranslateTarget.transform, clone.switchingBehaviour.camRotTarget.transform);
        clone.cam = cloneCam;
    }

    public void ActiveCarHasEnteredTunnel() {
        inTunnelWithActiveCar = true;
        CreateClone();
    }

    private void GetPathInfo() {
        pathBehaviour = referenceManagement.pathManagement.GetMyPath(pathID);
        SetPath(pathBehaviour.GetPath());
    }

    public Vector3 GetPrevPos() {
        return prevPos;
    }
}