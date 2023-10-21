using GameSignalList;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerMovement : NetworkBehaviour
{
    private PlayerProperties _playerProperties;
    private IMovement _movementLogic;
    private ClientMovement _clientMovement;
    private Player _playerComponent;
    private ClientTimeData _clientTimeData = null;
    private float _maxBufferTime = 0.5f;    //if the game were running as slow as 30fps
    private float _bufferTime;     
    private const float TARGET_DELTA_TIME = 1f / 240f; // Targeting 240 fps.
    private float _latency = 0.0f;

    private class ClientTimeData
    {
        public float AccumulatedDeltaTime { get; set; } = 0f;
        public float StartTime { get; set; } = Time.time;
        public float LastReceivedCommandTime { get; set; } = Time.time; // New property
        public bool IsRunning { get; set; } = true;
        public float PunishmentTime { get; set; } = 0f;
        public bool Punished { get; set; } = false;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _bufferTime = _maxBufferTime;
        _clientMovement = GetComponentInChildren<ClientMovement>();
        _playerComponent = GetComponentInChildren<Player>();
        _playerProperties = GetComponentInChildren<PlayerProperties>();

        _movementLogic = new BaseMovement(_playerProperties, transform);
        _clientMovement.OnNetworkSpawn(this, _playerProperties);
        _playerComponent.OnNetworkSpawn(this);

        //IsOwner is used to know if this is the player object, meaning was this network object spawned for the player
        //We do this to prevent input from going to a different player
        if (!GetComponent<NetworkObject>().IsOwner)
            enabled = false;
        
        Signals.Get<OnPlayerSpawnedSignal>().Dispatch(_playerComponent);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _clientMovement.OnNetworkDespawn();
        if (GetComponent<NetworkObject>().IsOwner)
            Signals.Get<OnPlayerDespawnedSignal>().Dispatch(_playerComponent);
    }
    /* 
      * Input: the input vector from unity's new input system
      * Function will rotate the players transform and move the players transform by the movDir
      */
    [ServerRpc(RequireOwnership = false)]
    public void MoveAndRotatePlayerServerRpc(Vector2 direction, float clientDeltaTime, ServerRpcParams serverRpcParams = default)
    {
        // Initialize client data if not present
        if (_clientTimeData == null)
        {
            _clientTimeData = new ClientTimeData { IsRunning = true, StartTime = Time.time, LastReceivedCommandTime = Time.time };
        }
        else if (_clientTimeData.IsRunning == false)
        {
            float timeSinceLastCommand = Time.time - _clientTimeData.LastReceivedCommandTime;
            if (!_clientTimeData.Punished && (_clientTimeData.AccumulatedDeltaTime == 0 || timeSinceLastCommand > _latency * 4.0f))
            {
                _clientTimeData.StartTime = Time.time;
                _clientTimeData.AccumulatedDeltaTime = 0;
            }
            else
            {
                _clientTimeData.StartTime += timeSinceLastCommand;
            }
            _clientTimeData.IsRunning = true;
        }

        if (!IsValidDeltaTime(ref clientDeltaTime))
        {
            Debug.Log("MoveAndRotatePlayerServerRpc rejected DeltaTIME corrected it because of possible cheating!!");
            // Server cannot move to client's position due to an obstacle
            // Notify the client to reconcile its position
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            CorrectPositionClientRpc(clientRpcParams);
        }

        // Update the time of the last received command
        _clientTimeData.LastReceivedCommandTime = Time.time;

        _movementLogic.HandleMovement((true, direction), clientDeltaTime);
    }
    private bool IsValidDeltaTime(ref float clientDeltaTime)
    {
        var clientData = _clientTimeData;

        if (_clientTimeData.Punished && _clientTimeData.PunishmentTime + 30 < Time.time)
        {
            _clientTimeData.Punished = false;
            _clientTimeData.StartTime = Time.time;
            _clientTimeData.AccumulatedDeltaTime = 0;
            Debug.Log("_clientTimeData.Punished = false && clientDeltaTime = " + clientDeltaTime);
        }

        // Accumulate the client's delta time
        clientData.AccumulatedDeltaTime += clientDeltaTime;

        // Calculate the expected accumulated delta time based on real time
        float expectedAccumulatedDeltaTime = (Time.time - clientData.StartTime) + _bufferTime;

        // Check if the accumulated delta time from the client exceeds the expected value
        if (clientData.AccumulatedDeltaTime > expectedAccumulatedDeltaTime)
        {
            clientData.Punished = true;
            clientData.PunishmentTime = Time.time;
            clientDeltaTime = TARGET_DELTA_TIME;
            //Debug.Log("HOW DARE YOU!!");
            return false;
        }

        if (clientData.Punished)
            clientDeltaTime = TARGET_DELTA_TIME;

        return true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void HandleInterpolationServerRpc(Vector3 clientPosition, ServerRpcParams serverRpcParams = default)
    {
        Vector3 directionToClient = (clientPosition - transform.position).normalized;
        float distanceToClient = Vector3.Distance(clientPosition, transform.position);

        if (transform.position == clientPosition)
        {
            // "Jiggle" the transform's position
            JigglePosition();
        }
        else if (distanceToClient > _playerProperties.PlayerRadius)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            CorrectPositionClientRpc(clientRpcParams);
        }
        else if (!Physics.CapsuleCast(transform.position, transform.position + Vector3.up * _playerProperties.PlayerHeight, _playerProperties.PlayerRadius, directionToClient, distanceToClient))
        {
            // Server can move to client's position
            transform.position = clientPosition;// Vector3.Lerp(transform.position, clientPosition, Time.deltaTime * movementSpeed.Value);
            Debug.Log("HandleInterpolationServerRpc Set position to " + transform.position);
        }
        else
        {
            // Server cannot move to client's position due to an obstacle
            // Notify the client to reconcile its position
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            Debug.Log("HandleInterpolationServerRpc ReconcilePositionClientRpc");
            ReconcilePositionClientRpc(clientRpcParams);
        }
    }
    private void JigglePosition()
    {
        float delta = 0.001f;
        (bool canMove, Vector3 movDir) tryMoveRight = _movementLogic.DetermineMovementAbilityAndDirection(new Vector2(1,0), delta);
        (bool canMove, Vector3 movDir) tryMoveLeft = _movementLogic.DetermineMovementAbilityAndDirection(new Vector2(-1,0), delta);
        (bool canMove, Vector3 movDir) tryMoveUp = _movementLogic.DetermineMovementAbilityAndDirection(new Vector2(0, 1), delta);
        (bool canMove, Vector3 movDir) tryMoveDown = _movementLogic.DetermineMovementAbilityAndDirection(new Vector2(0, -1), delta);

        List<Vector3> directions = new List<Vector3>();
        if (tryMoveRight.canMove && tryMoveUp.canMove)
            directions.Add(new Vector3(1, 0, 1));
        if (tryMoveLeft.canMove && tryMoveUp.canMove)
            directions.Add(new Vector3(-1, 0, 1));
        if (tryMoveRight.canMove && tryMoveDown.canMove)
            directions.Add(new Vector3(1, 0, -1));
        if (tryMoveLeft.canMove && tryMoveDown.canMove)
            directions.Add(new Vector3(-1, 0, -1));
        
        _movementLogic.MovePlayer(directions[Random.Range(0, directions.Count - 1)], delta);
        Debug.Log("Jiggle Position");
    }
    [ServerRpc(RequireOwnership = false)]
    public void PositionReconciledServerRpc()
    {
        Debug.Log("Position Reconciled");
        _clientTimeData.IsRunning = false;
    }
    [ClientRpc]
    public void ReconcilePositionClientRpc(ClientRpcParams rpcParams = default)
    {
        _clientMovement.ReAttachParent();
    }
    [ClientRpc]
    public void CorrectPositionClientRpc(ClientRpcParams rpcParams = default)
    {
        _clientMovement.CorrectPosition();
    }
    [ServerRpc]
    public void SetLatencyServerRpc(float latency)
    {
        _latency = latency;
    }
}

