using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SwitchingManagement : MonoBehaviourReferenced {

    public List<SwitchingBehaviour> allSwitchingBehaviours;
    private GearManagement gearManagement;
    public SwitchingBehaviour activeCar;
    private SwitchingBehaviour selectedCar;
    private SwitchingBehaviour markedCar;
    public SwitchingBehaviour MarkedCar {
        get {   return markedCar; }
        set {   if (markedCar != value) MarkedCarChanged(value);
                markedCar = value;
        }
    }

    private Camera cam;
    private Camera1stPerson camController;
    private GameObject switchImgObj;
    private RectTransform switchImgTransform;
    private Image switchImg;
    private GameObject perceptionBorderObj;
    private RectTransform perceptionBorderTransform;

    private float switchUIPadding = 10;

    private float perceptionW;
    private float perceptionH;
    private float perceptionR;
    private List<SwitchingBehaviour> SBsInFrame = new List<SwitchingBehaviour>();

    private CarManagement carManagement;

    private bool selectCarNow = false;

    private bool hasMarkedCar = false;
    public bool HasMarkedCar {
        get { return hasMarkedCar; }
        set { if (value != hasMarkedCar) MarkedCarValueChanged(value);
            hasMarkedCar = value;
        }
    }
    private bool hasSelectedCar = false;
    private bool canSelectCar = true;

    bool carsCreated;

    public UnityEvent CarSwitchedEvent;

    private void Start() {
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnBeatSubD.AddListener(OnSubDBeatDetected);
        beatDetector.bdOnBeatFull.AddListener(OnFullBeatDetected);
        camController = referenceManagement.cam;
        cam = camController.GetComponent<Camera>();
        switchImgObj = referenceManagement.switchImgObj;
        switchImgTransform = switchImgObj.GetComponent<RectTransform>();
        switchImg = switchImgObj.GetComponent<Image>();
        perceptionBorderObj = referenceManagement.perceptionBorderObj;
        perceptionBorderTransform = perceptionBorderObj.GetComponent<RectTransform>();
        perceptionW = referenceManagement.switchViewWidth;
        perceptionH = referenceManagement.switchViewHeight;
        perceptionR = referenceManagement.switchViewRange;
        allSwitchingBehaviours = referenceManagement.carManagement.GetAllSwitchingBehaviours();
        gearManagement = referenceManagement.gearManagement;
    }

    private void OnEnable() {
        referenceManagement.carManagement.cameraChanged.AddListener(OnCameraChanged);
        referenceManagement.carManagement.carsCreated.AddListener(OnCarsCreated);
    }

    private void OnDisable() {
        referenceManagement.carManagement.cameraChanged.RemoveListener(OnCameraChanged);
        referenceManagement.carManagement.carsCreated.RemoveListener(OnCarsCreated);

    }

    private void OnCarsCreated() {
        carsCreated = true;
        allSwitchingBehaviours = referenceManagement.carManagement.GetAllSwitchingBehaviours();
    }

    private void OnCameraChanged() {
        cam = referenceManagement.cam.GetComponent<Camera>();
        camController = cam.GetComponent<Camera1stPerson>();
    }

    private void SwitchToCar(SwitchingBehaviour newSB) {
        activeCar.SwitchOutOfCar();
        referenceManagement.cam.SwitchCar(newSB.camTranslateTarget.transform, newSB.camRotTarget.transform);
        newSB.SwitchIntoCar(camController);
    }

    public void SetUpInitialCar(SwitchingBehaviour initSB) {
        referenceManagement.cam.SwitchCar(initSB.camTranslateTarget.transform, initSB.camRotTarget.transform);
        initSB.isInitialCar = true;
        initSB.SwitchIntoCar(camController);
    }

    private void Update() {

        if (carsCreated) {
            SearchForCars();
        }

        if (HasMarkedCar && !hasSelectedCar) {
            HandleSwitchGUI(CarScreenPos(MarkedCar), Color.white);
        }

        if (/*GetInput("SwitchCar") && */ !hasSelectedCar && canSelectCar) {
            if (HasMarkedCar && MarkedCar.GetGear() == gearManagement.CurrentGear) {
                selectCarNow = true;
            }
        }

        if (selectCarNow) {
            SelectCar();
            selectCarNow = false;
        }
    }

    private void SearchForCars() {
        SBsInFrame.Clear();
        MarkedCar = null;
        // 1 Draw Perception Frame
        float w = cam.pixelWidth * perceptionW;
        float h = cam.pixelHeight * perceptionH;
        float originX = (cam.pixelWidth / 2) - w / 2;
        float originY = (cam.pixelHeight / 2) - h / 2;
        perceptionBorderTransform.anchoredPosition = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
        perceptionBorderTransform.sizeDelta = new Vector2(cam.pixelWidth * perceptionW, cam.pixelHeight * perceptionH);

        float dist = Mathf.Infinity;
        for (int i = 0; i < allSwitchingBehaviours.Count; i++) {
            if (allSwitchingBehaviours[i] != activeCar) {
                // check if other car is visible
                bool isVisible = allSwitchingBehaviours[i].meshRenderer.IsVisibleFrom(cam);
                float[,] scrPos = CarScreenPos(allSwitchingBehaviours[i]);
                //check if other car is close enough
                bool inRange = scrPos[4, 0] <= perceptionR ? true : false;
                //check if points are in perceptionFrame
                bool inFrame = false;
                for (int j = 0; j < 4; j++) {
                    if (scrPos[j,0] > originX && scrPos[j,0] < originX + w && scrPos[j,1] > originY && scrPos[j, 1] < originY + h) {
                        inFrame = true;
                    }
                }
                if (isVisible && inRange && inFrame) {
                    SBsInFrame.Add(allSwitchingBehaviours[i]);
                    //evaluate closest car
                    if (scrPos[4,0] <= dist) {
                        dist = scrPos[4, 0];
                        MarkedCar = allSwitchingBehaviours[i];
                    }
                }
            }
        }
        HasMarkedCar = markedCar != null ? true : false;
    }

    private void MarkedCarValueChanged(bool b) {
        switchImgObj.SetActive(b);
        activeCar.gearText.gameObject.SetActive(b);
    }

    private void MarkedCarChanged(SwitchingBehaviour sb) {
        if (sb != null) {
            activeCar.gearText.text = sb.GetGear().ToString();
        }
    }

    private void HandleSwitchGUI(float [,] scrPos1, Color col) {
        //get x1, y1, x2, y2
        float[] scrPos = new float[] { scrPos1[0, 0], scrPos1[0, 1], scrPos1[3, 0], scrPos1[3, 1] };
        float x = scrPos[0];
        float y = scrPos[1];
        float w = scrPos[2] - x;
        float h = scrPos[3] - y;

        float padding = switchUIPadding;

        switchImg.color = col;
        switchImgTransform.anchoredPosition = new Vector2(x - padding, y - padding);
        switchImgTransform.sizeDelta = new Vector2(w + 2 * padding, h + 2 * padding);
    }

    private float[,] CarScreenPos(SwitchingBehaviour sb) {
        Vector3[] boundingVerts;
        boundingVerts = sb.boxCollider.GetVerticesOfBoxCollider();

        float sumZ = 0;
        for (int i = 0; i < 8; i++) {
            boundingVerts[i] = cam.WorldToScreenPoint(boundingVerts[i]);
            sumZ += boundingVerts[i].z;
        }

        float x1 = Mathf.Infinity;
        float y1 = Mathf.Infinity;
        float x2 = 0;
        float y2 = 0;
        for (int i = 0; i < 8; i++) {
            y1 = boundingVerts[i].y < y1 ? boundingVerts[i].y : y1;
            x1 = boundingVerts[i].x < x1 ? boundingVerts[i].x : x1;
            x2 = boundingVerts[i].x > x2 ? boundingVerts[i].x : x2;
            y2 = boundingVerts[i].y > y2 ? boundingVerts[i].y : y2;
        }

        float averageZ = sumZ / 8;

        return new float[,] { { x1, y1 }, { x1, y2 }, { x2, y1 }, { x2, y2 } , { averageZ, 0 } };
    }

    private void SelectCar() {
        hasSelectedCar = true;
        selectedCar = markedCar;
        referenceManagement.selectedSwitchCar.Play();
        canSelectCar = false;
    }

    private void DeselectCar() {
        selectedCar = null;
        hasSelectedCar = false;
    }

    private IEnumerator SelectCoolDown() {
        while (!camController.IsInTargetRange) {
            yield return new WaitForEndOfFrame();
        }
        canSelectCar = true;
    }

    private void Switch() {
        SwitchToCar(selectedCar);
        activeCar = selectedCar;
        DeselectCar();
        referenceManagement.switchSound.Play();
        selectedCar = null;
        StartCoroutine(SelectCoolDown());
        CarSwitchedEvent.Invoke();
    }

    bool switchInUse = false;
    private bool GetInput(string input) {
        if (referenceManagement.inputManagement.GetInput(input) != 0) {
            if (switchInUse == false) {
                switchInUse = true;
                return true;
            }
            else {
                return false;
            }
        } else {
            switchInUse = false;
            return false;
        }
    }

    private void OnFullBeatDetected() {
        if (hasSelectedCar) Switch();
    }

    private void OnSubDBeatDetected() {
        //if (hasSelectedCar) Switch();
    }

    public void SetActiveCar(SwitchingBehaviour activeCar) {
        this.activeCar = activeCar;
    }
}