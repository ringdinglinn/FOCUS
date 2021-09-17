using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class PauseManagement : MonoBehaviourReferenced {
    InputManagement inputManagement;
    GameObject pauseScreen;
    Volume postProces;
    DepthOfField depthOfField;
    bool paused = false;
    bool restartPressed = false;

    private void Start() {
        inputManagement = referenceManagement.inputManagement;
        pauseScreen = referenceManagement.pauseScreen;
        postProces = referenceManagement.postProcess;
        postProces.profile.TryGet(out depthOfField);
    }

    private void Update() {
        if (inputManagement.GetInputButtonUp(Inputs.pause)) {
            paused = !paused;
            ToggleControls(paused);
        }

        if (inputManagement.GetInputButton(Inputs.restart) && !restartPressed) {
            Restart();
        }
        restartPressed = inputManagement.GetInputButton(Inputs.restart);
    }

    private void ToggleControls(bool b) {
        pauseScreen.SetActive(b);
        if (b) {
            Time.timeScale = 0;
            depthOfField.nearFocusEnd.Override(299);
        } else {
            Time.timeScale = 1;
            depthOfField.nearFocusEnd.Override(3);
        }
    }

    private void Restart() {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}