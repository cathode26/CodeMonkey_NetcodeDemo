using SoundSignalList;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField]
    private StoveCounter stoveCounter;
    private AudioSource audioSource;
    private float warningSoundTimer;
    bool playWarningSound = false;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        stoveCounter.OnPlayerSetObject += StoveCounter_OnPlayerSetObject;
        stoveCounter.OnPlayerRemovedObject += StoveCounter_OnPlayerRemovedObject;
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }
    private void OnDisable()
    {
        stoveCounter.OnPlayerSetObject -= StoveCounter_OnPlayerSetObject;
        stoveCounter.OnPlayerRemovedObject -= StoveCounter_OnPlayerRemovedObject;
        stoveCounter.OnProgressChanged -= StoveCounter_OnProgressChanged;
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
        playWarningSound = false;
    }
    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        if (e.progressNormalized < 1)
        {
            float burnShowProgressAmout = 0.5f;
            playWarningSound = stoveCounter.IsCooked() && e.progressNormalized >= burnShowProgressAmout;
        }
        else
        {
            playWarningSound = false;
        }
    }
    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer <= 0)
            {
                float warningSoundTimerMax = 0.2f;
                warningSoundTimer = warningSoundTimerMax;
                Signals.Get<OnWarningSignal>().Dispatch(stoveCounter.transform.position);
            }
        }
    }
}
