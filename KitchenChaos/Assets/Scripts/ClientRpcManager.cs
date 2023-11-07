using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages a list of clients excluding the sender for RPC calls.
/// ClientRpcManager is only used on the server because ClientRpc calls are only made on the server
/// </summary>
public class ClientRpcManager : NetworkBehaviour
{
    private Dictionary<ulong, ClientRpcParams> _allClientsExcludingSenderParams = new Dictionary<ulong, ClientRpcParams>();
    private bool _shouldUpdateRpcParams = false;
    private const float UPDATE_DELAY = 1.0f;
    private float _elapsedTime = 0.0f;
    public static ClientRpcManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of ClientExclusionManager detected! Destroying...");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        Signals.Get<GameSignalList.OnPlayerSpawnedSignal>().AddListener(OnPlayerSpawned);
        Signals.Get<GameSignalList.OnPlayerDespawnedSignal>().AddListener(OnPlayerDespawned);
    }
    public override void OnDestroy()
    {
        Signals.Get<GameSignalList.OnPlayerSpawnedSignal>().RemoveListener(OnPlayerSpawned);
        Signals.Get<GameSignalList.OnPlayerDespawnedSignal>().RemoveListener(OnPlayerDespawned);
        Instance = null;
        base.OnDestroy();
    }
    private void OnPlayerSpawned(Player player)
    {
        _shouldUpdateRpcParams = true;
    }
    private void OnPlayerDespawned(Player player)
    {
        _shouldUpdateRpcParams = true;
    }
    private void Update()
    {
        if (_shouldUpdateRpcParams)
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > UPDATE_DELAY)
            {
                UpdateAllClientsExcludingSenderServerRpc();
                _shouldUpdateRpcParams = false;
                _elapsedTime = 0.0f;
            }
        }
    }
    // <summary>
    /// Gets the client parameters excluding the specified sender.
    /// </summary>
    /// <param name="senderId">The sender's client ID.</param>
    /// <returns>ClientRpcParams excluding the sender.</returns>
    public ClientRpcParams GetClientsExcludeSender(ulong senderId)
    {
        if (!_allClientsExcludingSenderParams.TryGetValue(senderId, out ClientRpcParams result))
        {
            result = UpdateExcludedSenderList(senderId);
            _allClientsExcludingSenderParams[senderId] = result;
        }

        return result;
    }
    public HashSet<ulong> GetAllClients()
    {
        HashSet<ulong> targetClientIds = new HashSet<ulong>();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            targetClientIds.Add(client.ClientId);
        return targetClientIds;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateAllClientsExcludingSenderServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong originalSenderId = serverRpcParams.Receive.SenderClientId;
        _allClientsExcludingSenderParams[originalSenderId] = UpdateExcludedSenderList(originalSenderId);
    }
    private ClientRpcParams UpdateExcludedSenderList(ulong originalSenderId)
    {
        List<ulong> targetClientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsList.Count);
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != originalSenderId)
            {
                targetClientIds.Add(client.ClientId);
            }
        }

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = targetClientIds }
        };
    }
    public ClientRpcParams GetClientRpcParams(ulong senderId)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { senderId } }
        };
    }
}
