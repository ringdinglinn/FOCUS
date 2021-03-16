using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelBehaviour : MonoBehaviourReferenced {
    private List<CarAI> carsInTunnel = new List<CarAI>();
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
            if (carAI.PathID == endTunnelID) {
                carsInTunnel.Add(carAI);
                if (!carAI.autopilotEnabled) {
                    foreach (CarAI car in carsInTunnel) {
                        car.ActiveCarHasEnteredTunnel();
                    }
                }
            } 
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            CarAI carAI;
            carAI = other.GetComponentInParent<CarAI>();
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