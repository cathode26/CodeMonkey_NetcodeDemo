using System;
using Unity.Netcode.Components;

public class SynchronizedNetworkTransform : NetworkTransform
{
    public int NetworkTransformUpdates { get; private set; } = 0;
    public event Action OnNetworkTransformUpdatesComplete;

    protected override void OnNetworkTransformStateUpdated(ref NetworkTransform.NetworkTransformState oldState, ref NetworkTransform.NetworkTransformState newState)
    {
        base.OnNetworkTransformStateUpdated(ref oldState, ref newState);
        UnityEngine.Debug.Log($"networkTransformUpdates {NetworkTransformUpdates}");
        NetworkTransformUpdates++;
        OnNetworkTransformUpdatesComplete?.Invoke();
    }
    public void BeginSyncronizedNetworkTransform()
    {
        NetworkTransformUpdates = 0;
    }
}