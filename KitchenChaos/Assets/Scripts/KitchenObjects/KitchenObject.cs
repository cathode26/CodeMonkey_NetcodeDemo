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
    private Renderer[] objectRenderers;

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return KitchenObjectSO; 
    }
    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
        objectRenderers = GetComponentsInChildren<Renderer>();
    }
    public void OnEnable()
    {
        isVisible.OnValueChanged += OnVisibilityChanged;
    }
    public void OnDisable()
    {
        isVisible.OnValueChanged -= OnVisibilityChanged;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnVisibilityChanged(!isVisible.Value, isVisible.Value);
    }
    virtual protected void OnVisibilityChanged(bool oldVal, bool newVal)
    {
        foreach (Renderer renderer in objectRenderers)
            renderer.enabled = newVal;
    }
    virtual public void SetVisibilityLocal(bool visible)
    {
        foreach (Renderer renderer in objectRenderers)
            renderer.enabled = visible;
    }
    public void SetVisibility(bool visible)
    {
        isVisible.Value = visible;
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
    [ClientRpc]
    public void ClearKitchenObjectParentClientRpc()
    {
        kitchenObjectsParent?.ClearKitchenObject();
        kitchenObjectsParent = null;
        followTransform.ResetTarget();
    }
    public IKitchenObjectParent GetKitchenObjectsParent()
    {
        return kitchenObjectsParent;
    }
    public void DestroySelf()
    {
        SetVisibilityLocal(false);
        kitchenObjectsParent.ClearKitchenObject();
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
}