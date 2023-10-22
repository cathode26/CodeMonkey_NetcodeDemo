using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] 
    private KitchenObjectSO KitchenObjectSO;
    private IKitchenObjectParent kitchenObjectsParent;
    private FollowTransform followTransform;

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return KitchenObjectSO; 
    }
    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
    public void SetKitchenObjectsParent(IKitchenObjectParent kitchenObjectsParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectsParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectsParentNetObjRef)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectsParentNetObjRef);
    }
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectsParentNetObjRef)
    {
        if (kitchenObjectsParentNetObjRef.TryGet(out NetworkObject kitchenObjectsParentNetObj))
        {
            IKitchenObjectParent kitchenObjectsParent = kitchenObjectsParentNetObj.GetComponent<IKitchenObjectParent>();
            this.kitchenObjectsParent?.ClearKitchenObject();
            this.kitchenObjectsParent = kitchenObjectsParent;

            if (kitchenObjectsParent.HasKitchenObject())
                Debug.Log("Counter already has a kitchen object");

            kitchenObjectsParent.SetKitchenObject(this);

            followTransform.SetTargetTransform(kitchenObjectsParent.GetKitchenObjectFollowTransform());
        }
    }
    public IKitchenObjectParent GetKitchenObjectsParent()
    {
        return kitchenObjectsParent;
    }
    public void DestroySelf()
    {
        kitchenObjectsParent.ClearKitchenObject();
        Destroy(gameObject);
    }
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        plateKitchenObject = GetComponent<PlateKitchenObject>();
        if (plateKitchenObject != null)
            return true;
        else
            return false;
    }
}