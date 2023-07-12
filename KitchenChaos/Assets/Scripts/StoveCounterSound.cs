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
        stoveCounter.OnPlayerSetObject += StoveCounter_OnPlayerSetObject; ;
    }
    private void OnDisable()
    {
        stoveCounter.OnPlayerSetObject -= StoveCounter_OnPlayerSetObject; ;
    }
    private void StoveCounter_OnPlayerSetObject(object sender, StoveCounter.OnPlayerCookingEventArgs e)
    {
        if (e.state != CookingRecipeSO.State.Burned)
            audioSource.Play();
        else
            audioSource.Stop();
    }
}
