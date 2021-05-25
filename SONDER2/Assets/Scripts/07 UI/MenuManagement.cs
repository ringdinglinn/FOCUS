using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManagement : MonoBehaviourReferenced {
    public Image menu0;
    public Image[] menuItems;
    public Image controlsImg;
    public Image titleImg;
    public Image creditsImg;
    public Image waitForSignalImg;

    InputManagement inputManagement;

    int index = 0;
    int total = 4;

    bool upPressed;
    bool downPressed;
    bool enterPressed;
    bool escPressed;

    float startClarity = 1.3f;
    float defaultClarity = 0.013f;
    float changedClarity = 0.8f;
    float currentClarity = 0;

    AsyncOperation asyncLoad;
    bool titleDone = false;

    bool menuActive = true;
    bool creditsActive = false;

    FMOD.Studio.EventInstance radioStatic;
    FMOD.Studio.EventInstance menuClicks;
    FMOD.Studio.EventInstance menuAmbience;
    FMOD.Studio.EventInstance menuSelect;
    FMOD.Studio.EventInstance menuTitle;

    void Start() {
        inputManagement = referenceManagement.inputManagement;
        currentClarity = startClarity;
        radioStatic = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.radioStatic);
        menuClicks = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.menuClicks);
        menuAmbience = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.menuAmbience);
        menuSelect = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.menuSelect);
        menuTitle = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.menuTitle);
        radioStatic.start();
        menuAmbience.start();
    }

    void OnDisable() {
        radioStatic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        menuAmbience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        foreach (Image img in menuItems) {
            img.material.SetFloat("Clarity", defaultClarity);
        }
    }

    void Update() {
        if (menuActive) {
            if (!inputManagement.GetInputButton(Inputs.down) && downPressed) {
                ++index;
                index += total;
                index %= total;
                ChangeSelected();
            }
            else if (!inputManagement.GetInputButton(Inputs.up) && upPressed) {
                --index;
                index += total;
                index %= total;
                ChangeSelected();
            }

            downPressed = inputManagement.GetInputButton(Inputs.down);
            upPressed = inputManagement.GetInputButton(Inputs.up);

            currentClarity = Mathf.Lerp(currentClarity, defaultClarity, 0.3f);
            menuItems[index].material.SetFloat("Clarity", currentClarity);
            Debug.Log($"currentClarity = {currentClarity}");
            float rs = Mathf.InverseLerp(defaultClarity, changedClarity, currentClarity);
            rs = 1 - rs;
            rs *= 2;
            Debug.Log($"radioStatic = {rs}");
            radioStatic.setParameterByName("SignalStrength", rs);

            if (!inputManagement.GetInputButton(Inputs.enter) && enterPressed) {
                menuSelect.start();
                switch (index) {
                    case 0:
                        Play();
                        break;
                    case 1:
                        break;
                    case 2:
                        Credits();
                        break;
                    case 3:
                        Quit();
                        break;
                }
            }

            enterPressed = inputManagement.GetInputButton(Inputs.enter);
        } else if (creditsActive) {
            if (!inputManagement.GetInputButton(Inputs.esc) && escPressed) {
                BackToMenu();
            }

            escPressed = inputManagement.GetInputButton(Inputs.esc);
        }
    }

    void ChangeSelected() {
        menuClicks.start();
        foreach (Image img in menuItems) {
            img.gameObject.SetActive(false);
        }
        menuItems[index].gameObject.SetActive(true);
        currentClarity = changedClarity;
    }

    void Play() {
        menuActive = false;
        StartCoroutine(StartInfo());
    }

    IEnumerator StartInfo() {
        menuItems[index].gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        controlsImg.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        controlsImg.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        titleImg.gameObject.SetActive(true);
        menuTitle.start();
        yield return new WaitForSeconds(3.5f);
        titleImg.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        waitForSignalImg.gameObject.SetActive(true);
        SceneManager.LoadScene("samplescene5");
    }

    void Credits() {
        menuActive = false;
        menuItems[index].gameObject.SetActive(false);
        creditsImg.gameObject.SetActive(true);
        creditsActive = true;
    }

    void BackToMenu() {
        creditsActive = false;
        menuActive = true;
        creditsImg.gameObject.SetActive(false);
        menuItems[index].gameObject.SetActive(true);
        currentClarity = changedClarity;
    }

    void Quit() {
        menuActive = false;
        Application.Quit();
    }

    IEnumerator LoadAsyncScene() {
        asyncLoad = SceneManager.LoadSceneAsync("samplescene5");
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone) {
            yield return null;
        }
        if (titleDone) {
            asyncLoad.allowSceneActivation = true;
        }
    }
}