using ServerSignalList;
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
    }
    public static LatencyManager Instance { get; private set; }
    private Queue<ulong> queuedPlayers = new Queue<ulong>();
    private bool calculatingRoundTripTime = true;
    private int maxSize = 10;
    public float elapsedTime = 0.0f;
    private float UPDATE_DELAY = 1.0f;
    private Dictionary<ulong, LatencyData> clientIdToLatencyData = new Dictionary<ulong, LatencyData>();
    private LatencyData localLatencyData = new LatencyData();
    private bool initializing = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of LatencyAverage detected! Destroying...");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        Signals.Get<OnPlayerSpawnedSignal>().AddListener(OnPlayerSpawned);
        Signals.Get<OnPlayerDespawnedSignal>().AddListener(OnPlayerDespawned);
    }
    public override void OnDestroy()
    {
        Signals.Get<OnPlayerSpawnedSignal>().RemoveListener(OnPlayerSpawned);
        Signals.Get<OnPlayerDespawnedSignal>().RemoveListener(OnPlayerDespawned);
        Instance = null;
        base.OnDestroy();
    }
    private void OnPlayerSpawned(ulong clientId)
    {
        queuedPlayers.Enqueue(clientId);
    }
    private void OnPlayerDespawned(ulong clientId)
    {
        OnPlayerDespawnedServerRpc(clientId);
    }
    private void InitializePlayer()
    {
        ulong clientId = queuedPlayers.Dequeue();
        localLatencyData.StartTime = Time.time;
        CalculateRoundTripTimeServerRpc(clientId);
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
        UPDATE_DELAY = Mathf.Max(1.0f, GetAverageRoundTripTime(localLatencyData));
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

        AddRoundTripValue(latencyData, Time.time - latencyData.StartTime);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnPlayerDespawnedServerRpc(ulong clientId)
    {
        if(clientIdToLatencyData.ContainsKey(clientId))
            clientIdToLatencyData.Remove(clientId);
    }
    private void Update()
    {
        if (queuedPlayers.Count > 0)
            InitializePlayer();

        if(!IsServer && calculatingRoundTripTime == false)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > UPDATE_DELAY)
            {
                localLatencyData.StartTime = Time.time;
                calculatingRoundTripTime = true;
                elapsedTime = 0.0f;
                CalculateRoundTripTimeServerRpc(NetworkManager.LocalClientId);
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
    public float GetLongestRoundTripTime()
    {
        float longestRoundTrip = -1;
        foreach (KeyValuePair<ulong, LatencyData> clientIdToLatencyDataPair in clientIdToLatencyData)
        {
            float rtt = GetAverageRoundTripTime(clientIdToLatencyDataPair.Value);

            if (longestRoundTrip < rtt)
                longestRoundTrip = rtt;
        }
        return longestRoundTrip;
    }
}
