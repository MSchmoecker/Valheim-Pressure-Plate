using System;
using UnityEngine;

namespace PressurePlate {
    public class AudioBinder : MonoBehaviour{
        private void Awake() {
            GetComponent<AudioSource>().outputAudioMixerGroup = AudioMan.instance.m_ambientMixer;
        }
    }
}
