#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

public class ScreenshotGrabber
{
    [MenuItem("Screenshot/Grab")]
    public static void Grab()
    {
        ScreenCapture.CaptureScreenshot("screenshot-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
    }
}
#endif