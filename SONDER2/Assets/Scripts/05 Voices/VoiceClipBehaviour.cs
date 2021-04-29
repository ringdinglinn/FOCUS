using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipBehaviour : MonoBehaviourReferenced {
	[SerializeField] List<AudioClip> voiceClips = new List<AudioClip>();
	private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    public void ChangeClip(int i) {
        audioSource.clip = voiceClips[i];
    }

    public void Play() {
        audioSource.Play();
    }

    public void Stop() {
        audioSource.Stop();
    }

    public bool IsPlaying() {
        return audioSource.isPlaying;
    }
}