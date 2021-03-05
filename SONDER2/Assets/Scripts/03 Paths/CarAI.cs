using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;
using PathCreation;

public class CarAI : MonoBehaviourReferenced {

    private WheelVehicle carConroller;
    private PathCreator myPath;
    private SwitchingBehaviour switchingBehaviour;
    private Rigidbody rb;

    public bool autopilotEnabled;

    private Vector3 startPos;
    private Vector3 startDir;
    private Vector3 offset = new Vector3(0, 2f, 0);

    private float distOnPath;

    private Vector3 prevPos;

    public bool started = false;

    private float steerTowardsDist = 2;


    private void Start() {
        carConroller = GetComponent<WheelVehicle>();
        switchingBehaviour = GetComponent<SwitchingBehaviour>();
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb);
        Debug.Log("id = " + switchingBehaviour.id);
    }

    private void CreateStartConfig() {
        //distOnPath = Random.Range(0f, myPath.path.length);
        distOnPath = 0;
        startPos = myPath.path.GetPointAtDistance(distOnPath, EndOfPathInstruction.Loop);
        startPos += offset;
        startDir = myPath.path.GetDirectionAtDistance(distOnPath, EndOfPathInstruction.Loop);
        SetToStartConfig();
    }

    public void SetToStartConfig() {
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

    public void SetPath(PathCreator p) {
        myPath = p;
        CreateStartConfig();
    }

    public void SwitchOnAutopilot() {
        autopilotEnabled = true;
    }

    public void SwitchOffAutopilot() {
        autopilotEnabled = false;
    }

    public void EndReached() {
        Debug.Log("end reached");
        Debug.Log("end reached, started = " + started);
        if (started) Loop();
    }

    private void Loop() { 
        Debug.Log("end Loop");
        distOnPath = 0;
        transform.position = myPath.path.GetPointAtDistance(distOnPath);
        rb.velocity = Vector3.zero;
        float angle = Vector3.SignedAngle(transform.forward, myPath.path.GetPointAtDistance(distOnPath + steerTowardsDist, EndOfPathInstruction.Loop) - transform.position, Vector3.up);
        carConroller.InstantSetWheelAngle(angle);
        transform.rotation = Quaternion.LookRotation(myPath.path.GetDirectionAtDistance(distOnPath));
    }
}