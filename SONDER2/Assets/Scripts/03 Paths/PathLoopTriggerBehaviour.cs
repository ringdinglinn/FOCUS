using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLoopTriggerBehaviour : MonoBehaviourReferenced {
	private CarAI carAI;
    private bool isColliding;

    private TunnelBehaviour tunnelBehaviour;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            carAI = other.gameObject.GetComponentInParent<CarAI>();
            if (carAI.autopilotEnabled) {
                carAI.PortalReachedInactiveCar();
            } else if (!carAI.autopilotEnabled && !tunnelBehaviour.isLevelStartTunnel) {
                carAI.PortalReachedActiveCar();
            }
        }
    }

    public void SetTunnelBehaviour(TunnelBehaviour tunnelBehaviour) {
        this.tunnelBehaviour = tunnelBehaviour;
    }
}