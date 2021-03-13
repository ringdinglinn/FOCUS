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

    private int frameCounter = 0;

    private int loopFrames = 5;
    private int loopCounter = 0;
    private bool looping = false;
    private Vector3 loopOffset = Vector3.zero;
    private Vector3 loopTargetPos = Vector3.zero;

    private void Start() {
        cam = gameObject.GetComponent<Camera>();
        prevPos = transform.position;
    }

    private void FixedUpdate() {
        InertiaMovement();
        if (looping) {
            Debug.Log($"cam loop, frameCounter: {frameCounter}");
            transform.position = loopTargetPos - loopOffset;
            looping = false;
            //Debug.Break();
        }
        Debug.Log($"frameCounter: {frameCounter++}");
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
        Debug.Log($"handle translation, frameCounter: {frameCounter}");
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

    public void Loop(Vector3 startPos, Vector3 endPos) {
        loopOffset = startPos - transform.position;
        loopTargetPos = endPos;
        looping = true;
    }
}
