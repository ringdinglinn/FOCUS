using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;
using PathCreation;

public class CarAI : MonoBehaviourReferenced {

    private CarManagement carManagement;

    private WheelVehicle wheelVehicle;
    public PathBehaviour pathBehaviour;
    private PathCreator myPath;
    private SwitchingBehaviour switchingBehaviour;
    private Rigidbody rb;

    public bool autopilotEnabled = true;

    public bool isClone = false;
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

    public int pathID;

    public int id;
    public int PathID {
        get { return pathID; }
        set { pathID = value;
            GetPathInfo(); }
    }
    public int manualPathID;

    public TunnelBehaviour startTunnel;
    public TunnelBehaviour endTunnel;

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

    private int currentGear = 1;
    public int CurrentGear {
        get { return currentGear;  }
    }

    public bool dontLoop = false;


    private void OnEnable() {
        wheelVehicle = GetComponent<WheelVehicle>();
        switchingBehaviour = GetComponent<SwitchingBehaviour>();
        carManagement = referenceManagement.carManagement;
        id = switchingBehaviour.id;
        rb = GetComponent<Rigidbody>();
        carManagement.AddCarManagement(this);
    }

    private void Start() {
        if (carManagement.GetManualInitialCar() && switchingBehaviour.isInitialCar) {
            PathID = manualPathID;
        }
        if (isClone) {
            SetCloneTransform();
        }
    }

    private void FixedUpdate() {
        if (autopilotEnabled) {
            AutoPilot();
        } 
    }

    public void SetUpInititalCar() {
        currentGear = 1;
    }

    #region ------------------------------------------------ AUTOPILOT -----------------------------------------------------

    private void AutoPilot() {

        wheelVehicle.Throttle = (Mathf.Pow(pathBehaviour.GetSpeedLimit(), 2) - rb.velocity.sqrMagnitude) / 10;

        distOnPath += (transform.position - prevPos).magnitude;
        prevPos = transform.position;
        float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
        wheelVehicle.InstantSetWheelAngle(angle);

        RaycastHit hit;
        int layerMask = 1 << 9;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, layerMask)) {
            wheelVehicle.Throttle = 1 - Mathf.InverseLerp(0, 20, hit.distance);
        }
    }

    public void SwitchOnAutopilot() {
        autopilotEnabled = true;
    }

    public void SwitchOffAutopilot() {
        autopilotEnabled = false;
    }

    #endregion

    #region ------------------------------------------------ PATH LOGIC ----------------------------------------------------

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

    private void SetPath(PathCreator p) {
        myPath = p;
    }

    private void GetPathInfo() {
        pathBehaviour = referenceManagement.pathManagement.GetMyPath(pathID);
        if (pathBehaviour.endTunnel != null) endTunnel = pathBehaviour.endTunnel;
        if (pathBehaviour.startTunnel != null) startTunnel = pathBehaviour.startTunnel;
        SetPath(pathBehaviour.GetPath());
    }

    #endregion

    #region -------------------------------------------------- GEARS -------------------------------------------------------

    private void SelectRandomGear() {
        currentGear = Random.Range(2, 7);
    }

    public void SetGear(int gear) {
        currentGear = gear;
    }

    #endregion

    #region -------------------------------------------- LOOPING & CLONING -------------------------------------------------


    public void ActiveCarHasReachedPortal() {
        StartCoroutine(WaitToChangeToClone());
    }

    private IEnumerator WaitToChangeToClone() {
        yield return new WaitForEndOfFrame();
        ChangeToClone();
    }

    public void PortalReached() {
        if (!dontLoop) {
            if (!autopilotEnabled) {
            }
            else if (!inTunnelWithActiveCar && !dontLoop) {

            }
        }
    }

    public void PortalReachedActiveCar() {
        if (!dontLoop) {
            carManagement.ActiveCarHasReachedPortal(this);
        }
    }

    public void PortalReachedInactiveCar() {
        Debug.Log($"portal reached inactive car, dontLoop = {dontLoop}");
        if (!dontLoop) {
            if (!inTunnelWithActiveCar) {
                if (startTunnel != null) TunnelLoop();
                else Loop();
            }
        }
    }

    private void Loop() {
        Vector3 vel = rb.velocity;
        Vector3 dir0 = myPath.path.GetDirectionAtDistance(distOnPath);
        float angle = Vector3.SignedAngle(dir0, vel, Vector3.up);

        distOnPath = 2;
        Vector3 pos = myPath.path.GetPointAtDistance(distOnPath);
        Vector3 dir = myPath.path.GetDirectionAtDistance(distOnPath);
        vel = RotatePointAroundPivot(vel, pos, angle);

        rb.velocity = vel;
        
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);

        prevPos = myPath.path.GetPointAtDistance(0);

        float angle2 = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
        wheelVehicle.InstantSetWheelAngle(angle2);
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle) {
         Vector3  dir = point - pivot; // get point direction relative to pivot
         dir = Quaternion.Euler(0,angle,0)* dir; // rotate it
         point = dir + pivot; // calculate rotated point
         return point; // return it
    }

    private void TunnelLoop() {
        dontLoop = true;
        Vector3 pos = TransformPointToStart(transform.position);
        Vector3 dir = TransformDirectionToStart(transform.forward);
        Vector3 vel = TransformDirectionToStart(rb.velocity);
        Vector3 angVel = TransformDirectionToStart(rb.angularVelocity);

        Debug.Log($"pos = {pos}, {gameObject.name}, startTunnel = {startTunnel}");

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
        pos = endTunnel.transform.InverseTransformPoint(pos);
        pos = startTunnel.transform.TransformPoint(pos);
        return pos;
    }

    private Vector3 TransformDirectionToStart(Vector3 dir) {
        dir = endTunnel.transform.InverseTransformDirection(dir);
        dir = startTunnel.transform.TransformDirection(dir);
        return dir;
    }

    private void ChangeToClone() {
        carManagement.ChangeToClone(!autopilotEnabled, this, clone);
    }

    public void MakeMainCar(bool isActiveCar) {
        isClone = false;
        if (isActiveCar) cam.MakeMainCarCamera();
    }

    public void StartClone(bool isActiveCar, WheelVehicle wv, Transform transform, Rigidbody rb, CarAI carAI) {
        autopilotEnabled = !isActiveCar;
        dontLoop = true;
        wheelVehicle.IsPlayer = isActiveCar;
        originalCarWV = wv;
        originalCarTransform = transform;
        originalCarRB = rb;
        originalCarAI = carAI;
        startTunnel = carAI.startTunnel;
        endTunnel = carAI.endTunnel;
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
        if (!dontLoop) {
            inTunnelWithActiveCar = true;
            CreateClone();
        }
    }

    #endregion

    #region ------------------------------------------------ PUBLIC INFO ---------------------------------------------------

    public Vector3 GetPrevPos() {
        return prevPos;
    }

    public SwitchingBehaviour GetSwitchingBehaviour() {
        return switchingBehaviour;
    }

    #endregion
}