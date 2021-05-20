using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyManagement : MonoBehaviourReferenced {
    PathManagement pathManagement;
	SwitchingManagement switchingManagement;
    CarManagement carManagement;
    int nrOfSwitches;

    int currentLevelPathID = -1;
    int levelID = 0;

    GameObject alternatePath0;

    List<CarAI> fallenCars = new List<CarAI>();
    bool hasFallenCars;
    Transform[] fallTargets;
    Transform activeCarFallTargets;

    SwitchingBehaviour activeCar;

    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        switchingManagement.CarChangedEvent.AddListener(HandleSwitched);
        pathManagement = referenceManagement.pathManagement;
        carManagement = referenceManagement.carManagement;
        fallTargets = referenceManagement.fallTargets;
        activeCarFallTargets = referenceManagement.activeCarFallTarget;

        alternatePath0 = referenceManagement.alternatePath0;
        activeCar = switchingManagement.ActiveCar;
    }

    private void OnDisable() {
        switchingManagement.CarChangedEvent.RemoveListener(HandleSwitched);
    }

    private void HandleSwitched() {
        nrOfSwitches++;
        EvaluateSwitch();
        activeCar = switchingManagement.ActiveCar;
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


    private void ToLevel1() {
        pathManagement.GetMyPath(0).TransitionToNextLevel();
        journeyState = JourneyState.Transition_Intro_1;
    }


    public void SpeedUpAfterJam() {
        List<CarAI> carAIs = carManagement.GetAllCarAIs();
        for (int i = 0; i < carAIs.Count; i++) {
            carAIs[i].StartCar();
            carAIs[i].endOfPathInstruction = PathCreation.EndOfPathInstruction.Stop;
        }
    }

    public void StopAllCars() {
        List<CarAI> carAIs = carManagement.GetAllCarAIs();
        for (int i = 0; i < carAIs.Count; i++) {
            if (carAIs[i].autopilotEnabled) {
                carAIs[i].StopCar();
            }
        }
    }

    private void Update() {
        if (hasFallenCars) {
            activeCar.GetCarAI().SetKinematic(true);
            activeCar.transform.position = Vector3.Lerp(activeCar.transform.position, activeCarFallTargets.transform.position, 0.2f * Time.deltaTime);
            activeCar.transform.rotation = Quaternion.Slerp(activeCar.transform.rotation, activeCarFallTargets.transform.rotation, 0.2f * Time.deltaTime);
            for (int i = 0; i < fallenCars.Count && i < fallTargets.Length; i++) {
                Debug.Log($"fallen car = {i}");
                fallenCars[i].SetKinematic(true);
                fallenCars[i].transform.position = Vector3.Lerp(fallenCars[i].transform.position, fallTargets[i].position, 0.2f * Time.deltaTime);
                fallenCars[i].transform.rotation = Quaternion.Slerp(fallenCars[i].transform.rotation, fallTargets[i].rotation, 0.2f * Time.deltaTime);
            }
        }
    }

    public void StartCarsFalling(CarAI carAI) {
        fallenCars.Add(carAI);
        hasFallenCars = true;
    }

    public enum JourneyState { Intro, Transition_Intro_1, Level1, Transition_1_21, Level21, Transition_21_22, Level22, Transition_22_31, Level31, Transition_31_32, Level32, Transition_32_4, Level4, Outro }

    [SerializeField]
    private JourneyState journeyState = JourneyState.Intro;
}