using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking"; 
    private Animator animator;
    private ClientMovement clientMovement;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        clientMovement = GetComponent<ClientMovement>();
    }
    private void OnEnable()
    {
        clientMovement.OnWalkingStateChanged += UpdateWalkingState;
    }
    private void OnDisable()
    {
        clientMovement.OnWalkingStateChanged -= UpdateWalkingState;
    }
    private void UpdateWalkingState(bool state)
    {
        animator.SetBool(IS_WALKING, state);
    }
}
