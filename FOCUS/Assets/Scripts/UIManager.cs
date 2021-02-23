using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Text textTime;

    [SerializeField]
    private Text textTargets;

    [SerializeField]
    private Text textReachedEnd;


    private bool timeCounterIsRunning = true;
    private int targetsReached = 0;

    public void StopTimeCounter() {
        timeCounterIsRunning = false;
    }

    public void ReachedEnd() {
        StopTimeCounter();
        textReachedEnd.enabled = true;
    }

    public void ReachedTarget() {
        targetsReached += 1;
        textTargets.text = "Targets: " + targetsReached;
    }

    private void Update() {

        if(timeCounterIsRunning) {
            textTime.text = Time.time.ToString();
        }


        
    }
}
