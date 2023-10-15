using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking"; 
    private Animator animator;
    private PlayerVisualState playerVisualState;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerVisualState = GetComponent<PlayerVisualState>();
    }
    private void OnEnable()
    {
        playerVisualState.OnWalkingStateChanged += UpdateWalkingState;
    }
    private void OnDisable()
    {
        playerVisualState.OnWalkingStateChanged -= UpdateWalkingState;
    }
    private void UpdateWalkingState(bool state)
    {
        animator.SetBool(IS_WALKING, state);
    }
}
