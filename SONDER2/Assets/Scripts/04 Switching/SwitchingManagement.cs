using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchingManagement : MonoBehaviourReferenced {

    public List<SwitchingBehaviour> allSwitchingBehaviours = new List<SwitchingBehaviour>();
    public List<SwitchingBehaviour> eligibleSwitchtingBehaviours = new List<SwitchingBehaviour>();
    public SwitchingBehaviour activeCar;
    private SwitchingBehaviour selectedCar;

    private Camera cam;
    private GameObject switchImgObj;
    private RectTransform switchImgTransform;

    private bool canSwitch = true;

    private bool gonnaSwitch = false;

    private void Start() {
        //AudioProcessor processor = referenceManagement.audioProcessor;
        //processor.onBeat.AddListener(OnBeatDetected);
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnBeatSubD.AddListener(OnSubDBeatDetected);
        beatDetector.bdOnBeatFull.AddListener(OnFullBeatDetected);
        cam = referenceManagement.cam.GetComponent<Camera>();
        switchImgObj = referenceManagement.switchImgObj;
        switchImgTransform = switchImgObj.GetComponent<RectTransform>();
    }

    public void AddToAllSwitchingBehaviours(SwitchingBehaviour sb) {
        allSwitchingBehaviours.Add(sb);
    }

    public void AddToEligibleSwitchingBehaviours(SwitchingBehaviour sb) {
        eligibleSwitchtingBehaviours.Add(sb);
        sb.ChangeColorToVisible();
    }

    public void RemoveFromEligibleSwitchingBehaviours(SwitchingBehaviour sb) {
        eligibleSwitchtingBehaviours.Remove(sb);
        sb.ChangeColorToInvisible();
    }

    private void SwitchToCar(SwitchingBehaviour newSB) {
        activeCar.SwitchOutOfCar();
        eligibleSwitchtingBehaviours.Clear();
        foreach (SwitchingBehaviour sb in eligibleSwitchtingBehaviours) {
            sb.ChangeColorToInvisible();
        }
        referenceManagement.cam.SwitchCar(newSB.camTranslateTarget.transform, newSB.camRotTarget.transform);
        newSB.SwitchIntoCar();
    }

    public void SetUpInitialCar(SwitchingBehaviour initSB) {
        referenceManagement.cam.SwitchCar(initSB.camTranslateTarget.transform, initSB.camRotTarget.transform);
        initSB.isInitialCar = true;
        initSB.SwitchIntoCar();
    }

    private void Update() {
        if (GetInput("SwitchCar") != 0) {
            if (eligibleSwitchtingBehaviours.Count != 0 && canSwitch) {
                SelectSwitchCar();
            }
        }

        if (gonnaSwitch) {
            bool inSight = false;
            foreach (SwitchingBehaviour sb in eligibleSwitchtingBehaviours) {
                if (sb == selectedCar) inSight = true;
            }
            if (!inSight) {
                DeselectSwitchCar();
                return;
            }
            DrawSwitchGUI();
        }
    }

    private void SelectSwitchCar() {
        gonnaSwitch = true;
        selectedCar = eligibleSwitchtingBehaviours[0];
        EnableSwitchGUI();
        referenceManagement.selectedSwitchCar.Play();
    }

    private void DeselectSwitchCar() {
        selectedCar = null;
        gonnaSwitch = false;
        DisableSwitchGUI();
    }

    private void Switch() {
        SwitchToCar(selectedCar);
        StartCoroutine(SwitchCoolDown());
        DeselectSwitchCar();
        referenceManagement.switchSound.Play();
    }

    IEnumerator SwitchCoolDown() {
        canSwitch = false;
        yield return new WaitForSeconds(2);
        canSwitch = true;
    }

    private float GetInput(string input) {
        return referenceManagement.inputManagement.GetInput(input);
    }

    private void OnFullBeatDetected() {
        //if (gonnaSwitch) Switch();
    }

    private void OnSubDBeatDetected() {
        if (gonnaSwitch) Switch();
    }

    private void EnableSwitchGUI() {
        switchImgObj.SetActive(true);
        DrawSwitchGUI();
    }

    private void DisableSwitchGUI() {
        switchImgObj.SetActive(false);
    }

    private void DrawSwitchGUI() {
        Vector3 bounds = selectedCar.meshRenderer.bounds.extents;
        Vector3 pos = selectedCar.meshRenderer.bounds.center;
        Vector3[] boundingVerts = new Vector3[8];
        float factorX = 1;
        float factorY = 1;
        float factorZ = 1;
        for (int i = 0; i < 8; i++) {
            factorZ = i % 2 == 0 ? 1 : -1;
            factorY = (int)(i / 2) % 2 == 0 ? 1 : -1;
            factorX = (int)(i / 4) % 2 == 0 ? 1 : -1;
            boundingVerts[i] = pos + new Vector3(factorX * bounds.x, factorY * bounds.y, factorZ * bounds.z);
            boundingVerts[i] = cam.WorldToScreenPoint(boundingVerts[i]);
        }

        float x1 = Mathf.Infinity;
        float y1 = Mathf.Infinity;
        float x2 = 0;
        float y2 = 0;
        for (int i = 0; i < 8; i++) {
            y1 = boundingVerts[i].y < y1 ? boundingVerts[i].y : y1;
            x1 = boundingVerts[i].x < x1 ? boundingVerts[i].x : x1;
            x2 = boundingVerts[i].x > x2 ? boundingVerts[i].x : x2;
            y2 = boundingVerts[i].x > y2 ? boundingVerts[i].y : y2;
        }

        float w = x2 - x1;
        float h = y2 - y1;

        switchImgTransform.anchoredPosition = new Vector2(x1, y1);
        switchImgTransform.sizeDelta = new Vector2(w, h);
    }
}