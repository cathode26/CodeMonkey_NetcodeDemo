using CheatSignalList;
using GameSignalList;
using UnityEngine;

//This script visualizes the currently selected kitchen counter by highlighting it.
//When the selected counter is changed, it checks whether the selected counter is the one it's attached to and activates or deactivates the visualization accordingly.
public class SelectedBaseCounterVisual : MonoBehaviour
{
    [SerializeField]
    private BaseCounter baseCounter;
    [SerializeField]
    private GameObject[] baseCounterFocusedVisuals;

    private void OnEnable()
    {
        Signals.Get<OnSelectedBaseCounterChangedSignal>().AddListener(InstanceOnSelectedBaseCounterChanged);
    }
    private void OnDisable()
    {
        Signals.Get<OnSelectedBaseCounterChangedSignal>().RemoveListener(InstanceOnSelectedBaseCounterChanged);
    }
    private void InstanceOnSelectedBaseCounterChanged(BaseCounter selectedBaseCounter)
    {
        if (selectedBaseCounter != null && selectedBaseCounter == baseCounter)
        {
            foreach (GameObject baseCounterVisual in baseCounterFocusedVisuals)
                baseCounterVisual.SetActive(true);
        }
        else
        {
            foreach (GameObject baseCounterVisual in baseCounterFocusedVisuals)
                baseCounterVisual.SetActive(false);
        }
    }
}
