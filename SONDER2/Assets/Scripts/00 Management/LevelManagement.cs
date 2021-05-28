using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class LevelManagement : MonoBehaviourReferenced {

    PathManagement pathManagement;
    List<PathBehaviour> allPathBehaviours;
    GameObject[] levels;
    HDRISky sky;
    Volume skyAndFog;
    Cubemap[] cubeMaps;
    SkyData[] skyData;
    JourneyManagement journeyManagement;
    
    CarManagement carManagement;

    bool inEndTunnel = false;

    public int levelNr = 0;

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
        ChangeLevel();
    }

    public void EnteredEndTunnel(int startPathID, int endPathID) {
        Debug.Log("entered end tunnel");
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

        if (levelNr == 5) {
            journeyManagement.StopAllCars();
        }
    }

    public void EnterOutroLevel() {
        Debug.Log("enter outro level");
        levelNr = levels.Length - 1;

        foreach (PathBehaviour pb in allPathBehaviours) {
            pb.gameObject.SetActive(true);
        }
        ChangeLevel();
        List<CarAI> outroCars = journeyManagement.GetOutroCars();
        Debug.Log($"outro cars = {outroCars.Count}");
        foreach (CarAI car in outroCars) {
            car.transform.SetParent(levels[levelNr].transform);
        }
    }
}

[System.Serializable]
public class SkyData {
    public Cubemap cubemap;
    public float lux;
}