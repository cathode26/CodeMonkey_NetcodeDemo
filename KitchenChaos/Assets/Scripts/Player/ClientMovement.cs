using SignalList;
using Unity.Netcode;
using UnityEngine;
public class ClientMovement : MonoBehaviour
{
    private IMovement _movementLogic;
    private Transform _originalParent;
    private ServerMovement _serverMovement;
    private NetworkObject _networkObject;
    private SynchronizedNetworkTransform _syncronizedNetworkTransform;
    private PlayerVisualState _playerVisualState;
    private PlayerProperties _playerProperties;

    private bool wasMovingLastFrame = false;
    private float firstCommandSentTime = -1f;
    private float firstUpdateReceivedTime = -1f;
    private float estimatedLatency = -1f;
    private float reattachTime = -1f; // Time when you should reattach to the original parent
    private bool lastMoveMade = false;
    public void OnNetworkSpawn(ServerMovement serverMovement, PlayerProperties playerProperties)
    {
        _networkObject = transform.parent.GetComponent<NetworkObject>();
        _playerVisualState = transform.GetComponent<PlayerVisualState>();
        _serverMovement = serverMovement;
        _playerProperties = playerProperties;

        if (_networkObject.IsOwner)
        {
            // Add a callback to the OnValueChanged event
            _syncronizedNetworkTransform = transform.parent.GetComponent<SynchronizedNetworkTransform>();
            _syncronizedNetworkTransform.OnNetworkTransformUpdatesComplete += OnFinalPositionChanged;
            Signals.Get<EnableMovementSpeedCheatSignal>().AddListener(EnableMovementSpeedCheat);
            Signals.Get<DisableMovementSpeedCheatSignal>().AddListener(DisableMovementSpeedCheat);

            if (_syncronizedNetworkTransform.IsServer)
                _movementLogic = new BaseMovement(_playerProperties, transform.parent);
            else
                _movementLogic = new BaseMovement(_playerProperties, transform);
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
            Signals.Get<EnableMovementSpeedCheatSignal>().RemoveListener(EnableMovementSpeedCheat);
            Signals.Get<DisableMovementSpeedCheatSignal>().RemoveListener(DisableMovementSpeedCheat);
        }
    }
    public void EnableMovementSpeedCheat(float speedMultiplier)
    {
        if (_networkObject.IsOwner)
        {
            if (_syncronizedNetworkTransform.IsServer)
                _movementLogic = new BaseMovementCheat(_playerProperties, transform.parent, speedMultiplier);
            else
                _movementLogic = new BaseMovementCheat(_playerProperties, transform, speedMultiplier);
        }
    }
    public void DisableMovementSpeedCheat()
    {
        if (_networkObject.IsOwner)
        {
            if (_syncronizedNetworkTransform.IsServer)
                _movementLogic = new BaseMovement(_playerProperties, transform.parent);
            else
                _movementLogic = new BaseMovement(_playerProperties, transform);
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
        //Debug.Log("Time " + Time.time);

        HandleMovement();

        // Check if we're awaiting a position update and if the position has changed
        if (_originalParent && lastMoveMade && _syncronizedNetworkTransform.IsServer == false)
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
        _serverMovement.PositionReconciledServerRpc();
    }
    private void HandleMovement()
    {
        //Controls the movement and rotation of the player according to user input.
        //This is the raw input without consideration of where the player can move to
        MovementResult movementResult = _movementLogic.HandleMovement(GameInput.Instance.GetMovementVectorNormalized(), Time.deltaTime);

        if (movementResult.ReceivedMovementInput && _syncronizedNetworkTransform.IsServer == false)
        {
            // Detach from parent and store the reference
            if (transform.parent)
            {
                _originalParent = transform.parent;
                transform.parent = null;
            }

            if (!wasMovingLastFrame)
            {
                _syncronizedNetworkTransform.BeginSyncronizedNetworkTransform();
                firstCommandSentTime = Time.time; // Store the time when the first command is sent
                firstUpdateReceivedTime = -1;
                // Reset the flag
                lastMoveMade = false;
                reattachTime = 0.0f;
            }
            _serverMovement.MoveAndRotatePlayerServerRpc(movementResult.Direction, movementResult.ClientDeltaTime);
        }

        _playerVisualState.HandleMovement(movementResult);

        if (wasMovingLastFrame && !movementResult.ReceivedMovementInput)
            lastMoveMade = true;

        wasMovingLastFrame = movementResult.ReceivedMovementInput;
    }
    public void CorrectPosition()
    {
        if (_originalParent)
        {
            transform.position = _originalParent.position;
            Debug.Log("Corrected client position " + transform.position);
        }
    }
    // Call this method after the server has confirmed the final position
    // TODO, lots of things to interpolate between these two positions
    public void ReconcilePosition(Vector3 serverPosition)
    {
        transform.position = serverPosition;
    }
}
