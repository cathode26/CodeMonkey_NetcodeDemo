using UnityEngine;
using UnityEngine.UI;
using static CuttingCounter;

public class CuttingProgressBarUI : MonoBehaviour
{
    [SerializeField] 
    private Image barImage;
    [SerializeField]
    private CuttingCounter cuttingCounter;
    [SerializeField]
    private GameObject progressBar;

    private void OnEnable()
    {
        if (cuttingCounter)
        {
            cuttingCounter.OnPlayerCutObject += ContainerCounter_OnPlayerCutObject;
            cuttingCounter.OnPlayerSetCuttableObject += CuttingCounter_OnPlayerSetCuttableObject;
            cuttingCounter.OnPlayerRemovedObject += CuttingCounter_OnPlayerRemovedObject;
        }
        else
        {
            Debug.LogError("ContainerCounterVisual: Failed to add Listener to OnPlayerGrabbedObject event");
        }
    }
    private void OnDisable()
    {
        if (cuttingCounter)
        {
            cuttingCounter.OnPlayerCutObject -= ContainerCounter_OnPlayerCutObject;
            cuttingCounter.OnPlayerSetCuttableObject -= CuttingCounter_OnPlayerSetCuttableObject;
        }
        else
        {
            Debug.LogError("ContainerCounterVisual: Failed to remove Listener to OnPlayerGrabbedObject event");
        }
    }
    private void ContainerCounter_OnPlayerCutObject(object sender, OnPlayerCutEventArgs e)
    {
        barImage.fillAmount = e.percentCut;
    }
    private void CuttingCounter_OnPlayerSetCuttableObject(object sender, System.EventArgs e)
    {
        EnableProgressBar();
    }
    private void CuttingCounter_OnPlayerRemovedObject(object sender, System.EventArgs e)
    {
        DisableProgressBar();
    }
    private void EnableProgressBar()
    {
        progressBar.SetActive(true);
        barImage.fillAmount = 0.0f;
    }
    private void DisableProgressBar()
    {
        progressBar.SetActive(false);
    }
}
