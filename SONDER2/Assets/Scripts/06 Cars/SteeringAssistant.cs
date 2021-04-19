using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;
using PathCreation;

public class SteeringAssistant : MonoBehaviourReferenced {

    SwitchingManagement switchingManagement;
    WheelVehicle wv;
    CarAI carAI;

    [SerializeField]
    float Kp = 1;
    [SerializeField]
    float Kd = 1;
    [SerializeField]
    float Ki = 1;

    float i = 0;

    private SteerAssitMode mode;
    private float angleCorrection;

    private Vector3 pathDir;
    private float deltaPositionAngle;
    private float deltaDirectionAngle;

    private float maxAngle = 15;
    private float steerTowardsDist = 1;
    private float maxDist = 1.5f;
    private float prevDist = 0f;

    private void Start() {
        switchingManagement = referenceManagement.switchingManagement;
        mode = referenceManagement.steerAssitMode;
        wv = GetComponent<WheelVehicle>();
        carAI = GetComponent<CarAI>();
    }

    private void Update() {
        if (!carAI.autopilotEnabled) {
            switch (mode) {
                case SteerAssitMode.Mode1:
                    Mode1();
                    break;
                case SteerAssitMode.Mode2:
                    break;
            }
        }
    }

    private void Mode1() {
        PathCreator path = carAI.pathBehaviour.GetPath();
        deltaDirectionAngle = Vector3.SignedAngle(transform.forward, path.path.GetDirectionAtDistance(path.path.GetClosestDistanceAlongPath(transform.position)), Vector3.up);
    }

    public float GetDirectionAngle() {
        return deltaDirectionAngle;
    }


}

public enum SteerAssitMode { Mode1, Mode2 };