using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyManagement : MonoBehaviourReferenced {
    PathManagement pathManagement;
	SwitchingManagement switchingManagement;
    int nrOfSwitches;

    GameObject alternatePath0;

    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        switchingManagement.CarSwitchedEvent.AddListener(HandleSwitched);
        introLoopTrigger = referenceManagement.introLoopTrigger;
        pathManagement = referenceManagement.pathManagement;

        alternatePath0 = referenceManagement.alternatePath0;
    }

    private void OnDisable() {
        switchingManagement.CarSwitchedEvent.RemoveListener(HandleSwitched);
    }

    private void HandleSwitched() {
        nrOfSwitches++;
        EvaluateSwitch();
    }

    private void EvaluateSwitch() {

        // into done
        if (nrOfSwitches == 1) {
            introDone = true;
            IntroDone();
        }
    }

    // INTRO
    private bool introDone;
    private GameObject introLoopTrigger;

	public void IntroLoopTrigger(CarAI carAI) {
		if (!introDone) {
			carAI.SetToStartConfig();
        }
    }

    private void IntroDone() {
        introLoopTrigger.SetActive(false);
    }

    // LEVEL 1

    public void Gate0Trigger(CarAI carAI) {
        if (!carAI.autopilotEnabled) {
            CloseGate0();
        }
    }

    private void CloseGate0() {
        switchingManagement.ActiveCar.GetCarAI().PathID = 1;
        alternatePath0.SetActive(true);
        pathManagement.GetAllPaths()[0].gameObject.SetActive(false);
    }
	
}