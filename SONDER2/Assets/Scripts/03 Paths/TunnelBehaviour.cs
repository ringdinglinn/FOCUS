using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelBehaviour : MonoBehaviourReferenced {
    public List<CarAI> carsInTunnel = new List<CarAI>();
    private int id;

    public TunnelBehaviour startTunnel;
    public bool isEndTunnel;
    public bool isLevelEndTunnel;
    public bool isLevelStartTunnel;
    public TunnelBehaviour nextLevelStartTunnel;


    public PathLoopTriggerBehaviour portalTrigger;

    public MeshFilter entryFilter0;
    public MeshFilter entryFilter1;

    public PathBehaviour pathBehaviour;

    bool isColliding = false;

    public GameObject goalIndicator;
    public GameObject nextGoalIndicator;

    SwitchingManagement switchingManagement;
    LevelManagement levelManagement;

    private void OnEnable() {
        referenceManagement.entryFilters.Add(entryFilter0);
        referenceManagement.entryFilters.Add(entryFilter1);
        referenceManagement.pathManagement.AddToTunnels(this);
        portalTrigger.SetTunnelBehaviour(this);
        pathBehaviour = GetComponentInParent<PathBehaviour>();
        switchingManagement = referenceManagement.switchingManagement;
        levelManagement = referenceManagement.levelManagement;
    }

    public void SetID(int id) {
        this.id = id;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            if (isColliding) return;
            isColliding = true;
            StartCoroutine(Reset());

            CarAI carAI;
            carAI = other.GetComponentInParent<CarAI>();

            Debug.Log($"car entered tunnel = {other.name}");

            if (isEndTunnel) {
                carAI.startTunnel = startTunnel;
                carAI.endTunnel = this;
            }
            if (isEndTunnel && isLevelEndTunnel && !carAI.autopilotEnabled) {
                levelManagement.EnteredEndTunnel(pathBehaviour.id, nextLevelStartTunnel.pathBehaviour.id);

                carAI.startTunnel = nextLevelStartTunnel;
                carAI.endTunnel = this;
                foreach (CarAI car in carsInTunnel) {
                    car.startTunnel = nextLevelStartTunnel;
                }
            }
            bool carInList = false;
            foreach (CarAI car in carsInTunnel) if (car == carAI) carInList = true;
            if (!carInList && !carAI.dontLoop && isEndTunnel) carsInTunnel.Add(carAI);

            if (!carAI.autopilotEnabled && isEndTunnel) {

                foreach (CarAI car in carsInTunnel) {
                    Debug.Log($"car = {car}");

                    car.ActiveCarHasEnteredTunnel(isLevelEndTunnel ? nextLevelStartTunnel.pathBehaviour.id : pathBehaviour.id);
                }
                if (goalIndicator != null) {
                    goalIndicator.SetActive(false);
                }
                if (nextGoalIndicator != null) {
                    nextGoalIndicator.SetActive(true);
                }
            }

            if (!carAI.autopilotEnabled && isLevelStartTunnel) {
                carAI.PathID = pathBehaviour.id;
            }

            if (!carAI.autopilotEnabled) {
                switchingManagement.inTunnel = true;
            }
        }
    }

    IEnumerator Reset() {
        yield return new WaitForEndOfFrame();
        isColliding = false;
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            CarAI carAI;
            carAI = other.GetComponentInParent<CarAI>();
            if (this != carAI.endTunnel) {
                carAI.dontLoop = false;
            }
            if (carAI.pathID == pathBehaviour.id) {
                carsInTunnel.Remove(carAI);
            }
            if (!carAI.autopilotEnabled && isLevelStartTunnel) {
                isLevelStartTunnel = false;
                isEndTunnel = false;
            }
            if (!carAI.autopilotEnabled) {
                switchingManagement.inTunnel = false;
            }
        }
    }

    public void CarIsDestroyed(CarAI carAI) {
        carsInTunnel.Remove(carAI);
    }

    public void ActiveCarHasReachedPortal() {
        foreach (CarAI car in carsInTunnel) {
            car.ActiveCarHasEnteredTunnel(isLevelEndTunnel ? nextLevelStartTunnel.pathBehaviour.id : pathBehaviour.id);
        }
    }

    public List<CarAI> GetCarAIsInTunnel() {
        return carsInTunnel;
    }
}