using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera1stPerson : MonoBehaviourReferenced
{
    public Transform rotTarget;
    public Transform translateTarget;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;

    private Vector3 velocity = new Vector3(0, 0, 0);


    private Vector3 targetVelocity;
    private Vector3 targetPos;
    private Vector3 prevTargetPos;
    private Vector3 catchUpVelocity = Vector3.zero;

    private Vector3 prevPos;

    [SerializeField] private float velocityDamping = 0.1f;
    [SerializeField] private float catchUpDamping = 0.1f;

    private bool inRegion = true;

    Camera cam;
    AudioListener audioListener;

    private int frameCounter = 0;

    private int loopFrames = 5;
    private int loopCounter = 0;
    private bool looping = false;
    private Vector3 loopOffset = Vector3.zero;
    private Vector3 loopTargetPos = Vector3.zero;

    private bool isCloneCam;

    float targetRange = 2;
    bool isInTargetRange;
    public bool IsInTargetRange {
        get { return isInTargetRange; }
    }

    private void OnEnable() {
        cam = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();
        prevPos = transform.position;
    }

    private void FixedUpdate() {
        InertiaMovement();
        if (looping) {
            transform.position = loopTargetPos;
            HandleRotation();
            looping = false;
        }
    }

    private void Update() {
        isInTargetRange = (translateTarget.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(targetRange, 2) ? true : false;
    }

    private IEnumerator StopLooping() {
        loopCounter = loopFrames;
        while (loopCounter > 0) {
            loopCounter--;
            yield return new WaitForFixedUpdate();
        }
        looping = false;
    }

    public void SwitchCar(Transform tt, Transform rt) {
        translateTarget = tt;
        rotTarget = rt;
        prevTargetPos = translateTarget.position;
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


    private void InertiaMovement() {
        HandleTranslation();
        HandleRotation();
    }

    private void HandleTranslation() {
        targetPos = translateTarget.position;

        // Velocity
        targetVelocity = targetPos - prevTargetPos;
        velocity = Vector3.Lerp(velocity, targetVelocity, velocityDamping * Time.deltaTime);
        if (!inRegion) {
            velocity = targetVelocity;
        }

        // Catch Up
        Vector3 distToTarget = targetPos - transform.position;
        catchUpVelocity = distToTarget / catchUpDamping;

        // Set Camera X
        Vector3 newCamPos = transform.position + velocity + catchUpVelocity;
        transform.position = newCamPos;

        // Set prev to get Velocity on next frame
        prevTargetPos = targetPos;
    }

    private void HandleRotation() {
        var direction = rotTarget.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("PlayerRegion")) {
            inRegion = false;
        } 
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("PlayerRegion")) {
            inRegion = true;
        }
    }

    public void Loop(Vector3 endPos) {
        loopTargetPos = endPos;
        prevTargetPos = endPos;
        looping = true;
    }

    public Vector3 GetCurrentOffset() {
        return translateTarget.position - transform.position;
    }
}
