using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking"; 
    private Animator animator;
    private Player player;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        player = GetComponent<Player>();
    }
    private void OnEnable()
    {
        player.OnWalkingStateChanged += UpdateWalkingState;
    }
    private void OnDisable()
    {
        player.OnWalkingStateChanged -= UpdateWalkingState;
    }
    private void UpdateWalkingState(bool state)
    {
        animator.SetBool(IS_WALKING, state);
    }
}
