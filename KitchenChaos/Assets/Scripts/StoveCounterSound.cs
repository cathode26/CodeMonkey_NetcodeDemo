using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField]
    private StoveCounter stoveCounter;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        stoveCounter.OnPlayerSetObject += StoveCounter_OnPlayerSetObject;
        stoveCounter.OnPlayerRemovedObject += StoveCounter_OnPlayerRemovedObject;
    }
    private void OnDisable()
    {
        stoveCounter.OnPlayerSetObject -= StoveCounter_OnPlayerSetObject;
        stoveCounter.OnPlayerRemovedObject -= StoveCounter_OnPlayerRemovedObject;
    }
    private void StoveCounter_OnPlayerSetObject(object sender, StoveCounter.OnPlayerCookingEventArgs e)
    {
        if (e.state != CookingRecipeSO.State.Burned)
            audioSource.Play();
        else
            audioSource.Stop();
    }
    private void StoveCounter_OnPlayerRemovedObject(object sender, System.EventArgs e)
    {
        audioSource.Stop();
    }
}
