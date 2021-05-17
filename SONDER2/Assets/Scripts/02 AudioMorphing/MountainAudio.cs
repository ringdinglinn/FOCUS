using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainAudio : MonoBehaviourReferenced {

    MeshRenderer meshRenderer;
    float noiseOffset;
    float persistance;
    float deltaPersistance = 0.2f;
    float maxHeight;
    float deltaMaxHeight = 0.02f;

    Texture2D noiseTex;

    int width = 1024;
    int height = 1024;

    int octaves = 10;
    float lacunarity = 2.0f;

    private void Start() {
        referenceManagement.beatDetector.bdOnBar.AddListener(HandleBar);
        referenceManagement.beatDetector.bdOnHalf.AddListener(HandleHalf);
        referenceManagement.beatDetector.bdOnFourth.AddListener(HandleFourth);
        meshRenderer = GetComponent<MeshRenderer>();

        noiseTex = CreateTex();
    }

    Texture2D CreateTex() {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        // set the pixel values
        texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
        texture.SetPixel(1, 0, Color.clear);
        texture.SetPixel(0, 1, Color.white);
        texture.SetPixel(1, 1, Color.black);

        // Apply all SetPixel calls
        texture.Apply();

        return texture;
    }

    private void HandleBar() {

    }

    private void HandleHalf() {
    }

    private void HandleFourth() {
        SetValues();

    }

    void SetValues() {
        noiseOffset += 10;
        persistance = 0.3f;
        maxHeight = 3.3f;
    }

    private void Update() {
        meshRenderer.material.SetFloat("_Persistance", -1f * Mathf.Pow(persistance - 1f,2) + 1);
        meshRenderer.material.SetFloat("_NoiseOffset", noiseOffset);
        meshRenderer.material.SetFloat("_MaxHeight", maxHeight);

        persistance -= deltaPersistance * Time.deltaTime;
        persistance = Mathf.Clamp(persistance, 0, 1);

        maxHeight -= deltaMaxHeight * Time.deltaTime;
        maxHeight = Mathf.Clamp(maxHeight, 0, 3.3f);
    }

}