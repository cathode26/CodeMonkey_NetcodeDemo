using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LatencyManager : NetworkBehaviour
{
    public class LatencyData
    {
        public LinkedList<float> RoundTripTimes = new LinkedList<float>();
        public float Sum = 0;
        public float StartTime = 0;
        public float RoundTripTime = 0.0f;
    }
    public static LatencyManager Instance { get; private set; }
    private bool calculatingRoundTripTime = false;
    private int maxSize = 10;
    public float elapsedTime = 0.0f;
    private float UPDATE_DELAY = 1.0f;
    private Dictionary<ulong, LatencyData> clientIdToLatencyData = new Dictionary<ulong, LatencyData>();
    private LatencyData localLatencyData = new LatencyData();
    private bool initializing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of LatencyAverage detected! Destroying...");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    public override void OnDestroy()
    {
        Instance = null;
        base.OnDestroy();
    }
    public override void OnNetworkSpawn()
    {
        localLatencyData.StartTime = Time.time;
        calculatingRoundTripTime = true;
        initializing = true;
        CalculateRoundTripTimeServerRpc(OwnerClientId);
    }
    public override void OnNetworkDespawn()
    {
        OnPlayerDespawnedServerRpc(OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CalculateRoundTripTimeServerRpc(ulong clientId)
    {
        if (!clientIdToLatencyData.TryGetValue(clientId, out LatencyData latencyData))
        {
            latencyData = new LatencyData();
            clientIdToLatencyData[clientId] = latencyData;
        }
        latencyData.StartTime = Time.time;
        CalculateRoundTripTimeClientRpc(clientId, ClientRpcManager.Instance.GetClientRpcParams(clientId));
    }
    [ClientRpc]
    private void CalculateRoundTripTimeClientRpc(ulong clientId, ClientRpcParams clientRpcParams)
    {
        AddRoundTripValue(localLatencyData, Time.time - localLatencyData.StartTime);
        UPDATE_DELAY = GetAverageRoundTripTime(localLatencyData);
        CalculatedRoundTripTimeServerRpc(clientId);
        calculatingRoundTripTime = false;
        if (initializing)
        {
            Signals.Get<ServerSignalList.OnLatencyInitializedSignal>().Dispatch();
            initializing = false;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CalculatedRoundTripTimeServerRpc(ulong clientId)
    {
        if (!clientIdToLatencyData.TryGetValue(clientId, out LatencyData latencyData))
            return;

        AddRoundTripValue(latencyData, latencyData.StartTime - Time.time);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnPlayerDespawnedServerRpc(ulong clientId)
    {
        if(clientIdToLatencyData.ContainsKey(clientId))
            clientIdToLatencyData.Remove(clientId);
    }
    private void Update()
    {
        if (calculatingRoundTripTime == false)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > UPDATE_DELAY)
            {
                localLatencyData.StartTime = Time.time;
                calculatingRoundTripTime = true;
                elapsedTime = 0.0f;
                CalculateRoundTripTimeServerRpc(OwnerClientId);
            }
        }
    }
    private void AddRoundTripValue(LatencyData latencyData, float roundTripTime)
    {
        // Add the new value to the end (back) of the LinkedList
        roundTripTime = Mathf.Max(roundTripTime, 0.001f);
        latencyData.RoundTripTimes.AddLast(roundTripTime);
        latencyData.Sum += roundTripTime;

        // If the LinkedList size exceeds the maximum size, remove the first (front) value
        if (latencyData.RoundTripTimes.Count > maxSize)
        {
            latencyData.Sum -= latencyData.RoundTripTimes.First.Value;
            latencyData.RoundTripTimes.RemoveFirst();
        }
    }
    private float GetAverageRoundTripTime(LatencyData latencyData)
    {
        if (latencyData.RoundTripTimes.Count == 0) 
            return 0; // Avoid division by zero
        return latencyData.Sum / latencyData.RoundTripTimes.Count;
    }
    public float GetAverageRoundTripTime(ulong clientId)
    {
        if (!clientIdToLatencyData.TryGetValue(clientId, out LatencyData latencyData))
            return 1;
        else
            return GetAverageRoundTripTime(latencyData);
    }
    public float GetAverageRoundTripTime()
    {
        if (localLatencyData.RoundTripTimes.Count == 0)
            return 0; // Avoid division by zero
        return localLatencyData.Sum / localLatencyData.RoundTripTimes.Count;
    }
    public bool CalculatedRoundTripTime()
    {
        if (localLatencyData.RoundTripTimes.Count > 0)
            return true;
        else 
            return false;
    }
}
