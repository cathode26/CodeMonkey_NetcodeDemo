using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
    Transform GetKitchenObjectFollowTransform();
    void SetKitchenObject(KitchenObject kitchenObject);
    void WaitForNetwork();
    void NetworkComplete();
    bool WaitingOnNetwork { get; }

    KitchenObject GetKitchenObject();
    void ClearKitchenObject();
    bool HasKitchenObject();
    NetworkObject GetNetworkObject();
}
