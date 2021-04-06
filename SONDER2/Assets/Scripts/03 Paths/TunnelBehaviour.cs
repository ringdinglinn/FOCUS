using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelBehaviour : MonoBehaviourReferenced {
    public List<CarAI> carsInTunnel = new List<CarAI>();
    private int id;

    private int endTunnelID;

    private void Start() {
        referenceManagement.pathManagement.AddToTunnels(this);
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
            bool carInList = false;
            foreach (CarAI car in carsInTunnel) if (car == carAI) carInList = true;
            Debug.Log("Trigger entered");
            if (!carInList && !carAI.dontLoop) carsInTunnel.Add(carAI);
            if (!carAI.autopilotEnabled) {
                foreach (CarAI car in carsInTunnel) {
                    Debug.Log("Trigger entered foreach");
                    car.ActiveCarHasEnteredTunnel();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            CarAI carAI;
            carAI = other.GetComponentInParent<CarAI>();
            carAI.dontLoop = false;
            if (carAI.PathID == endTunnelID) {
                carsInTunnel.Remove(carAI);
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