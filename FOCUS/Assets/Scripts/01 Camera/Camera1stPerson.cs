using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera1stPerson : MonoBehaviourReferenced
{
    private Vector3 relDefaultPos;
    [SerializeField] private Transform rotTarget;
    [SerializeField] private Transform translateTarget;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;

    private Vector3 velocity = new Vector3(0, 0, 0);

    private Vector3 prevTargetVel = new Vector3(0,0,0);
    private Vector3 targetAcc = new Vector3(0,0,0);
    private Vector3 offsetVel = new Vector3(0, 0, 0);
    private Vector3 offset = new Vector3(0, 0, 0);
    private Vector3 deltaVelTarget = new Vector3(0, 0, 0);
    private Vector3 prevTargetAcc = new Vector3(0, 0, 0);

    private Vector3[] recordedVels = new Vector3[100];

    private Vector3 targetVelocity;
    private Vector3 targetPos;
    private Vector3 prevTargetPos;
    private Vector3 dist = Vector3.zero;
    private Vector3 catchUpVelocity = Vector3.zero;

    [SerializeField] private float velocityDamping = 0.1f;
    [SerializeField] private float catchUpDamping = 0.1f;

    private bool inRegion = true;

    private bool camIsOn = true;
    Camera cam;

    private void Start() {
        relDefaultPos = transform.position - translateTarget.position;
        cam = gameObject.GetComponent<Camera>();

    }

    private void FixedUpdate() {
        HandleTranslation();
        HandleRotation();
    }

    private void HandleTranslation() {
        //try opt 1 but with own vel

        //// car acceleration
        //targetAcc = (translateTarget.velocity - prevTargetVel) / Time.fixedDeltaTime;
        //prevTargetVel = translateTarget.velocity;
        //// smooth acc
        //targetAcc = Vector3.Lerp(prevTargetAcc, targetAcc, 0.02f);
        //prevTargetAcc = targetAcc;
        //// force acting on driver, opposite to car's acceleration
        //offsetVel -= targetAcc * Time.fixedDeltaTime;
        //Debug.Log("offsetVel = " + offsetVel);
        //// force acting on driver, moving body to default pos
        //offsetVel = Vector3.Lerp(offsetVel, Vector3.zero, 0.6f);
        //// apply
        //transform.localPosition = relDefaultPos + offsetVel;

        targetPos = translateTarget.position;

        // Velocity
        targetVelocity = targetPos - prevTargetPos;
        velocity = Vector3.Lerp(velocity, targetVelocity, velocityDamping * Time.deltaTime);
        Debug.Log("velocity t = " + velocityDamping * Time.deltaTime);
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
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void OnTriggerExit(Collider other) {
        Debug.Log("hi");
        if (other.gameObject.CompareTag("PlayerRegion")) {
            inRegion = false;
        } 
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("PlayerRegion")) {
            inRegion = true;
        }
    }

    private void OnTriggerStay(Collider other) {
        Debug.Log("Hey");
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
