using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalIndicator : MonoBehaviourReferenced {

    SwitchingManagement switchingManagement;
    Transform activeCar;

    private void Start() {
        switchingManagement = referenceManagement.switchingManagement;
        activeCar = switchingManagement.ActiveCar.transform;
    }

    private void OnEnable() {
        switchingManagement.CarSwitchedEvent.AddListener(ActiveCarChanged);
    }

    private void OnDisable() {
        switchingManagement.CarSwitchedEvent.RemoveListener(ActiveCarChanged);
    }

    private void ActiveCarChanged() {
        activeCar = switchingManagement.ActiveCar.transform;
    }

    private void Update() {
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(activeCar.forward, Vector3.up));
    }
}