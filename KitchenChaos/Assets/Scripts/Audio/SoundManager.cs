using SoundSignalList;
using UnityEngine;

public class SoundManager : MonoBehaviour // Responsible for managing the game's sound effects, including volume control
{
    private const string PLAYER_PREF_SOUND_EFFECTS_VOLUME = "SOUND_EFFECTS_VOLUME";
    [SerializeField]
    private AudioClipRefsSO audioClipRefsSO;
    private int volume = 10;

    private void Awake()
    {
        Signals.Get<OnChangeSoundEffectVolumeSignal>().AddListener(ChangeVolume);
        Signals.Get<OnRecipeSuccessSignal>().AddListener(OnRecipeSuccess);
        Signals.Get<OnRecipeFailedSignal>().AddListener(OnRecipeFailed);
        Signals.Get<OnChoppedSignal>().AddListener(OnChopped);
        Signals.Get<OnObjectPickupSignal>().AddListener(OnObjectPickup);
        Signals.Get<OnObjectDropSignal>().AddListener(OnObjectDrop);
        Signals.Get<OnAnyObjectTrashedSignal>().AddListener(OnAnyObjectTrashed);
        Signals.Get<OnFootStepsSignal>().AddListener(OnFootSteps);
        Signals.Get<OnCountdownSignal>().AddListener(OnCountdown);
        Signals.Get<OnWarningSignal>().AddListener(OnWarning);
    }
    private void OnDestroy()
    {
        Signals.Get<OnChangeSoundEffectVolumeSignal>().RemoveListener(ChangeVolume);
        Signals.Get<OnRecipeSuccessSignal>().RemoveListener(OnRecipeSuccess);
        Signals.Get<OnRecipeFailedSignal>().RemoveListener(OnRecipeFailed);
        Signals.Get<OnChoppedSignal>().RemoveListener(OnChopped);
        Signals.Get<OnObjectPickupSignal>().RemoveListener(OnObjectPickup);
        Signals.Get<OnObjectDropSignal>().RemoveListener(OnObjectDrop);
        Signals.Get<OnAnyObjectTrashedSignal>().RemoveListener(OnAnyObjectTrashed);
        Signals.Get<OnFootStepsSignal>().RemoveListener(OnFootSteps);
        Signals.Get<OnCountdownSignal>().RemoveListener(OnCountdown);
        Signals.Get<OnWarningSignal>().RemoveListener(OnWarning);
    }
    private void OnEnable()
    {
        volume = PlayerPrefs.GetInt(PLAYER_PREF_SOUND_EFFECTS_VOLUME, volume);
        Signals.Get<OnSoundEffectsVolumeChangedSignal>().Dispatch(volume);
    }
    private void OnRecipeSuccess(Vector3 position)
    {
        PlaySound(audioClipRefsSO.deliverySuccess, position);
    }
    private void OnRecipeFailed(Vector3 position)
    {
        PlaySound(audioClipRefsSO.deliveryFail, position);
    }
    private void OnChopped(Vector3 position)
{
        PlaySound(audioClipRefsSO.chop, position);
    }
    private void OnObjectPickup(Vector3 position)
    {
        PlaySound(audioClipRefsSO.objectPickup, position);
    }
    private void OnObjectDrop(Vector3 position)
    {
        PlaySound(audioClipRefsSO.objectDrop, position);
    }
    private void OnAnyObjectTrashed(Vector3 position)
    {
        PlaySound(audioClipRefsSO.trash, position);
    }
    private void OnFootSteps(Vector3 position)
    {
        PlaySound(audioClipRefsSO.footstep, position);
    }
    private void OnCountdown()
    {
        PlaySound(audioClipRefsSO.warning, new Vector3(0,0,0));
    }
    private void OnWarning(Vector3 position)
    {
        PlaySound(audioClipRefsSO.warning, position);
    }
    private void ChangeVolume()
    {
        volume += 1;
        if (volume > 10f)
            volume = 0;
        PlayerPrefs.SetInt(PLAYER_PREF_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
        Signals.Get<OnSoundEffectsVolumeChangedSignal>().Dispatch(volume);
    }
    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1.0f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier);
    }
    private void PlaySound(AudioClip[] audioClips, Vector3 position, float volumeMultiplier = 1.0f)
    {
        System.Guid guid = System.Guid.NewGuid();
        byte[] bytes = guid.ToByteArray();
        int seed = System.BitConverter.ToInt32(bytes, 0);
        Random.InitState(seed);
        AudioSource.PlayClipAtPoint(audioClips[Random.Range(0, audioClips.Length)], position, GetVolume() * volumeMultiplier);
    }
    public float GetVolume()
    {
        return volume / 10.0f;
    }
}
