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
    private CarVisuals carVisuals;
    private LevelManagement levelManagement;

    public bool autopilotEnabled = true;

    public bool isClone = false;
    private bool hasClone = false;
    public bool HasClone {
        get { return hasClone; }
    }

    private Transform originalCarTransform;
    private Rigidbody originalCarRB;
    private CarAI originalCarAI;

    private CarAI clone;

    private Vector3 startPos;
    private Vector3 startDir;
    private Vector3 offset = new Vector3(0, 2f, 0);

    private float distOnPath;

    private Vector3 prevPos;

    public bool started = false;

    private float steerTowardsDist = 2;
    private float throttle = 0;

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

    float speedLimit;
    float targetSpeedLimit;

    private int dangleNr;

    private void OnEnable() {
        wheelVehicle = GetComponent<WheelVehicle>();
        switchingBehaviour = GetComponent<SwitchingBehaviour>();
        carVisuals = GetComponent<CarVisuals>();
        carManagement = referenceManagement.carManagement;
        id = switchingBehaviour.id;
        rb = GetComponent<Rigidbody>();
        if (carVisuals != null) carVisuals = GetComponent<CarVisuals>();
        carManagement.AddCarAI(this);
        levelManagement = referenceManagement.levelManagement;
    }

    private void OnDisable() {
        carManagement.RemoveCarAI(this);
    }

    private void Start() {
        if (carManagement.HasManualInitialCar() && switchingBehaviour.isInitialCar) {
            PathID = manualPathID;
        }
        if (isClone) {
            SetCloneTransform();
        }
    }

    private void Update() {
        if (!autopilotEnabled) {
            AutoPilot();
        }
        speedLimit = Mathf.Lerp(speedLimit, targetSpeedLimit, 0.15f * Time.deltaTime);
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
        throttle = Mathf.Pow(speedLimit, 2) - rb.velocity.sqrMagnitude;
        wheelVehicle.Throttle = throttle;

        distOnPath += (transform.position - prevPos).magnitude;
        prevPos = transform.position;
        float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);

        if (autopilotEnabled) {
            wheelVehicle.InstantSetWheelAngle(angle);
        } else {
            wheelVehicle.SetAutoAngle(angle);
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
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
        speedLimit = pathBehaviour.GetSpeedLimit();
        targetSpeedLimit = speedLimit;
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
        Debug.Log($"car ai, active car has reached portal");
        //StartCoroutine(WaitToChangeToClone());
        ChangeToClone();
    }

    private IEnumerator WaitToChangeToClone() {
        Debug.Log($"wait to change to clone");
        yield return new WaitForSeconds(Time.deltaTime * 2);
        Debug.Log($"wait to change to clone 2");
        ChangeToClone();
    }

    public void PortalReachedActiveCar() {
        if (!dontLoop) {
            levelManagement.CheckIfNextLevel();
            carManagement.ActiveCarHasReachedPortal(this);
        }
    }

    public void PortalReachedInactiveCar() {
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
        Debug.Log("car ai change to clone");
        carManagement.ChangeToClone(!autopilotEnabled, this, clone);
    }

    public void MakeMainCar(bool isActiveCar) {
        isClone = false;
        if (isActiveCar) cam.MakeMainCarCamera();
        carVisuals.SetDangle(dangleNr);
    }

    public void StartClone(bool isActiveCar, Transform transform, Rigidbody rb, CarAI carAI, Vector3 startPos, Vector3 startDir, int pathID, int myCar, int v, int dangleNr) {
        Debug.Log("start clone");
        autopilotEnabled = !isActiveCar;
        dontLoop = true;
        wheelVehicle.IsPlayer = isActiveCar;
        originalCarTransform = transform;
        originalCarRB = rb;
        originalCarAI = carAI;
        startTunnel = carAI.startTunnel;
        endTunnel = carAI.endTunnel;
        this.PathID = pathID;
        this.dangleNr = dangleNr;
        carVisuals = GetComponent<CarVisuals>();
        carVisuals.SetCarVisuals(myCar, v);
        carVisuals.UpdateVisuals(isActiveCar);
        if (isActiveCar) {
            Debug.Log("is active car, set active car values");
            switchingBehaviour.SetActiveCarValues();
        } else {
            switchingBehaviour.SetInactiveCarValues();
        }

        this.startPos = startPos;
        this.startDir = startDir;

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
        distOnPath = myPath.path.GetClosestDistanceAlongPath(pos);
        prevPos = TransformPointToStart(originalCarAI.GetPrevPos());
    }

    private void CreateClone(int pathID) {
        hasClone = true;
        clone = referenceManagement.carManagement.CreateCarClone(pathID);
        clone.StartClone(!autopilotEnabled, transform, rb, this, startPos, startDir, pathID, (int)carVisuals.myCar, carVisuals.variation, carVisuals.dangleNr);
        if (!autopilotEnabled) CreateCloneCam();
    }

    private void CreateCloneCam() {
        Vector3 pos = TransformPointToStart(cam.transform.position);
        Vector3 dir = TransformDirectionToStart(cam.transform.forward);
        GameObject cloneCamObj = Instantiate(referenceManagement.camPrefab, pos, Quaternion.LookRotation(dir));
        Camera1stPerson cloneCam = cloneCamObj.GetComponent<Camera1stPerson>();
        cloneCam.SetAsCloneCam();
        cloneCam.SwitchCar(clone.switchingBehaviour.camTranslateTarget.transform, clone.switchingBehaviour.camRotTarget.transform, true, clone.transform);
        clone.cam = cloneCam;
    }

    public void ActiveCarHasEnteredTunnel(int pathID) {
        if (!dontLoop) {
            inTunnelWithActiveCar = true;
            CreateClone(pathID);
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


    public void SlowDown() {
        Debug.Log("Slow down");
        targetSpeedLimit = 1;
    }
}