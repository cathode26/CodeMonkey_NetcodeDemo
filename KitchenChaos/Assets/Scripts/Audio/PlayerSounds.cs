using UnityEngine;

public class PlayerSounds : MonoBehaviour // Class responsible for managing the player's sounds, such as footsteps
{
    private ClientMovement clientMovement;
    private float footstepTimer = 0;
    private float footstepTimeMax = 0.25f;

    private void Awake()
    {
        clientMovement = GetComponent<ClientMovement>();
    }

    private void Update()
    {
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepTimeMax)
        {
            if (clientMovement.IsWalking)
            {
                footstepTimer = 0.0f;
                SoundManager.Instance.PlayFootStepSound();
            }
        }
    }
}
