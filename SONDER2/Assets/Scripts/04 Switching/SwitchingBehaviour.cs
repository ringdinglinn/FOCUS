using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using VehicleBehaviour;


public class SwitchingBehaviour : MonoBehaviourReferenced {
    private CarAI carAI;
    private WheelVehicle wheelVehicle;
    private SwitchingManagement switchingManagement;
    private PathCreator myPath;

    private PathBehaviour pathBehaviour;

    public MeshRenderer meshRenderer;
    public Material visibleMat;
    public Material invisibleMat;
    public Material windowMat;

    public BoxCollider boxCollider;

    public GameObject camRotTarget;
    public GameObject camTranslateTarget;

    public int id;

    public bool isInitialCar = false;

    private void OnEnable() {
        CollectReferences();
    }

    public void CollectReferences() {
        wheelVehicle = GetComponent<WheelVehicle>();
        wheelVehicle.IsPlayer = isInitialCar;
        carAI = GetComponent<CarAI>();
        switchingManagement = referenceManagement.switchingManagement;
        id = switchingManagement.allSwitchingBehaviours.Count;
    }

    public void SwitchIntoCar(Camera1stPerson cam) {
        Debug.Log($"assigned cam = {cam}");
        carAI.SwitchOffAutopilot();
        ChangeColorToInvisible();
        carAI.cam = cam;
        switchingManagement.activeCar = this;
        wheelVehicle.IsPlayer = true;
    }

    public void SwitchOutOfCar() {
        carAI.SwitchOnAutopilot();
        carAI.cam = null;
        wheelVehicle.IsPlayer = false;
    }

    public void ChangeColorToVisible() {
        Material[] mats = new Material[2];
        mats[0] = windowMat;
        mats[1] = visibleMat;
        meshRenderer.materials = mats;
    }

    public void ChangeColorToInvisible() {
        Material[] mats = new Material[2];
        mats[0] = windowMat;
        mats[1] = invisibleMat;
        meshRenderer.materials = mats;
    }
}