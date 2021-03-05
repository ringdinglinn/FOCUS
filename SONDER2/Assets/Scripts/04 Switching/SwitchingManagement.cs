using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchingManagement : MonoBehaviourReferenced {

    public List<SwitchingBehaviour> allSwitchingBehaviours = new List<SwitchingBehaviour>();
    public List<SwitchingBehaviour> eligibleSwitchtingBehaviours = new List<SwitchingBehaviour>();
    public SwitchingBehaviour activeCar;

    private bool canSwitch = true;

    private bool gonnaSwitch = false;

    private void Start() {
        AudioProcessor processor = referenceManagement.audioProcessor;
        processor.onBeat.AddListener(OnBeatDetected);
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

    private void Update() {
        if (GetInput("SwitchCar") != 0) {
            if (eligibleSwitchtingBehaviours.Count != 0 && canSwitch) {
                gonnaSwitch = true;
            }
        }
    }

    private void Switch() {
        SwitchToCar(eligibleSwitchtingBehaviours[0]);
        StartCoroutine(SwitchCoolDown());
        gonnaSwitch = false;
    }

    IEnumerator SwitchCoolDown() {
        canSwitch = false;
        yield return new WaitForSeconds(2);
        canSwitch = true;
    }

    private float GetInput(string input) {
        return referenceManagement.inputManagement.GetInput(input);
    }

    private void OnBeatDetected() {
        if (gonnaSwitch) Switch();
    }
}