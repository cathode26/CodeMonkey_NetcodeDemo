using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    [SerializeField]
    private CuttingCounter cuttingCounter;

    private Animator animator;
    string CUT = "Cut";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        if (cuttingCounter)
            cuttingCounter.OnPlayerCutObject += ContainerCounter_OnPlayerCutObject;
        else
            Debug.LogError("ContainerCounterVisual: Failed to add Listener to OnPlayerGrabbedObject event");
    }
    private void OnDisable()
    {
        if (cuttingCounter)
            cuttingCounter.OnPlayerCutObject -= ContainerCounter_OnPlayerCutObject;
        else
            Debug.LogError("ContainerCounterVisual: Failed to remove Listener to OnPlayerGrabbedObject event");
    }
    private void ContainerCounter_OnPlayerCutObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
