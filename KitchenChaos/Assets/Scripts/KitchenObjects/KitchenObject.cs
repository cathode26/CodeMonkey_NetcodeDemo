using Unity.Netcode;
using UnityEngine;

/**
 * Class: KitchenObject
 * 
 * Purpose:
 * This class defines the behavior of kitchen objects within a multiplayer game setting.
 * It includes mechanisms for visual state management, networked interactions, and
 * handling relationships with parent objects (players). This class ensures that
 * kitchen objects can be interacted with by players and manages the ownership and 
 * state transitions of these objects, especially in scenarios where multiple players
 * simultaneously interact with the same object.
 *
 * Usage:
 * KitchenObject is used as a base for all interactable objects in the kitchen environment.
 * It plays a critical role in client-side prediction and in resolving ownership conflicts
 * in a networked multiplayer scenario.
 */

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] 
    private KitchenObjectSO KitchenObjectSO;
    private IKitchenObjectParent kitchenObjectsParent;
    private FollowTransform followTransform;
    public NetworkVariable<int> objId = new NetworkVariable<int> ();
    public NetworkVariable<ulong> clientId = new NetworkVariable<ulong> ();
    private KitchenObjectVisualStateManager kitchenObjectVisualStateManager = null;
    public bool IsVisible { get => kitchenObjectVisualStateManager.IsVisible; }

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
        //If a player is picking up a kitchen object from a counter
        //it is right here that we clear the object locally
        IKitchenObjectParent prevKitchenObjectsParent = this.kitchenObjectsParent;
        this.kitchenObjectsParent?.ClearKitchenObject();
        this.kitchenObjectsParent = kitchenObjectsParent;

        if (kitchenObjectsParent.HasKitchenObject())
            Debug.Log("Counter already has a kitchen object");

        kitchenObjectsParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectsParent.GetKitchenObjectFollowTransform());

        double interactionTime = NetworkManager.Singleton.ServerTime.Time;
        if (!IsServer)
            interactionTime = interactionTime + ((NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(0) / 2) / 1000.0);

        Debug.Log("client id " + NetworkManager.LocalClientId + " interactionTime " + interactionTime);

        KitchenObjectParentManager.Instance.AddStateChange(NetworkManager.LocalClientId, interactionTime, this, kitchenObjectsParent, prevKitchenObjectsParent);

        if (prevKitchenObjectsParent != null)
            SetKitchenObjectParentServerRpc(interactionTime, kitchenObjectsParent.GetNetworkObject(), prevKitchenObjectsParent.GetNetworkObject());
        else
            SetKitchenObjectParentServerRpc(interactionTime, kitchenObjectsParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(double interactionTime, NetworkObjectReference kitchenObjectsParentNetObjRef, NetworkObjectReference prevKitchenObjectsParentNetObjRef = default, ServerRpcParams serverRpcParams = default)
    {
        SetKitchenObjectParentClientRpc(serverRpcParams.Receive.SenderClientId, interactionTime, kitchenObjectsParentNetObjRef, prevKitchenObjectsParentNetObjRef);
    }
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(ulong clientId, double interactionTime, NetworkObjectReference kitchenObjectsParentNetObjRef, NetworkObjectReference prevKitchenObjectsParentNetObjRef = default)
    {
        if (kitchenObjectsParentNetObjRef.TryGet(out NetworkObject kitchenObjectsParentNetObj))
        {
            IKitchenObjectParent parentAdded = GetKitchenObjectParent(kitchenObjectsParentNetObjRef);
            IKitchenObjectParent parentRemoved = GetKitchenObjectParent(prevKitchenObjectsParentNetObjRef);
            if (KitchenObjectParentManager.Instance.MaybeAddStateChange(clientId, interactionTime, this, parentAdded, parentRemoved))
            {
                IKitchenObjectParent kitchenObjParent = KitchenObjectParentManager.Instance.GetKitchenObjectParent(this);
                this.kitchenObjectsParent?.ClearKitchenObject();
                this.kitchenObjectsParent = kitchenObjParent;

                if (kitchenObjectsParent.HasKitchenObject())
                    Debug.Log("Counter already has a kitchen object");

                kitchenObjectsParent.SetKitchenObject(this);

                followTransform.SetTargetTransform(kitchenObjectsParent.GetKitchenObjectFollowTransform());
            }
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
    private IKitchenObjectParent GetKitchenObjectParent(NetworkObjectReference parentRef)
    {
        if (parentRef.TryGet(out NetworkObject networkObject))
            return networkObject.GetComponent<IKitchenObjectParent>();
        else
            return null;
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