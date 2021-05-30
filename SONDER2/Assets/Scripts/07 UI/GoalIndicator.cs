using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalIndicator : MonoBehaviourReferenced {

    SwitchingManagement switchingManagement;
    public Transform activeCar;
    private Camera1stPerson cam;

    float prevRot;

    private void Start() {
        activeCar = switchingManagement.ActiveCar.transform;
        cam = referenceManagement.cam;
    }

    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        switchingManagement.CarChangedEvent.AddListener(ActiveCarChanged);
        referenceManagement.carManagement.cameraChanged.AddListener(HandleCameraSwitched);
    }

    private void HandleCameraSwitched() {
        cam = referenceManagement.cam;
    }

    private void OnDisable() {
        switchingManagement.CarChangedEvent.RemoveListener(ActiveCarChanged);
    }

    private void ActiveCarChanged() {
        activeCar = switchingManagement.ActiveCar.transform;
    }

    private void Update() {
        transform.forward = activeCar.position - transform.position;
        transform.localRotation = cam.transform.localRotation;
        transform.RotateAround(transform.position, transform.up, 180);
    }
}