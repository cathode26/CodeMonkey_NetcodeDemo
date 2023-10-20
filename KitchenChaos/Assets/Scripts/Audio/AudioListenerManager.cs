// AudioListenerManager Class
using GameSignalList;
using UnityEngine;

public class AudioListenerManager : MonoBehaviour
{
    private AudioListener audioListener;
    private Transform defaultParent;
    private void Awake()
    {
        audioListener = GetComponent<AudioListener>();
        defaultParent = transform.parent;

        Signals.Get<SetAudioListenerSignal>().AddListener(SetAudioListener);
        Signals.Get<ReturnAudioListenerSignal>().AddListener(ReturnAudioListener);
    }
    private void OnDestroy()
    {
        Signals.Get<SetAudioListenerSignal>().RemoveListener(SetAudioListener);
        Signals.Get<ReturnAudioListenerSignal>().RemoveListener(ReturnAudioListener);
    }
    private void SetAudioListener(Transform newParent)
    {
        audioListener.transform.SetParent(newParent);
        // Optionally reset local position and rotation to keep the listener directly at the new parent's position
        audioListener.transform.localPosition = Vector3.zero;
        audioListener.transform.localRotation = Quaternion.identity;
    }
    private void ReturnAudioListener()
    {
        audioListener.transform.SetParent(defaultParent);
    }
}