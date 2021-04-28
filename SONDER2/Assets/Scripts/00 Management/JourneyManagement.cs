using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyManagement : MonoBehaviourReferenced {
    PathManagement pathManagement;
	SwitchingManagement switchingManagement;
    int nrOfSwitches;

    int currentLevelPathID = -1;
    int levelID = 0;

    GameObject alternatePath0;

    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        switchingManagement.CarChangedEvent.AddListener(HandleSwitched);
        pathManagement = referenceManagement.pathManagement;

        alternatePath0 = referenceManagement.alternatePath0;
    }

    private void OnDisable() {
        switchingManagement.CarChangedEvent.RemoveListener(HandleSwitched);
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

    public void NewLevelReached(int id) {
        if (id != currentLevelPathID) {
            levelID++;
            currentLevelPathID = id;
            EvaluateNewLevel();
        }
    }

    private void EvaluateNewLevel() {
        //if (currentLevelPathID == 5) {
        //    StartCoroutine(WaitToQuit());
        //    referenceManagement.youDidItText.SetActive(true);
        //}
    }

    IEnumerator WaitToQuit() {
        yield return new WaitForSeconds(3f);
        Application.Quit();
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