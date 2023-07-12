using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player player;
    private float footstepTimer = 0;
    private float footstepTimeMax = 0.25f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepTimeMax)
        {
            if (player.IsWalking)
            {
                footstepTimer = 0.0f;
                AudioManager.Instance.PlayFootStepSound();
            }
        }
    }
}
