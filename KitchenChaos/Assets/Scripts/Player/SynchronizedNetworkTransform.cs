using System;
using Unity.Netcode.Components;

public class SynchronizedNetworkTransform : NetworkTransform
{
    public event Action OnNetworkTransformUpdatesComplete;

    protected override void OnNetworkTransformStateUpdated(ref NetworkTransform.NetworkTransformState oldState, ref NetworkTransform.NetworkTransformState newState)
    {
        base.OnNetworkTransformStateUpdated(ref oldState, ref newState);
        //UnityEngine.Debug.Log($"networkTransformUpdates {NetworkTransformUpdates}");
        OnNetworkTransformUpdatesComplete?.Invoke();
    }
}