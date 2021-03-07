using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using VehicleBehaviour;


public class SwitchingBehaviour : MonoBehaviourReferenced {
    private CarAI carAI;
    private WheelVehicle wheelVehicle;
    private GameObject viewCone;
    private SwitchingManagement switchingManagement;
    private PathCreator myPath;

    private PathBehaviour pathBehaviour;

    public MeshRenderer meshRenderer;
    public Material visibleMat;
    public Material invisibleMat;
    public Material windowMat;

    public GameObject camRotTarget;
    public GameObject camTranslateTarget;

    public int id;

    public bool isInitialPlayer;

    private void Start() {
        wheelVehicle = GetComponent<WheelVehicle>();
        wheelVehicle.IsPlayer = isInitialPlayer;
        carAI = GetComponent<CarAI>();
        switchingManagement = referenceManagement.switchingManagement;
        id = switchingManagement.allSwitchingBehaviours.Count;
        switchingManagement.AddToAllSwitchingBehaviours(this);
        viewCone = GetComponentInChildren<ViewCone>().gameObject;
        if (isInitialPlayer) {
            SwitchIntoCar();
        }
        else SwitchOutOfCar();
    }

    public void SwitchIntoCar() {
        carAI.SwitchOffAutopilot();
        viewCone.SetActive(true);
        ChangeColorToInvisible();
        switchingManagement.activeCar = this;
        wheelVehicle.IsPlayer = true;
    }

    public void SwitchOutOfCar() {
        carAI.SwitchOnAutopilot();
        viewCone.SetActive(false);
        wheelVehicle.IsPlayer = false;
    }

    public void CarBecomesVisible(SwitchingBehaviour car) {
        switchingManagement.AddToEligibleSwitchingBehaviours(car);
    }

    public void CarBecomesInvisible(SwitchingBehaviour car) {
        switchingManagement.RemoveFromEligibleSwitchingBehaviours(car);
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