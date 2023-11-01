using GameSignalList;
using Unity.Netcode;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    // This script should be attached to an empty root node.
    // The actual player model/mesh should be a child of the root node.
    // This allows you to scale the model independently, while only moving and rotating the root.
    //Private, public, protected are called the accessors

    [SerializeField]
    LayerMask countersLayerMask;
    [SerializeField]
    private Transform kitchenObjectHoldPoint;
    [SerializeField]
    private Transform playerVisual;

    private KitchenObject kitchenObject;
    private Vector3 lastInteractionDirection = Vector3.zero; 
    private float interactDistance = 2.0f;
    private BaseCounter selectedCounter;
    private ServerMovement serverMovement;
    private NetworkObject networkObject;
    private bool waitingOnNetwork = false;
    public bool IsOwner { get { return networkObject.IsOwner; } }   //public bool IsOwner { get => networkObject.IsOwner; }
    public ulong OwnerClientId { get { return networkObject.OwnerClientId; } }
    public bool WaitingOnNetwork { get => waitingOnNetwork; }

    public void OnNetworkSpawn(ServerMovement serverMovement)
    {
        this.serverMovement = serverMovement;
        networkObject = transform.GetComponent<NetworkObject>();
        if (!networkObject.IsOwner)
            enabled = false;
    }

    private void OnEnable()
    {
        GameInput.Instance.OnInteractAction += GameInputOnInteractAction;
        GameInput.Instance.OnInteractAlternativeAction += GameInputOnInteractAlternativeAction;
    }
    private void OnDisable()
    {
        GameInput.Instance.OnInteractAction -= GameInputOnInteractAction;
        GameInput.Instance.OnInteractAlternativeAction -= GameInputOnInteractAlternativeAction;
    }
    /*  The GameInputOnInteractAction event is called from the GameInput when the user presses e
        The selectedCounter is set when the user is in front of a counter
        If both are true, then we can call the interact function
    */
    private void GameInputOnInteractAction(object sender, System.EventArgs e)
    {
        //Check for null operator ?
        selectedCounter?.Interact(this);
    }
    private void GameInputOnInteractAlternativeAction(object sender, System.EventArgs e)
    {
        //Check for null operator ?
        selectedCounter?.InteractAlternative(this);
    }
    private void Update()
    {
        //Handles interaction with objects in front of the player.
        HandleInteractions();
    }
    private void HandleInteractions()
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            SetSelectedCounter(null);
            return;
        }

        //The input vector is either the direction of the movement or the direction of the last interaction if the player is not moving.
        //This is used to determine what object the player is facing and possibly interacting with.
        (bool moved, Vector2 dir) movementData = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir;
        if (movementData.moved)
            moveDir = new Vector3(movementData.dir.x, 0.0f, movementData.dir.y);
        else
            moveDir = lastInteractionDirection;

        //interact with the counter, we will see if we are touching it with a raycast
        //this only returns the first object it hits To get everything it hits, then use RaycastAll
        //The next option is to put objects in a particular layerMask, and then you can use the layerMask to filter the objects
        if (Physics.Raycast(playerVisual.position, moveDir, out RaycastHit info, interactDistance))
        {
            lastInteractionDirection = moveDir;
            if (info.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != selectedCounter)
                    SetSelectedCounter(baseCounter);
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }
    private void SetSelectedCounter(BaseCounter baseCounter)
    {
        //Changes the currently selected kitchen counter and triggers the OnSelectedKitchenCounterChanged event.
        selectedCounter = baseCounter;
        Signals.Get<OnSelectedBaseCounterChangedSignal>().Dispatch(baseCounter);
    }
    public void ClearKitchenObject()
    {
        if (kitchenObject != null)
            Signals.Get<ServerSoundSignalList.OnObjectDropSignal>().Dispatch(kitchenObject.transform.position);
        kitchenObject = null;
    }
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        if (kitchenObject != null)
            Signals.Get<ServerSoundSignalList.OnObjectPickupSignal>().Dispatch(kitchenObject.transform.position);
        this.kitchenObject = kitchenObject;
    }
    public NetworkObject GetNetworkObject()
    {
        return networkObject;
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