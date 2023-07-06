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
        Player.Instance.OnSelectedBaseCounterChanged += InstanceOnSelectedBaseCounterChanged;
    }
    private void OnDisable()
    {
        Player.Instance.OnSelectedBaseCounterChanged -= InstanceOnSelectedBaseCounterChanged;
    }
    private void InstanceOnSelectedBaseCounterChanged(object sender, Player.OnSelectedBaseCounterChangedEventArgs e)
    {
        if (e.selectedBaseCounter != null && e.selectedBaseCounter == baseCounter)
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
