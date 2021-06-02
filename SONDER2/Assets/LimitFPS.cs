using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }
}
