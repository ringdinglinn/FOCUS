﻿using System.Collections;
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

    private int endTunnelID;

    public PathLoopTriggerBehaviour portalTrigger;

    public MeshFilter entryFilter0;
    public MeshFilter entryFilter1;

    public PathBehaviour pathBehaviour;

    private void OnEnable() {
        referenceManagement.entryFilters.Add(entryFilter0);
        referenceManagement.entryFilters.Add(entryFilter1);
    }

    private void Start() {
        referenceManagement.pathManagement.AddToTunnels(this);
        portalTrigger.SetTunnelBehaviour(this);
        pathBehaviour = GetComponentInParent<PathBehaviour>();
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
                carAI.startTunnel = startTunnel;
                carAI.endTunnel = this;
            }
            if (isEndTunnel && isLevelEndTunnel && !carAI.autopilotEnabled) {
                carAI.startTunnel = nextLevelStartTunnel;
                carAI.endTunnel = this;
                foreach (CarAI car in carsInTunnel) {
                    car.startTunnel = nextLevelStartTunnel;
                }
            }
            bool carInList = false;
            foreach (CarAI car in carsInTunnel) if (car == carAI) carInList = true;
            if (!carInList && !carAI.dontLoop) carsInTunnel.Add(carAI);

            if (!carAI.autopilotEnabled && isEndTunnel) {
                foreach (CarAI car in carsInTunnel) {
                    car.ActiveCarHasEnteredTunnel(isLevelEndTunnel ? nextLevelStartTunnel.pathBehaviour.id : pathBehaviour.id);
                }
            }

            if (!carAI.autopilotEnabled && isLevelStartTunnel) {
                carAI.PathID = pathBehaviour.id;
                Debug.Log($"assigned new path, {pathBehaviour.id}, {carAI.name}");
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
            car.ActiveCarHasEnteredTunnel(isLevelEndTunnel ? nextLevelStartTunnel.pathBehaviour.id : pathBehaviour.id);
        }
    }

    public List<CarAI> GetCarAIsInTunnel() {
        return carsInTunnel;
    }
}