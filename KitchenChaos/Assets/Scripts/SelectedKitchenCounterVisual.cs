using UnityEngine;

//This script visualizes the currently selected kitchen counter by highlighting it.
//When the selected counter is changed, it checks whether the selected counter is the one it's attached to and activates or deactivates the visualization accordingly.
public class SelectedKitchenCounterVisual : MonoBehaviour
{
    [SerializeField]
    private KitchenCounter kitchenCounter;
    [SerializeField]
    private GameObject kitchenCounterFocusedVisual;

    private void OnEnable()
    {
        Player.Instance.OnSelectedKitchenCounterChanged += InstanceOnSelectedKitchenCounterChanged;
    }
    private void OnDisable()
    {
        Player.Instance.OnSelectedKitchenCounterChanged -= InstanceOnSelectedKitchenCounterChanged;
    }
    private void InstanceOnSelectedKitchenCounterChanged(object sender, Player.OnSelectedKitchenCounterChangedEventArgs e)
    {
        if (e.selectedKitchenCounter != null && e.selectedKitchenCounter == kitchenCounter)
            kitchenCounterFocusedVisual.SetActive(true);
        else
            kitchenCounterFocusedVisual.SetActive(false); 
    }
}
