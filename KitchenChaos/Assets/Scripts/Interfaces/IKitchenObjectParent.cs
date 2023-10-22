using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
    Transform GetKitchenObjectFollowTransform();
    void SetKitchenObject(KitchenObject kitchenObject);
    KitchenObject GetKitchenObject();
    void ClearKitchenObject();
    bool HasKitchenObject();
    NetworkObject GetNetworkObject();
}
