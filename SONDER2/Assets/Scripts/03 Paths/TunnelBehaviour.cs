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

    private int endTunnelID;

    public PathLoopTriggerBehaviour portalTrigger;

    private void Start() {
        referenceManagement.pathManagement.AddToTunnels(this);
        portalTrigger.SetTunnelBehaviour(this);
    }

    public void SetID(int id) {
        this.id = id;
    }

    public void SetEndTunnelID(int id) {
        endTunnelID = id;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            CarAI carAI;
            carAI = other.GetComponentInParent<CarAI>();
            if (isEndTunnel) {
                Debug.Log($"set end tunnel, {carAI.gameObject.name}, {gameObject.name}");
                carAI.startTunnel = startTunnel;
                carAI.endTunnel = this;
            }
            bool carInList = false;
            foreach (CarAI car in carsInTunnel) if (car == carAI) carInList = true;
            if (!carInList && !carAI.dontLoop) carsInTunnel.Add(carAI);
            if (!carAI.autopilotEnabled && !isLevelEndTunnel && !isLevelStartTunnel) {
                foreach (CarAI car in carsInTunnel) {
                    car.ActiveCarHasEnteredTunnel();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            CarAI carAI;
            carAI = other.GetComponentInParent<CarAI>();
            if (this != carAI.endTunnel) {
                carAI.dontLoop = false;
            }
            if (carAI.PathID == endTunnelID) {
                carsInTunnel.Remove(carAI);
            }
            if (!carAI.autopilotEnabled && isLevelStartTunnel) {
                isLevelStartTunnel = false;
                isEndTunnel = false;
            }
        }
    }

    public void CarIsDestroyed(CarAI carAI) {
        carsInTunnel.Remove(carAI);
    }

    public void ActiveCarHasReachedPortal() {
        foreach (CarAI car in carsInTunnel) {
            car.ActiveCarHasEnteredTunnel();
        }
    }

    public List<CarAI> GetCarAIsInTunnel() {
        return carsInTunnel;
    }
}