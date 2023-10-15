using Unity.Netcode;
using UnityEngine;
public class ClientMovement : MonoBehaviour
{
    private IMovement _movementLogic;
    private Transform _originalParent;
    private ServerMovement _serverMovement;

    private bool _isWalking = false;
    public bool IsWalking { private set { _isWalking = value; } get { return _isWalking; } }
    public delegate void WalkingState(bool isWalking);
    public event WalkingState OnWalkingStateChanged;

    private SynchronizedNetworkTransform syncronizedNetworkTransform;
    NetworkObject networkObject;
    private bool wasMovingLastFrame = false;
    private float firstCommandSentTime = -1f;
    private float firstUpdateReceivedTime = -1f;
    private float estimatedLatency = -1f;
    private float reattachTime = -1f; // Time when you should reattach to the original parent
    private bool lastMoveMade = false;
    public void OnNetworkSpawn(ServerMovement serverMovement, PlayerProperties playerProperties)
    {
        networkObject = transform.parent.GetComponent<NetworkObject>();
        this._serverMovement = serverMovement;
        _movementLogic = new BaseMovement(playerProperties, transform);

        if (networkObject.IsOwner)
        {
            // Add a callback to the OnValueChanged event
            syncronizedNetworkTransform = transform.parent.GetComponent<SynchronizedNetworkTransform>();
            syncronizedNetworkTransform.OnNetworkTransformUpdatesComplete += OnFinalPositionChanged;
        }
        else
        {
            enabled = false;
        }
    }
    public void OnNetworkDespawn()
    {
        if (networkObject.IsOwner)
        {
            // Remove a callback to the OnValueChanged event
            syncronizedNetworkTransform.OnNetworkTransformUpdatesComplete -= OnFinalPositionChanged;
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
                ReAttachParent();
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
        (bool moved, Vector2 dir) movementData = GameInput.Instance.GetMovementVectorNormalized();

        if (movementData.moved)
        {
            if (!wasMovingLastFrame)
            {
                syncronizedNetworkTransform.BeginSyncronizedNetworkTransform();
                firstCommandSentTime = Time.time; // Store the time when the first command is sent
                firstUpdateReceivedTime = -1;
            }

            // Detach from parent and store the reference
            if (transform.parent)
            {
                _originalParent = transform.parent;
                transform.parent = null;
            }

            //The DetermineMovementAbilityAndDirection function returns the allowed direction that the player can move in
            //It may change the direction it can move in if it is blocked
            float clientDeltaTime = Time.deltaTime;
            (bool canMove, Vector3 movDir) tryMoveData = _movementLogic.DetermineMovementAbilityAndDirection(movementData.dir, clientDeltaTime);
            //We always take the uneditted direction of the input as the rotation direction because rotation is any direction is allowed
            Vector3 rotateDir = new Vector3(movementData.dir.x, 0.0f, movementData.dir.y);
            if (tryMoveData.canMove)
            {
                if (IsWalking == false)
                    OnWalkingStateChanged?.Invoke(true);
                
                IsWalking = true;
                _movementLogic.MoveAndRotatePlayer(tryMoveData.movDir, rotateDir, clientDeltaTime);
                // Send the movement data to the server
                //Debug.Log("outstandingMovementCommands + 1 " + outstandingMovementCommands);
            }
            else
            {
                _movementLogic.RotatePlayer(rotateDir, clientDeltaTime);
                //Debug.Log("RotatePlayer outstandingMovementCommands + 1 " + outstandingMovementCommands);
                if (IsWalking == true)
                    OnWalkingStateChanged?.Invoke(false);
                IsWalking = false;
            }
            _serverMovement.MoveAndRotatePlayerServerRpc(movementData.dir, clientDeltaTime);
        }
        else
        {
            if (IsWalking)
                OnWalkingStateChanged?.Invoke(false);
            IsWalking = false;
        }

        if (wasMovingLastFrame && !movementData.moved)
            lastMoveMade = true;

        wasMovingLastFrame = movementData.moved;
    }
    
    // Call this method after the server has confirmed the final position
    // TODO, lots of things to interpolate between these two positions
    public void ReconcilePosition(Vector3 serverPosition)
    {
        transform.position = serverPosition;
    }
}
