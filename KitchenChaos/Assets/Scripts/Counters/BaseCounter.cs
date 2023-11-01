using Unity.Netcode;
using UnityEngine;

public abstract class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    [SerializeField]
    private Transform counterTopPoint;
    private KitchenObject kitchenObject;
    private bool waitingOnNetwork = false;
    public abstract void Interact(Player player);
    public virtual void InteractAlternative(Player player) { }
    public bool WaitingOnNetwork { get => waitingOnNetwork; }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
    public void WaitForNetwork()
    {
        waitingOnNetwork = true;
    }
    public void NetworkComplete()
    {
        waitingOnNetwork = false;
    }
}
