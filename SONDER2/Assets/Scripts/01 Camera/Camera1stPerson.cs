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

    [SerializeField] private float velocityDamping = 0.1f;
    [SerializeField] private float catchUpDamping = 0.1f;

    private bool inRegion = true;

    private bool camIsOn = true;
    Camera cam;

    private bool switching;
    

    private void Start() {
        cam = gameObject.GetComponent<Camera>();

    }

    private void FixedUpdate() {
        if (!switching) InertiaMovement();
        else SwitchingMovement();
        Debug.Log("switching = " + switching);
    }

    public void SwitchCar(Transform tt, Transform rt) {
        translateTarget = tt;
        rotTarget = rt;
        switching = true;
        prevTargetPos = translateTarget.position;
    }

    private void SwitchingMovement() {
        targetPos = translateTarget.position;

        // Velocity
        targetVelocity = targetPos - prevTargetPos;
        velocity = Vector3.Lerp(velocity, targetVelocity, 0.3f);

        Vector3 pos = Vector3.Lerp(transform.position, targetPos, 0.2f);

        Vector3 distToTarget = targetPos - transform.position;
        catchUpVelocity = distToTarget / catchUpDamping;

        catchUpVelocity = Vector3.ClampMagnitude(catchUpVelocity, (targetPos - pos).magnitude);

        // Set Camera X
        Vector3 newCamPos = pos + catchUpVelocity;


        transform.position = newCamPos;

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
            Debug.Log("out of region");
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


    bool switchCoolDown;

    private void Update() {
        if (referenceManagement.inputManagement.GetInput("SwitchCam") != 0 && !switchCoolDown) {
            camIsOn = !camIsOn;
            switchCoolDown = true;
            StartCoroutine(SwitchCoolDown());
            if (camIsOn) {
                cam.depth = 1;
            } else {
                cam.depth = -1;
            }
        }
    }

    IEnumerator SwitchCoolDown() {
        yield return new WaitForSeconds(0.5f);
        switchCoolDown = false;
    }
}
