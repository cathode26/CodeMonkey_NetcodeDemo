using System;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField]
    protected KitchenObjectSO kitchenObjectSO;
    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject() && !player.WaitingOnNetwork)
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);
            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
            InteractServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId);
        InteractClientRpc(clientRpcParams);
    }
    [ClientRpc]
    private void InteractClientRpc(ClientRpcParams clientRpcParams)
    {
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }
}
