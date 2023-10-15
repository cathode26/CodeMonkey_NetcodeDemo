using UnityEngine;

public class PlayerSounds : MonoBehaviour // Class responsible for managing the player's sounds, such as footsteps
{
    private PlayerVisualState playerVisualState;
    private float footstepTimer = 0;
    private float footstepTimeMax = 0.25f;

    private void Awake()
    {
        playerVisualState = GetComponent<PlayerVisualState>();
    }

    private void Update()
    {
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepTimeMax)
        {
            if (playerVisualState.IsWalking)
            {
                footstepTimer = 0.0f;
                SoundManager.Instance.PlayFootStepSound();
            }
        }
    }
}
