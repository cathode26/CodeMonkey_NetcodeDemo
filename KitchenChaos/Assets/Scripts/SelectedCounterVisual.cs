using UnityEngine;

//This script visualizes the currently selected kitchen counter by highlighting it.
//When the selected counter is changed, it checks whether the selected counter is the one it's attached to and activates or deactivates the visualization accordingly.
public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField]
    private ClearCounter clearCounter;
    [SerializeField]
    private GameObject counterVisual;

    private void OnEnable()
    {
        Player.Instance.OnSelectedCounterChanged += InstanceOnSelectedCounterChanged;
    }
    private void OnDisable()
    {
        Player.Instance.OnSelectedCounterChanged -= InstanceOnSelectedCounterChanged;
    }
    private void InstanceOnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == clearCounter)
            counterVisual.SetActive(true);
        else
            counterVisual.SetActive(false); 
    }
}
