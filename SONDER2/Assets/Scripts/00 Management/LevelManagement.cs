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
        ChangeLevel(true);
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
            ChangeLevel(false);

            inEndTunnel = false;
        }
    }

    private void ChangeLevel(bool start) {
        for (int i = 0; i < levels.Length; i++) {
            if (i != levelNr)
            levels[i].SetActive(false);
        }
        levels[levelNr].SetActive(true);

        sky.hdriSky.Override(skyData[levelNr].cubemap);

        if (levelNr == 5) journeyManagement.StopAllCars();
    }
}

[System.Serializable]
public class SkyData {
    public Cubemap cubemap;
    public float exposure;
}