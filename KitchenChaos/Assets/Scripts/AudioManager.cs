using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField]
    private AudioClipRefsSO audioClipRefsSO;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("AudioManager is a singleton");
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
        PlaySound(audioClipRefsSO.deliveryFail, Player.Instance.transform.position);
    }
    private void DeliveryManager_OnRecipeSuccessChanged()
    {
        PlaySound(audioClipRefsSO.deliverySuccess, Player.Instance.transform.position); ;
    }
    private void CuttingCounter_OnChop()
    {
        PlaySound(audioClipRefsSO.chop, Player.Instance.transform.position);
    }
    private void Player_OnObjectDropChanged()
    {
        PlaySound(audioClipRefsSO.objectDrop, Player.Instance.transform.position);
    }
    private void Player_OnObjectPickupChanged()
    {
        PlaySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);
    }
    private void TrashCounter_OnAnyObjectTrashed()
    {
        PlaySound(audioClipRefsSO.trash, Player.Instance.transform.position);
    }
    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1.0f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume );
    }
    private void PlaySound(AudioClip[] audioClips, Vector3 position, float volume = 1.0f)
    {
        System.Guid guid = System.Guid.NewGuid();
        byte[] bytes = guid.ToByteArray();
        int seed = System.BitConverter.ToInt32(bytes, 0);
        Random.InitState(seed);
        AudioSource.PlayClipAtPoint(audioClips[Random.Range(0, audioClips.Length)], position, volume);
    }
    public void PlayFootStepSound()
    {
        PlaySound(audioClipRefsSO.footstep, Player.Instance.transform.position);
    }
}
