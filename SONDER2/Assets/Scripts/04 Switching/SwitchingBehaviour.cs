using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using VehicleBehaviour;
using UnityEngine.Rendering.HighDefinition;
using TMPro;


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

    public HDAdditionalLightData headlight1;
    public HDAdditionalLightData headlight2;
    private float activeCarVolumetric = 0.5f;
    private float inactiveCarVolumetric = 3.5f;

    public int id;

    public bool isInitialCar = false;

    public TMP_Text gearText;

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
        gearText.gameObject.SetActive(switchingManagement.HasMarkedCar);

        SetHeadlightsActiveCar();
    }

    public void SwitchOutOfCar() {
        carAI.SwitchOnAutopilot();
        carAI.cam = null;
        wheelVehicle.IsPlayer = false;
        gearText.gameObject.SetActive(false);

        SetHeadlightsInactiveCar();
    }

    public void SetHeadlightsActiveCar() {
        headlight1.volumetricDimmer = activeCarVolumetric;
        headlight2.volumetricDimmer = activeCarVolumetric;
    }

    public void SetHeadlightsInactiveCar() {
        headlight1.volumetricDimmer = inactiveCarVolumetric;
        headlight2.volumetricDimmer = inactiveCarVolumetric;
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

    public int GetGear() {
        return carAI.CurrentGear;
    }
}