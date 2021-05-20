using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Camera1stPerson : MonoBehaviourReferenced {
    public Transform rotTarget;
    public Transform translateTarget;
    public Transform targetCar;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;


    private Vector3 targetPos;
    private Vector3 startPos;
    private float loopTime;
    private float timer;

    [SerializeField] private float velocityDamping = 0.1f;
    [SerializeField] private float catchUpDamping = 0.1f;

    Camera cam;
    AudioListener audioListener;

    private bool looping = false;

    bool shake = false;

    private bool isCloneCam;

    [Range(0,0.02f)]
    public float shakeRange = 0.003f;

    float targetRange = 4f;
    bool isInTargetRange;
    public bool IsInTargetRange {
        get { return isInTargetRange; }
    }

    SwitchingManagement switchingManagement;

    private void OnEnable() {
        cam = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();
        switchingManagement = referenceManagement.switchingManagement;
    }

    private void Update() {
        if (looping) {
            LoopMovement();
            if ((translateTarget.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(targetRange, 2)) {
                switchingManagement.SwitchDone();
                looping = false;
                timer = 0;
            }
        } else {
            BaseMovement();
        }
    }

    public void SwitchCar(Transform tt, Transform rt, bool shake, Transform tc) {
        translateTarget = tt;
        rotTarget = rt;
        targetCar = tc;
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
        targetPos = targetCar.InverseTransformPoint(translateTarget.position);
        //Vector3 currentPos = targetCar.InverseTransformPoint(transform.position);

        //currentPos = Vector3.Lerp(currentPos, targetPos, 5 * Time.deltaTime);

        //transform.position = targetCar.TransformPoint(currentPos);

        Vector3 startPosLocal = targetCar.InverseTransformPoint(startPos);
        Vector3 currentPos = new Vector3();

        Debug.Log($"loopTime = {loopTime}");

        timer += Time.deltaTime / loopTime;
        if (timer < 1) {
            currentPos = Vector3.Lerp(startPosLocal, targetPos, timer);
        }

        transform.position = targetCar.TransformPoint(currentPos);
    }

    private void HandleTranslation() {
        transform.position = translateTarget.position;
        if (shake) Shake();
    }

    private void HandleRotation() {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotTarget.rotation, 0.15f);
    }

    private void Shake() {
        Vector3 shake = new Vector3(Random.Range(-shakeRange, shakeRange), Random.Range(-shakeRange, shakeRange), Random.Range(-shakeRange, shakeRange));
        transform.position += shake;
    }

    public void Loop(float t) {
        looping = true;
        startPos = transform.position;
        loopTime = t;
    }

    public bool GetLooping() {
        return looping;
    }

    public Vector3 GetCurrentOffset() {
        return translateTarget.position - transform.position;
    }
}
