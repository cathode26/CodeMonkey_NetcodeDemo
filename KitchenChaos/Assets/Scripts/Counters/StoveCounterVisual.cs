using UnityEngine;
using static StoveCounter;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField]
    private StoveCounter stoveCounter;
    [SerializeField]
    private GameObject stoveOnVisual;
    [SerializeField]
    private GameObject sizzlingParticles;

    private void OnEnable()
    {
        if (stoveCounter)
        {
            stoveCounter.OnPlayerSetObject += StoveCounter_OnPlayerSetObject;
            stoveCounter.OnPlayerRemovedObject += StoveCounter_OnPlayerRemovedObject;
        }
        else
        {
            Debug.LogError("StoveCounterVisual: Failed to add Listener to OnPlayerSetObject event");
        }
    }
    private void OnDisable()
    {
        if (stoveCounter)
        {
            stoveCounter.OnPlayerSetObject -= StoveCounter_OnPlayerSetObject;
            stoveCounter.OnPlayerRemovedObject -= StoveCounter_OnPlayerRemovedObject;
        }
        else
        {
            Debug.LogError("StoveCounterVisual: Failed to remove Listener to OnPlayerSetObject event");
        }
    }
    private void StoveCounter_OnPlayerSetObject(object sender, OnPlayerCookingEventArgs e)
    {
        if (e.state != CookingRecipeSO.State.Burned)
        {
            stoveOnVisual.SetActive(true);
            sizzlingParticles.SetActive(true);
        }
        else
        {
            stoveOnVisual.SetActive(false);
            sizzlingParticles.SetActive(false);
        }
    }
    private void StoveCounter_OnPlayerRemovedObject(object sender, System.EventArgs e)
    {
        stoveOnVisual.SetActive(false);
        sizzlingParticles.SetActive(false);
    }
}
