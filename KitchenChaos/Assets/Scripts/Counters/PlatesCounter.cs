using System;
using Unity.Netcode;
using UnityEngine;

// The PlatesCounter class manages the logic related to spawning and interacting with plates.
public class PlatesCounter : BaseCounter
{
    // Events that are invoked when a plate is spawned and consumed.
    public event EventHandler OnPlateVisualSpawned;
    public event EventHandler OnPlateVisualConsumed;
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    // Time-related variables for controlling the rate of plate spawning.
    private float spawnPlateTimer;
    private float spawnTime = 4.0f;

    // Variables for controlling the number of plates that can be spawned.
    private int plateVisualsSpawnedAmount = 0;
    private int platesSpawnedAmountMax = 4;

    // Update method where the timer for spawning plates is managed.
    private void Update()
    {
        if (!IsServer)
            return;

        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnTime)
        {
            spawnPlateTimer = 0.0f;
            if (KitchenGameManager.Instance.IsGamePlaying() && plateVisualsSpawnedAmount < platesSpawnedAmountMax)
            {
                SpawnPlateVisualServerRpc();
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateVisualServerRpc()
    {
        SpawnPlateVisualClientRpc();
    }
    [ClientRpc]
    private void SpawnPlateVisualClientRpc()
    {
        plateVisualsSpawnedAmount++;
        OnPlateVisualSpawned.Invoke(this, EventArgs.Empty);
    }
    // Interact method where the player can pick up a plate if they don't already have an object.
    // Decrements the number of plates spawned and invokes the OnPlateConsumed event.
    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject() && !player.WaitingOnNetwork && plateVisualsSpawnedAmount > 0)
        {
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            SpawnPlateServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        if (plateVisualsSpawnedAmount > 0)
        {
            OnPlateVisualConsumed.Invoke(this, EventArgs.Empty);
            plateVisualsSpawnedAmount--;
        }
    }
}
