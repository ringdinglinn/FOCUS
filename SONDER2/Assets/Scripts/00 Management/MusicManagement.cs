using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class MusicManagement : MonoBehaviourReferenced {
    // Variables that are modified in the callback need to be part of a seperate class.
    // This class needs to be 'blittable' otherwise it can't be pinned in memory.
    [StructLayout(LayoutKind.Sequential)]
    class TimelineInfo {
        public int currentMusicBar = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
    }

    TimelineInfo timelineInfo;
    GCHandle timelineHandle;

    FMOD.Studio.EVENT_CALLBACK beatCallback;
    FMOD.Studio.EventInstance track1;

    static int beatNr = 0;
    static int beatTotal = 4;
    int switchNr = 0;

    SwitchingManagement switchingManagement;

    private bool playVoiceOverAfterSwitch = false;

    void Start() {

        track1 = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.track1);

        timelineInfo = new TimelineInfo();


        // Explicitly create the delegate object and assign it to a member so it doesn't get freed
        // by the garbage collected while it's being used
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);

        // Pin the class that will store the data modified during the callback
        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        // Pass the object through the userdata of the instance
        track1.setUserData(GCHandle.ToIntPtr(timelineHandle));

        track1.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        track1.start();

        referenceManagement.switchingManagement.CarSwitchDoneEvent.AddListener(HandleCarSwtchDone);
    }

    void OnDestroy() {
        track1.setUserData(IntPtr.Zero);
        track1.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        track1.release();
        timelineHandle.Free();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr) {
        // Retrieve the user data
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);
        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
        if (result != FMOD.RESULT.OK) {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero) {
            // Get the object to store beat and marker details
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            switch (type) {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT: {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        timelineInfo.currentMusicBar = parameter.bar;
                        beatNr++;
                        if (beatNr == beatTotal) StartBeat();
                        beatNr %= beatTotal;
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER: {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                    }
                    break;
            }
        }
        return FMOD.RESULT.OK;
    }

    static void StartBeat() {
        SwitchingManagement switchingManagement = FindObjectOfType<SwitchingManagement>();
        switchingManagement.TimelineBarDetected();
    }

    void HandleCarSwtchDone() {
        if (!playVoiceOverAfterSwitch) ++switchNr;
        track1.setParameterByName("SwitchNr", switchNr);
        playVoiceOverAfterSwitch = false;
    }

    public void SetVolume(float v) {
        track1.setParameterByName("Volume", v);
    }

    public void SetPlayVoiceOverAfterSwitch(bool b) {
        playVoiceOverAfterSwitch = b;
    }
}