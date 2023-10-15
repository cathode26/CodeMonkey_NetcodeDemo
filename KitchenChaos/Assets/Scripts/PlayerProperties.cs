using Unity.Netcode;
using UnityEngine;

public class PlayerProperties : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<float> _movementSpeed = new NetworkVariable<float>(5.0f);
    [SerializeField]
    private NetworkVariable<float> _rotationSpeed = new NetworkVariable<float>(5.0f);
    [SerializeField]
    private NetworkVariable<float> _playerRadius = new NetworkVariable<float>(0.35f);
    [SerializeField]
    private NetworkVariable<float> _playerHeight = new NetworkVariable<float>(2.0f);
    Transform client;

    public float MovementSpeed { get => _movementSpeed.Value; }
    public float RotationSpeed { get => _rotationSpeed.Value; }
    public float PlayerRadius { get => _playerRadius.Value; }
    public float PlayerHeight { get => _playerHeight.Value; }

    [ServerRpc(RequireOwnership = false)]
    public void SetMovementSpeedServerRpc(int movementSpeed)
    {
        this._movementSpeed.Value = movementSpeed;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetRotationSpeedServerRpc(int rotationSpeed)
    {
        this._rotationSpeed.Value = rotationSpeed;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRadiusServerRpc(int playerRadius)
    {
        this._playerRadius.Value = playerRadius;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerHeightServerRpc(int playerHeight)
    {
        this._playerHeight.Value = playerHeight;
    }
}
