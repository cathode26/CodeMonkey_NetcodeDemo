using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    [SerializeField]
    private ContainerCounter containerCounter;

    private Animator animator;
    string OPEN_CLOSE = "OpenClose";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        if (containerCounter)
            containerCounter.OnPlayerGrabbedObject += ContainerCounter_OnPlayerGrabbedObject;
        else
            Debug.LogError("ContainerCounterVisual: Failed to add Listener to OnPlayerGrabbedObject event");
    }
    private void OnDisable()
    {
        if (containerCounter)
            containerCounter.OnPlayerGrabbedObject -= ContainerCounter_OnPlayerGrabbedObject;
        else
            Debug.LogError("ContainerCounterVisual: Failed to remove Listener to OnPlayerGrabbedObject event");
    }
    private void ContainerCounter_OnPlayerGrabbedObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
