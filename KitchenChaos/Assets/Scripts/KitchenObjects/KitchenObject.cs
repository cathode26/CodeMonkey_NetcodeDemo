using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] 
    private KitchenObjectSO KitchenObjectSO;
    private IKitchenObjectParent kitchenObjectsParent;
    private FollowTransform followTransform;
    public NetworkVariable<int> objId = new NetworkVariable<int> ();
    public NetworkVariable<ulong> clientId = new NetworkVariable<ulong> ();
    private NetworkVariable<bool> isVisible = new NetworkVariable<bool>(false);
    private KitchenObjectVisualStateManager kitchenObjectVisualStateManager = null;
    public bool IsVisible { get => isVisible.Value; }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return KitchenObjectSO; 
    }
    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
        kitchenObjectVisualStateManager = GetComponent<KitchenObjectVisualStateManager>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetVisibilityLocal(false);
    }
    virtual public void SetVisibilityLocal(bool visible)
    {
        kitchenObjectVisualStateManager.SetVisibility(visible);
    }
    virtual public void SetVisibility(bool visible)
    {
        kitchenObjectVisualStateManager.SetVisibility(visible);
        SetVisibilityServerRpc(visible);
    }
    [ServerRpc(RequireOwnership = false)]
    virtual public void SetVisibilityServerRpc(bool visible, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId);
        SetVisibilityClientRpc(visible, clientRpcParams);
    }
    [ClientRpc]
    virtual public void SetVisibilityClientRpc(bool visible, ClientRpcParams clientRpcParams)
    {
        SetVisibilityLocal(visible);
    }
    public void SetPredictedVisual(bool showPredictedVisuals)
    {
        if(showPredictedVisuals)
            kitchenObjectVisualStateManager.ShowPredictedVisuals();
        else
            kitchenObjectVisualStateManager.ShowDefaultVisuals();
    }
    public void SetKitchenObjectsParent(IKitchenObjectParent kitchenObjectsParent)
    {
        this.kitchenObjectsParent?.ClearKitchenObject();
        this.kitchenObjectsParent = kitchenObjectsParent;

        if (kitchenObjectsParent.HasKitchenObject())
            Debug.Log("Counter already has a kitchen object");

        kitchenObjectsParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectsParent.GetKitchenObjectFollowTransform());

        SetKitchenObjectParentServerRpc(kitchenObjectsParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectsParentNetObjRef, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId);
        SetKitchenObjectParentClientRpc(kitchenObjectsParentNetObjRef, clientRpcParams);
    }
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectsParentNetObjRef, ClientRpcParams clientRpcParams)
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
    public void ClearKitchenObjectParent()
    {
        kitchenObjectsParent?.ClearKitchenObject();
        kitchenObjectsParent = null;
        followTransform.ResetTarget();
    }
    public IKitchenObjectParent GetKitchenObjectsParent()
    {
        return kitchenObjectsParent;
    }
    public void ReturnKitchenObject()
    {
        SetPredictedVisual(false);
        SetVisibilityLocal(false);
        kitchenObjectsParent?.ClearKitchenObject();
        kitchenObjectsParent = null;
        followTransform.ResetTarget();
        KitchenGameMultiplayer.Instance.ReturnKitchenObject(this);
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
    public bool ShowingPredictedVisuals()
    {
        return kitchenObjectVisualStateManager.ShowingPredictedVisuals();
    }
}