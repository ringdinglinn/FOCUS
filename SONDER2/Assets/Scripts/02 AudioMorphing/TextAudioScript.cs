using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class TextAudioScript : MonoBehaviourReferenced {

    TextMeshPro m_TextMeshPro;
    TextMeshProUGUI m_TextMeshProUGUI;
    TextContainer m_TextContainer;

    Vector3[] newVertexPositions;
    Vector3[] origVertexPositions;
    public AnimationCurve curve;

    bool started = false;

    List<Vector3> randomPoints = new List<Vector3>();
    List<float> strengths = new List<float>();

    int minRandPoints = 3;
    int maxRandPoints = 5;

    float randRange = 7f;
    float yMean = 0;
    float defaultOffsetLength = 7f;


    private void OnEnable() {
        m_TextMeshPro = GetComponent<TextMeshPro>() ?? gameObject.AddComponent<TextMeshPro>();
        m_TextMeshProUGUI = GetComponent<TextMeshProUGUI>();
        m_TextMeshPro.alignment = TextAlignmentOptions.Center;
        m_TextContainer = GetComponent<TextContainer>();
    }

    private void Start() {
        StartCoroutine(GetVertices());
        referenceManagement.beatDetector.bdOnEighth.AddListener(OnBeat);
    }

    private void OnBeat() {
        Skew();
    }

    private void GenerateRandomPoints() {
        randomPoints.Clear();
        strengths.Clear();
        int n = Random.Range(minRandPoints, maxRandPoints + 1) - 2;
        float x0;
        float x1;
        x0 = origVertexPositions[0].x;
        x1 = origVertexPositions[origVertexPositions.Length - 5].x;
        randomPoints.Add(new Vector3(x0, yMean + Random.Range(-randRange, randRange), 0));
        randomPoints.Add(new Vector3(x1, yMean + Random.Range(-randRange, randRange), 0));
        for (int i = 0; i < n; i++) {
            float randX = Random.Range(x0, x1);
            float randY = yMean + Random.Range(-randRange, randRange);
            randomPoints.Add(new Vector3(randX, randY, 0));
        }
        for (int i = 0; i < n + 2; i++) {
            strengths.Add(Random.Range(-1f, 1f));
        }
    }

    private void GetYMean() {
        float sum = 0;
        for (int i = 0; i < origVertexPositions.Length; i++) {
            sum += origVertexPositions[i].y;
        }
        sum /= origVertexPositions.Length;
        yMean = sum;
    }

    private IEnumerator GetVertices() {
        while (origVertexPositions == null) {
            origVertexPositions = m_TextMeshPro.textInfo.meshInfo[0].vertices;
            newVertexPositions = origVertexPositions;
            yield return null;
        }
        Debug.Log($"orig vertices = {m_TextMeshPro.textInfo.meshInfo[0].vertices}");
        started = true;
        GetYMean();
        //Skew();
    }

    private Vector3[] TransformVertices(Vector3[] vertices) {
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }
        return vertices;

    }
 
    private void Skew() {
        if (started) {
            GenerateRandomPoints();

            for (int i = 0; i < origVertexPositions.Length; i++) {
                float dist = Mathf.Infinity;
                int index = -1;
                for (int j = 0; j < randomPoints.Count; j++) {
                    if ((randomPoints[j] - origVertexPositions[i]).sqrMagnitude < dist) {
                        dist = (randomPoints[j] - origVertexPositions[i]).sqrMagnitude;
                        index = j;
                    }
                }
                Vector3 offset = randomPoints[index] - origVertexPositions[i];
                offset.Normalize();
                offset *= defaultOffsetLength;
                newVertexPositions[i] = origVertexPositions[i] + offset;
            }

            m_TextMeshPro.mesh.vertices = newVertexPositions;
            m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo[0].uvs0;
            m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo[0].uvs2;
            m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo[0].colors32;
        }
    }

    IEnumerator AnimateVertexPositionsVII() {

        Vector3[] newVertexPositions;
        //Matrix4x4 matrix;

        int loopCount = 0;

        while (true) {
            m_TextMeshPro.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
            m_TextMeshPro.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

            TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
            int characterCount = textInfo.characterCount;


            newVertexPositions = textInfo.meshInfo[0].vertices;

            for (int i = 0; i < characterCount; i++) {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                float offsetY = 0;                  

                newVertexPositions[vertexIndex + 0].y += offsetY;
                newVertexPositions[vertexIndex + 1].y += offsetY;
                newVertexPositions[vertexIndex + 2].y += offsetY;
                newVertexPositions[vertexIndex + 3].y += offsetY;

            }

            loopCount += 1;

            // Upload the mesh with the revised information
            m_TextMeshPro.mesh.vertices = newVertexPositions;
            m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo[0].uvs0;
            m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo[0].uvs2;
            m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo[0].colors32;

            yield return new WaitForSeconds(0.025f);
        }
    }
}