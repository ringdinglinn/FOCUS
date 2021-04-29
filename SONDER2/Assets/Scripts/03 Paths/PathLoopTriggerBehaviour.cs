using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLoopTriggerBehaviour : MonoBehaviourReferenced {
	private CarAI carAI;
    private bool isColliding;

    private TunnelBehaviour tunnelBehaviour;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            Debug.Log($"portal reached, {other.transform.parent.parent.name}");
            if (isColliding) return;
            isColliding = true;
            carAI = other.gameObject.GetComponentInParent<CarAI>();
            if (carAI.autopilotEnabled) {
                carAI.PortalReachedInactiveCar();
            } else if (!carAI.autopilotEnabled && !tunnelBehaviour.isLevelStartTunnel) {
                Debug.Log($"portal reached, start command, {other.transform.parent.parent.name}");
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