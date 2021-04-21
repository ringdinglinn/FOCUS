using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera1stPerson : MonoBehaviourReferenced {
    public Transform rotTarget;
    public Transform translateTarget;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;

    private Vector3 velocity = new Vector3(0, 0, 0);


    private Vector3 targetVelocity;
    private Vector3 targetPos;
    private Vector3 prevTargetPos;
    private Vector3 catchUpVelocity = Vector3.zero;
    private Vector3 targetAcc;
    private Vector3 prevTargetVel;

    private Vector3 prevPos;

    [SerializeField] private float velocityDamping = 0.1f;
    [SerializeField] private float catchUpDamping = 0.1f;

    private bool inRegion = true;

    Camera cam;
    AudioListener audioListener;

    private bool looping = false;

    bool shake = false;

    private bool isCloneCam;

    [Range(0,0.02f)]
    public float shakeRange = 0.005f;

    float targetRange = 4f;
    bool isInTargetRange;
    public bool IsInTargetRange {
        get { return isInTargetRange; }
    }

    private void OnEnable() {
        cam = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();
        prevPos = transform.position;
    }

    private void Update() {
        if (looping) {
            BaseMovement();
            if ((translateTarget.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(targetRange, 2)) {
                looping = false;
            }
        } else {
            BaseMovement();
        }
    }

    public void SwitchCar(Transform tt, Transform rt, bool shake) {
        translateTarget = tt;
        rotTarget = rt;
        prevTargetPos = translateTarget.position;
        this.shake = shake;
    }

    public void SetAsCloneCam() {
        isCloneCam = true;
        cam.depth = -1;
        //cam.enabled = false;
        audioListener.enabled = false;
    }

    public void MakeMainCarCamera() {
        isCloneCam = false;
        cam.depth = 1;
        cam.enabled = true;
        audioListener.enabled = true;
    }

    private void BaseMovement() {
        HandleTranslation();
        HandleRotation();
    }

    private void LoopMovement() {
        Sloop();
        HandleRotation();
    }

    private void Sloop() {
        targetPos = translateTarget.position;

        // Velocity
        targetVelocity = targetPos - prevTargetPos;
        velocity = Vector3.Lerp(velocity, targetVelocity, velocityDamping * Time.deltaTime);

        // Catch Up
        Vector3 distToTarget = targetPos - (transform.position + velocity);
        catchUpVelocity = Vector3.Lerp(catchUpVelocity, distToTarget, catchUpDamping * Time.deltaTime);

        // Set Camera
        Vector3 newCamPos = transform.position + velocity + catchUpVelocity;
        transform.position = newCamPos;

        // Set prev to get Velocity on next frame
        prevTargetPos = targetPos;
    }

    private void HandleTranslation() {
        transform.position = translateTarget.position;
        if (shake) Shake();
    }

    private void HandleRotation() {
        var direction = rotTarget.transform.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 0.15f);
    }

    private void Shake() {
        Vector3 shake = new Vector3(Random.Range(-shakeRange, shakeRange), Random.Range(-shakeRange, shakeRange), Random.Range(-shakeRange, shakeRange));
        transform.position += shake;
    }

    public void Loop() {
        looping = true;
    }

    public bool GetLooping() {
        return looping;
    }

    public Vector3 GetCurrentOffset() {
        return translateTarget.position - transform.position;
    }
}
