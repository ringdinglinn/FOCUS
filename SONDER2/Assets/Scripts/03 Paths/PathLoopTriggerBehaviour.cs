using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLoopTriggerBehaviour : MonoBehaviourReferenced {
	private CarAI carAI;
    private bool isColliding;

    private TunnelBehaviour tunnelBehaviour;

    private void OnTriggerEnter(Collider other) {
        Debug.Log("should portal");
        if (other.gameObject.CompareTag("Car")) {
            if (isColliding) return;
            isColliding = true;
            carAI = other.gameObject.GetComponentInParent<CarAI>();
            Debug.Log($"entered portal = {carAI.autopilotEnabled}");
            if (carAI.autopilotEnabled) {
                Debug.Log($"inactive car portal, tunnel name = {tunnelBehaviour}");
                carAI.PortalReachedInactiveCar();
            } else if (!carAI.autopilotEnabled && !tunnelBehaviour.isLevelStartTunnel && !tunnelBehaviour.isLevelEndTunnel) {
                Debug.Log($"active car portal, tunnel name = {tunnelBehaviour}");
                carAI.PortalReachedActiveCar();
            }
            StartCoroutine(SetIsCollidingFalse());
        }
    }

    private IEnumerator SetIsCollidingFalse() {
        yield return new WaitForFixedUpdate();
        isColliding = false;
    }

    public void SetTunnelBehaviour(TunnelBehaviour tunnelBehaviour) {
        this.tunnelBehaviour = tunnelBehaviour;
    }
}