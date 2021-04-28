﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalIndicator : MonoBehaviourReferenced {

    SwitchingManagement switchingManagement;
    public Transform activeCar;

    private void Start() {
        activeCar = switchingManagement.ActiveCar.transform;
    }

    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        switchingManagement.CarChangedEvent.AddListener(ActiveCarChanged);
    }

    private void OnDisable() {
        switchingManagement.CarChangedEvent.RemoveListener(ActiveCarChanged);
    }

    private void ActiveCarChanged() {
        activeCar = switchingManagement.ActiveCar.transform;
    }

    private void Update() {
        transform.rotation = Quaternion.LookRotation(activeCar.position - transform.position);
    }
}