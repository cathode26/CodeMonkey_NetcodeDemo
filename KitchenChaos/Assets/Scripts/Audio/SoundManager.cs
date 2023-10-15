using UnityEngine;

public class SoundManager : MonoBehaviour // Singleton class responsible for managing the game's sound effects, including volume control
{
    public static SoundManager Instance { get; private set; } // Singleton instance of the SoundManager class
    private const string PLAYER_PREF_SOUND_EFFECTS_VOLUME = "SOUND_EFFECTS_VOLUME";
    [SerializeField]
    private AudioClipRefsSO audioClipRefsSO;
    private int volume = 10;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("SoundManager is a singleton");

        volume = PlayerPrefs.GetInt(PLAYER_PREF_SOUND_EFFECTS_VOLUME, volume);

    }
    private void OnEnable()
    {
        DeliveryManager.OnRecipeSuccessChanged += DeliveryManager_OnRecipeSuccessChanged;
        DeliveryManager.OnRecipeFailedChanged += DeliveryManager_OnRecipeFailedChanged;
        CuttingCounter.OnChop += CuttingCounter_OnChop;
        Player.OnObjectPickupChanged += Player_OnObjectPickupChanged;
        Player.OnObjectDropChanged += Player_OnObjectDropChanged;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;

    }
    private void OnDisable()
    {
        DeliveryManager.OnRecipeSuccessChanged -= DeliveryManager_OnRecipeSuccessChanged;
        DeliveryManager.OnRecipeFailedChanged -= DeliveryManager_OnRecipeFailedChanged;
        CuttingCounter.OnChop -= CuttingCounter_OnChop;
        Player.OnObjectPickupChanged -= Player_OnObjectPickupChanged;
        Player.OnObjectDropChanged -= Player_OnObjectDropChanged;
        TrashCounter.OnAnyObjectTrashed -= TrashCounter_OnAnyObjectTrashed;
    }
    private void DeliveryManager_OnRecipeFailedChanged()
    {
        //PlaySound(audioClipRefsSO.deliveryFail, Player.Instance.transform.position);
    }
    private void DeliveryManager_OnRecipeSuccessChanged()
    {
        //PlaySound(audioClipRefsSO.deliverySuccess, Player.Instance.transform.position);
    }
    private void CuttingCounter_OnChop()
    {
        //PlaySound(audioClipRefsSO.chop, Player.Instance.transform.position);
    }
    private void Player_OnObjectDropChanged()
    {
        //PlaySound(audioClipRefsSO.objectDrop, Player.Instance.transform.position);
    }
    private void Player_OnObjectPickupChanged()
    {
       // PlaySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);
    }
    private void TrashCounter_OnAnyObjectTrashed()
    {
       // PlaySound(audioClipRefsSO.trash, Player.Instance.transform.position);
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
    public void PlayFootStepSound()
    {
        //PlaySound(audioClipRefsSO.footstep, Player.Instance.transform.position);
    }
    public void PlayCountdownSound()
    {
        //PlaySound(audioClipRefsSO.warning, Player.Instance.transform.position);
    }
    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(audioClipRefsSO.warning, position);
    }
    public void ChangeVolume()
    {
        volume += 1;
        if (volume > 10f)
            volume = 0;
        PlayerPrefs.SetInt(PLAYER_PREF_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }
    public float GetVolume()
    {
        return volume / 10.0f; 
    }
}
