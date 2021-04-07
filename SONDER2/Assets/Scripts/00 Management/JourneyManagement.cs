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
        if (nrOfSwitches == 1) {
            ToLevel1();
        }
    }


    // LEVEL 1

    private void ToLevel1() {
        pathManagement.GetMyPath(0).TransitionToNextLevel();
        journeyState = JourneyState.Transition_Intro_1;
    }

    public enum JourneyState { Intro, Transition_Intro_1, Level1, Transition_1_21, Level21, Transition_21_22, Level22, Transition_22_31, Level31, Transition_31_32, Level32, Transition_32_4, Level4, Outro }

    [SerializeField]
    private JourneyState journeyState = JourneyState.Intro;
}