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

    private bool camIsOn = true;
    Camera cam;

    private bool switching = false;
    

    private void Start() {
        cam = gameObject.GetComponent<Camera>();
        //translateTarget = referenceManagement.initialCarSB.camTranslateTarget.transform;
        //rotTarget = referenceManagement.initialCarSB.camRotTarget.transform;
        prevPos = transform.position;
    }

    private void FixedUpdate() {
        InertiaMovement();
    }

    public void SwitchCar(Transform tt, Transform rt) {
        translateTarget = tt;
        rotTarget = rt;
        switching = true;
        prevTargetPos = translateTarget.position;
    }

    private void SwitchingMovement() {
        targetPos = translateTarget.position;
        targetVelocity = targetPos - prevTargetPos;
        prevTargetPos = targetPos;

        Vector3 currentVelocity = transform.position - prevPos;
        prevPos = transform.position;

        Vector3 distToTarget = targetPos - transform.position;
        catchUpVelocity = distToTarget;

        transform.position = Vector3.SmoothDamp(transform.position, transform.position + targetVelocity + catchUpVelocity, ref currentVelocity, 0.05f);

        if ((transform.position - translateTarget.position).sqrMagnitude <= 0.05f) {
            switching = false;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, rotTarget.rotation, 0.05f);
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
}
