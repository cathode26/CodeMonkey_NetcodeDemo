using Unity.Netcode;
using UnityEngine;
public class ClientMovement : MonoBehaviour
{
    private IMovement _movementLogic;
    private Transform _originalParent;
    private ServerMovement _serverMovement;
    private NetworkObject _networkObject;
    private SynchronizedNetworkTransform _syncronizedNetworkTransform;

    private bool _isWalking = false;
    public bool IsWalking { private set { _isWalking = value; } get { return _isWalking; } }
    public delegate void WalkingState(bool isWalking);
    public event WalkingState OnWalkingStateChanged;

    private bool wasMovingLastFrame = false;
    private float firstCommandSentTime = -1f;
    private float firstUpdateReceivedTime = -1f;
    private float estimatedLatency = -1f;
    private float reattachTime = -1f; // Time when you should reattach to the original parent
    private bool lastMoveMade = false;
    public void OnNetworkSpawn(ServerMovement serverMovement, PlayerProperties playerProperties)
    {
        _networkObject = transform.parent.GetComponent<NetworkObject>();
        _serverMovement = serverMovement;
        _movementLogic = new BaseMovement(playerProperties, transform);

        if (_networkObject.IsOwner)
        {
            // Add a callback to the OnValueChanged event
            _syncronizedNetworkTransform = transform.parent.GetComponent<SynchronizedNetworkTransform>();
            _syncronizedNetworkTransform.OnNetworkTransformUpdatesComplete += OnFinalPositionChanged;
        }
        else
        {
            enabled = false;
        }
    }
    public void OnNetworkDespawn()
    {
        if (_networkObject.IsOwner)
        {
            // Remove a callback to the OnValueChanged event
            _syncronizedNetworkTransform.OnNetworkTransformUpdatesComplete -= OnFinalPositionChanged;
        }
    }
    private void OnFinalPositionChanged()
    {
        if (firstUpdateReceivedTime < 0f)
        {
            firstUpdateReceivedTime = Time.time; // Store the time when the first update is received
            estimatedLatency = firstUpdateReceivedTime - firstCommandSentTime; // Calculate the estimated latency
            Debug.Log("OnFinalPositionChanged reattachTime " + reattachTime);
        }
        reattachTime = Time.time + estimatedLatency;
    }
    private void Update()
    {
        HandleMovement();

        // Check if we're awaiting a position update and if the position has changed
        if (lastMoveMade)
        {
            if (Vector3.Distance(transform.position, _originalParent.position) < 0.01 && reattachTime > 0f)
            {
                ReAttachParent();
            }
            else if (Time.time >= reattachTime && reattachTime > 0f)
            {
                _serverMovement.HandleInterpolationServerRpc(transform.position);
                reattachTime = Time.time + estimatedLatency;
            }
        }
    }
    public void ReAttachParent()
    {
        if (_originalParent != null)
        {
            transform.parent = _originalParent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _originalParent = null;
            Debug.Log("Reattach to the original parent ");
        }
        // Reset the flag
        reattachTime = -1f; // Reset the reattach time
        lastMoveMade = false;
        reattachTime = 0.0f;
    }
    private void HandleMovement()
    {
        //Controls the movement and rotation of the player according to user input.
        //This is the raw input without consideration of where the player can move to
        MovementResult movementResult = _movementLogic.HandleMovement(GameInput.Instance.GetMovementVectorNormalized(), Time.deltaTime);

        if (movementResult.ReceivedMovementInput)
        {
            // Detach from parent and store the reference
            if (transform.parent)
            {
                _originalParent = transform.parent;
                transform.parent = null;
            }
           
            if (movementResult.CanMove)
            {
                if (IsWalking == false)
                    OnWalkingStateChanged?.Invoke(true);

                IsWalking = true;
            }
            else
            {
                if (IsWalking == true)
                    OnWalkingStateChanged?.Invoke(false);
                IsWalking = false;
            }

            if (!wasMovingLastFrame)
            {
                _syncronizedNetworkTransform.BeginSyncronizedNetworkTransform();
                firstCommandSentTime = Time.time; // Store the time when the first command is sent
                firstUpdateReceivedTime = -1;
            }
            _serverMovement.MoveAndRotatePlayerServerRpc(movementResult.Direction, movementResult.ClientDeltaTime);
        }
        else
        {
            if (IsWalking)
                OnWalkingStateChanged?.Invoke(false);
            IsWalking = false;
        }

        if (wasMovingLastFrame && !movementResult.ReceivedMovementInput)
            lastMoveMade = true;

        wasMovingLastFrame = movementResult.ReceivedMovementInput;
    }
    
    // Call this method after the server has confirmed the final position
    // TODO, lots of things to interpolate between these two positions
    public void ReconcilePosition(Vector3 serverPosition)
    {
        transform.position = serverPosition;
    }
}
