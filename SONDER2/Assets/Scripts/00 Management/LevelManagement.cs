using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelManagement : MonoBehaviourReferenced {

    PathManagement pathManagement;
    List<PathBehaviour> allPathBehaviours;
    GameObject[] levels;
    HDRISky sky;
    Volume skyAndFog;
    Cubemap[] cubeMaps;
    SkyData[] skyData;
    JourneyManagement journeyManagement;
    SwitchingManagement switchingManagement;

    CarManagement carManagement;

    bool inEndTunnel = false;

    public int levelNr = 0;

    float titleDuration = 5f;
    Image[] titles;
    int titleIndex = 0;

    float[] predelays = new float[]{0,0,0,0,0,0, 5, 3};
    float[] delays = new float[] { 5, 10, 10, 10, 10, 10, 5, 5 };

    private void Start() {
        pathManagement = referenceManagement.pathManagement;
        allPathBehaviours = pathManagement.GetAllPaths();
        carManagement = referenceManagement.carManagement;
        levels = referenceManagement.levels;
        skyAndFog = referenceManagement.skyAndFog;
        skyAndFog.profile.TryGet(out sky);
        cubeMaps = referenceManagement.skyCubeMaps;
        skyData = referenceManagement.skyData;
        journeyManagement = referenceManagement.journeyManagement;
        titles = referenceManagement.texts;
        switchingManagement = referenceManagement.switchingManagement;
        referenceManagement.musicManagement.announcementDone.AddListener(HandleAnnouncementDone);
        titleIndex = levelNr;
        ChangeLevel();
    }

    public void EnteredEndTunnel(int startPathID, int endPathID) {
        foreach(GameObject level in levels) {
            level.SetActive(true);
        }

        foreach (PathBehaviour pb in allPathBehaviours) {
            if (pb.id != startPathID && pb.id != endPathID) pb.gameObject.SetActive(false);
        }
        inEndTunnel = true;
    }

    public void CheckIfNextLevel() {
        if (inEndTunnel) {

            levelNr++;

            foreach (PathBehaviour pb in allPathBehaviours) {
                pb.gameObject.SetActive(true);
            }
            ChangeLevel();

            inEndTunnel = false;
        }
    }

    private void ChangeLevel() {
        for (int i = 0; i < levels.Length; i++) {
            if (i != levelNr)
            levels[i].SetActive(false);
        }
        levels[levelNr].SetActive(true);

        sky.hdriSky.Override(skyData[levelNr].cubemap);
        sky.desiredLuxValue.Override(skyData[levelNr].lux);

        Camera cam = referenceManagement.cam.GetComponent<Camera>();
        cam.farClipPlane = 800;
        if (levelNr == 3) {
            cam.farClipPlane = 2000;
        } else if (levelNr == 6) {
            cam.farClipPlane = 3000;
        }
        if (levelNr == 5) {
            journeyManagement.StopAllCars();
        }

        if (levelNr != 0) {
            StartCoroutine(ShowTitle());
        }

        switchingManagement.rangeToClosestCar = referenceManagement.ranges[levelNr];
    }

    public void EnterFallLevel() {
        levelNr = levels.Length - 1;

        foreach (PathBehaviour pb in allPathBehaviours) {
            pb.gameObject.SetActive(true);
        }
        ChangeLevel();
        List<CarAI> outroCars = journeyManagement.GetOutroCars();
        foreach (CarAI car in outroCars) {
            car.transform.SetParent(levels[levelNr].transform);
        }
        referenceManagement.musicManagement.StartOutro1();
    }

    public void EnterOutroLevel() {
        StartCoroutine(ShowTitle());
        referenceManagement.voiceClipManagement.StartOutro();
        referenceManagement.musicManagement.StartOutro2();
    }

    void HandleAnnouncementDone() {
        if (levelNr == 0)
        StartCoroutine(ShowTitle());
    }
    

    IEnumerator ShowTitle(){
        switchingManagement.TitleShowing = true;
        yield return new WaitForSeconds(predelays[titleIndex]);
        titles[titleIndex].gameObject.SetActive(true);
    	yield return new WaitForSeconds(delays[titleIndex]);
    	titles[titleIndex].gameObject.SetActive(false);
    	titleIndex++;
        switchingManagement.TitleShowing = false;
    }
}

[System.Serializable]
public class SkyData {
    public Cubemap cubemap;
    public float lux;
}