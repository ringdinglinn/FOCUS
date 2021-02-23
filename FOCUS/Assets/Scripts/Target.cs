using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField]
    private bool isCurrentTarget = false;
    
    [SerializeField]
    private bool isLastTarget = false;

    [SerializeField]
    private Target nextTarget;

    [SerializeField]
    private string vehicleTag = "Vehicle";

    [SerializeField]
    private Material currentTargetMat;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onTargetReached;
    [SerializeField]
    private UnityEvent onLastTarget;

    private Material startMat;
    private Rigidbody vehicle;

    private void Start() {
        startMat = GetComponent<Renderer>().material;
        if(isCurrentTarget) SetToCurrent();
    }

    public void SetToCurrent() {
        isCurrentTarget = true;
        GetComponent<Renderer>().material = currentTargetMat;
    }

    private void SetToLast() {
        isCurrentTarget = false;
        GetComponent<Renderer>().material = startMat;
    }


    private void OnTriggerEnter(Collider collider) {

        if(isCurrentTarget == false) return;
        if(collider.attachedRigidbody == null) return;
        if(collider.attachedRigidbody.CompareTag(vehicleTag) == false) return;

        onTargetReached.Invoke();

        if(isLastTarget) {
            onLastTarget.Invoke();
            return;
        }

        nextTarget.SetToCurrent();
        SetToLast();
    }

}
