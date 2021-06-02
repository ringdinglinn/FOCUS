using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyManagement : MonoBehaviourReferenced {
    PathManagement pathManagement;
	SwitchingManagement switchingManagement;
    CarManagement carManagement;
    LevelManagement levelManagement;
    int nrOfSwitches;

    int currentLevelPathID = -1;
    int levelID = 0;

    GameObject alternatePath0;

    List<CarAI> outroCars = new List<CarAI>();
    List<CarAI> fallenCars = new List<CarAI>();
    bool hasFallenCars;
    Transform[] fallTargets;
    Transform activeCarFallTargets;

    CarAI startCar;

    int nrOutroCars = 0;
    int maxNrOutroCars = 8;


    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        switchingManagement.CarChangedEvent.AddListener(HandleSwitched);
        pathManagement = referenceManagement.pathManagement;
        carManagement = referenceManagement.carManagement;
        carManagement.carsCreated.AddListener(HandleCarsInstantiated);
        fallTargets = referenceManagement.fallTargets;
        activeCarFallTargets = referenceManagement.activeCarFallTarget;
        levelManagement = referenceManagement.levelManagement;

        alternatePath0 = referenceManagement.alternatePath0;
    }

    private void OnDisable() {
        switchingManagement.CarChangedEvent.RemoveListener(HandleSwitched);
        carManagement.carsCreated.RemoveListener(HandleCarsInstantiated);
    }

    private void HandleSwitched() {
        nrOfSwitches++;
        EvaluateSwitch();
    }

    private void EvaluateSwitch() {
        Debug.Log("Evaluate Switch");
        if (levelManagement.levelNr == 0 && nrOfSwitches == 1) {
            EnableFirstMovingCar();
        }
    }

    private void DisableFirstMovingCar() {
        SwitchingBehaviour[] sbs = referenceManagement.levels[0].GetComponentsInChildren<SwitchingBehaviour>(true);
        foreach (SwitchingBehaviour sb in sbs) {
            sb.gameObject.SetActive(false);
        }
    }

    private void EnableFirstMovingCar() {
        Debug.Log("Enable First Moving Car");
        SwitchingBehaviour[] sbs = referenceManagement.levels[0].GetComponentsInChildren<SwitchingBehaviour>(true);
        foreach (SwitchingBehaviour sb in sbs) {
            sb.gameObject.SetActive(true);
        }
    }

    private void HandleCarsInstantiated() {
        if (levelManagement.levelNr == 0) {
            DisableFirstMovingCar();
        }
    }

    //public void NewLevelReached(int id) {
    //    if (id != currentLevelPathID) {
    //        levelID++;
    //        currentLevelPathID = id;
    //        EvaluateNewLevel();
    //    }
    //}

    //private void EvaluateNewLevel() {
    //    //if (currentLevelPathID == 5) {
    //    //    StartCoroutine(WaitToQuit());
    //    //    referenceManagement.youDidItText.SetActive(true);
    //    //}
    //}

    //IEnumerator WaitToQuit() {
    //    yield return new WaitForSeconds(3f);
    //    Application.Quit();
    //}


    //private void ToLevel1() {
    //    pathManagement.GetMyPath(0).TransitionToNextLevel();
    //    journeyState = JourneyState.Transition_Intro_1;
    //}


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
            Debug.Log("Lerp in progress");
            startCar.SetKinematic(true);
            startCar.transform.position = Vector3.Lerp(startCar.transform.position, activeCarFallTargets.transform.position, 0.1f * Time.deltaTime);
            startCar.transform.rotation = Quaternion.Slerp(startCar.transform.rotation, activeCarFallTargets.transform.rotation, 0.1f * Time.deltaTime);
            for (int i = 0; i < outroCars.Count && i < fallTargets.Length; i++) {
                if (outroCars[i] != startCar) {
                    outroCars[i].SetKinematic(true);
                    outroCars[i].fallTargetID = i;
                    outroCars[i].transform.position = Vector3.Lerp(outroCars[i].transform.position, fallTargets[i].position, 0.1f * Time.deltaTime);
                    outroCars[i].transform.rotation = Quaternion.Slerp(outroCars[i].transform.rotation, fallTargets[i].rotation, 0.1f * Time.deltaTime);
                }
            }
        }
    }

    public void ActiveCarFell() {
        if (!hasFallenCars) {
            startCar = switchingManagement.ActiveCar.GetCarAI();
            levelManagement.EnterOutroLevel();
            StopAllCars();
            hasFallenCars = true;
        }
    }

    public void AddToOutroCars(CarAI carAI) {
        if (nrOutroCars < maxNrOutroCars) {
            outroCars.Add(carAI);
            nrOutroCars = outroCars.Count;
        }
    }

    public void AddFallenCar(CarAI carAI) {
        fallenCars.Add(carAI);
        --nrOutroCars;
    }

    public List<CarAI> GetOutroCars() {
        return outroCars;
    }

    public enum JourneyState { Intro, Transition_Intro_1, Level1, Transition_1_21, Level21, Transition_21_22, Level22, Transition_22_31, Level31, Transition_31_32, Level32, Transition_32_4, Level4, Outro }

    [SerializeField]
    private JourneyState journeyState = JourneyState.Intro;
}