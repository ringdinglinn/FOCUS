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


    InputManagement inputManagement;

    int index = 0;
    int total = 4;

    bool upPressed;
    bool downPressed;

    float startClarity = 1.3f;
    float defaultClarity = 0.013f;
    float changedClarity = 0.8f;
    float currentClarity = 0;

    void Start() {
        inputManagement = referenceManagement.inputManagement;
        currentClarity = startClarity;
    }

    void Update() {
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

        if (inputManagement.GetInputButton(Inputs.enter)) {
            switch (index) {
                case 0:
                    Play();
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    Quit();
                    break;
            }
        }
    }

    void ChangeSelected() {
        foreach (Image img in menuItems) {
            img.gameObject.SetActive(false);
        }
        menuItems[index].gameObject.SetActive(true);
        currentClarity = changedClarity;
    }

    void OnDisable() {
        foreach(Image img in menuItems) {
            img.material.SetFloat("Clarity", defaultClarity);
        }
    }

    void Play() {
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
        yield return new WaitForSeconds(4f);
        titleImg.gameObject.SetActive(false);
        SceneManager.LoadScene("samplescene5", LoadSceneMode.Single);
    }

    void Quit() {
        Application.Quit();
    }
}