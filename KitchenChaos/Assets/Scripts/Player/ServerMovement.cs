using Unity.Netcode;
using UnityEngine;

public class ServerMovement : NetworkBehaviour
{
    private PlayerProperties _playerProperties;
    private IMovement _movementLogic;
    private ClientMovement _clientMovement;
    private Player _playerComponent;
    private ClientTimeData _clientTimeData = null;
    private float _bufferTime = 5.0f;
    private float _commandTimeout = 5.0f;

    private class ClientTimeData
    {
        public float AccumulatedDeltaTime { get; set; } = 0f;
        public float StartTime { get; set; } = Time.time;
        public float LastReceivedCommandTime { get; set; } = Time.time; // New property
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _clientMovement = GetComponentInChildren<ClientMovement>();
        _playerComponent = GetComponentInChildren<Player>();
        _playerProperties = GetComponentInChildren<PlayerProperties>();
        
        //IsOwner is used to know if this is the player object, meaning was this network object spawned for the player
        //We do this to prevent input from going to a different player
        if (!GetComponent<NetworkObject>().IsOwner)
            enabled = false;

        _movementLogic = new BaseMovement(_playerProperties, transform);
        _clientMovement.OnNetworkSpawn(this, _playerProperties);
        _playerComponent.OnNetworkSpawn(this);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _clientMovement.OnNetworkDespawn();
    }
    /* 
      * Input: the input vector from unity's new input system
      * Function will rotate the players transform and move the players transform by the movDir
      */
    [ServerRpc(RequireOwnership = false)]
    public void MoveAndRotatePlayerServerRpc(Vector2 direction, float clientDeltaTime, ServerRpcParams serverRpcParams = default)
    {
        /*
        // Initialize client data if not present
        if (_clientTimeData == null)
            _clientTimeData = new ClientTimeData { StartTime = Time.time, LastReceivedCommandTime = Time.time };

        if (!IsValidDeltaTime(clientDeltaTime))
        {
            Debug.LogError("MoveAndRotatePlayerServerRpc rejected DeltaTIME!!");
            return;
        }
        */
        _movementLogic.HandleMovement((true, direction), clientDeltaTime);
    }
    private bool IsValidDeltaTime(float clientDeltaTime)
    {
        var clientData = _clientTimeData;

        // Accumulate the client's delta time
        clientData.AccumulatedDeltaTime += clientDeltaTime;

        // Calculate the expected accumulated delta time based on real time
        float expectedAccumulatedDeltaTime = (Time.time - clientData.StartTime) + _bufferTime;

        // Check if the accumulated delta time from the client exceeds the expected value
        if (clientData.AccumulatedDeltaTime > expectedAccumulatedDeltaTime)
        {
            // Reset the accumulated delta time if it's been too long since the last movement command
            if (Time.time - clientData.LastReceivedCommandTime > _commandTimeout)
            {
                clientData.AccumulatedDeltaTime = 0;
            }
            return false;
        }

        // Update the time of the last received command
        clientData.LastReceivedCommandTime = Time.time;

        return true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void HandleInterpolationServerRpc(Vector3 clientPosition, ServerRpcParams serverRpcParams = default)
    {
        // Check if the server can move to the client's position
        Vector3 directionToClient = (clientPosition - transform.position).normalized;
        float distanceToClient = Vector3.Distance(clientPosition, transform.position);

        if (transform.position == clientPosition)
        {
            // "Jiggle" the transform's position
            transform.position += new Vector3(0.01f, 0.01f, 0.01f); // Add a tiny offset
            Debug.Log("Jiggle Position");

        }
        else if (!Physics.CapsuleCast(transform.position, transform.position + Vector3.up * _playerProperties.PlayerHeight, _playerProperties.PlayerRadius, directionToClient, distanceToClient))
        {
            // Server can move to client's position
            transform.position = clientPosition;// Vector3.Lerp(transform.position, clientPosition, Time.deltaTime * movementSpeed.Value);
            Debug.Log("Set position to " + transform.position);
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
            Debug.Log("ReconcilePositionClientRpc");
            ReconcilePositionClientRpc(clientRpcParams);
        }
    }
    [ClientRpc]
    public void ReconcilePositionClientRpc(ClientRpcParams rpcParams = default)
    {
        _clientMovement.ReAttachParent();
    }
}

