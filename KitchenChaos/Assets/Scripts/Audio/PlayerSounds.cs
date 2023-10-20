using GameSignalList;
using SoundSignalList;
using UnityEngine;

public class PlayerSounds : MonoBehaviour // Class responsible for managing the player's sounds, such as footsteps
{
    private PlayerVisualState playerVisualState;
    private float footstepTimer = 0;
    private float footstepTimeMax = 0.25f;
    Player player;

    private void Awake()
    {
        playerVisualState = GetComponent<PlayerVisualState>();
        Signals.Get<OnPlayerSpawnedSignal>().AddListener(OnPlayerSpawned);
    }
    private void OnDestroy()
    {
        Signals.Get<OnPlayerSpawnedSignal>().RemoveListener(OnPlayerSpawned);
        if (player.IsOwner)
        {
            //Attach the AudioListener to the owner player
            Signals.Get<ReturnAudioListenerSignal>().Dispatch();
        }
    }
    private void Update()
    {
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepTimeMax)
        {
            if (playerVisualState.IsWalking)
            {
                footstepTimer = 0.0f;
                Signals.Get<OnFootStepsSignal>().Dispatch(transform.position);
            }
        }
    }
    private void OnPlayerSpawned(Player player)
    {
        this.player = player;
        if (player.IsOwner)
        {
            //Attach the AudioListener to the owner player
            Signals.Get<SetAudioListenerSignal>().Dispatch(transform);
        }
    }
}
