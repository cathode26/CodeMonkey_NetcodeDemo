using Unity.Netcode;
using UnityEngine;

public class ServerSoundManager : NetworkBehaviour
{
    private void Awake()
    {
        Signals.Get<ServerSoundSignalList.OnRecipeSuccessSignal>().AddListener(OnRecipeSuccess);
        Signals.Get<ServerSoundSignalList.OnRecipeFailedSignal>().AddListener(OnRecipeFailed);
        Signals.Get<ServerSoundSignalList.OnChoppedSignal>().AddListener(OnChopped);
        Signals.Get<ServerSoundSignalList.OnObjectPickupSignal>().AddListener(OnObjectPickup);
        Signals.Get<ServerSoundSignalList.OnObjectDropSignal>().AddListener(OnObjectDrop);
        Signals.Get<ServerSoundSignalList.OnAnyObjectTrashedSignal>().AddListener(OnAnyObjectTrashed);
        Signals.Get<ServerSoundSignalList.OnFootStepsSignal>().AddListener(OnFootSteps);
        Signals.Get<ServerSoundSignalList.OnWarningSignal>().AddListener(OnWarning);
    }
    public override void OnDestroy()
    {
        Signals.Get<ServerSoundSignalList.OnRecipeSuccessSignal>().RemoveListener(OnRecipeSuccess);
        Signals.Get<ServerSoundSignalList.OnRecipeFailedSignal>().RemoveListener(OnRecipeFailed);
        Signals.Get<ServerSoundSignalList.OnChoppedSignal>().RemoveListener(OnChopped);
        Signals.Get<ServerSoundSignalList.OnObjectPickupSignal>().RemoveListener(OnObjectPickup);
        Signals.Get<ServerSoundSignalList.OnObjectDropSignal>().RemoveListener(OnObjectDrop);
        Signals.Get<ServerSoundSignalList.OnAnyObjectTrashedSignal>().RemoveListener(OnAnyObjectTrashed);
        Signals.Get<ServerSoundSignalList.OnFootStepsSignal>().RemoveListener(OnFootSteps);
        Signals.Get<ServerSoundSignalList.OnWarningSignal>().RemoveListener(OnWarning);

        base.OnDestroy();
    }
    private void OnRecipeSuccess(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnRecipeSuccessSignal>().Dispatch(position);
        RecipeSuccessServerRpc(position);
    }
    private void OnRecipeFailed(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnRecipeFailedSignal>().Dispatch(position);
        RecipeFailedServerRpc(position);
    }
    private void OnChopped(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnChoppedSignal>().Dispatch(position);
        ChoppedServerRpc(position);
    }
    private void OnObjectPickup(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnObjectPickupSignal>().Dispatch(position);
        ObjectPickupServerRpc(position);
    }
    private void OnObjectDrop(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnObjectDropSignal>().Dispatch(position);
        ObjectDropServerRpc(position);
    }
    private void OnAnyObjectTrashed(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnAnyObjectTrashedSignal>().Dispatch(position);
        AnyObjectTrashedServerRpc(position);
    }
    private void OnFootSteps(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnFootStepsSignal>().Dispatch(position);
        FootStepsServerRpc(position);
    }
    private void OnWarning(Vector3 position)
    {
        Signals.Get<SoundSignalList.OnWarningSignal>().Dispatch(position);
        WarningServerRpc(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RecipeSuccessServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        RecipeSuccessClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void RecipeSuccessClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnRecipeSuccessSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RecipeFailedServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        RecipeFailedClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void RecipeFailedClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnRecipeFailedSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChoppedServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        ChoppedClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void ChoppedClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnChoppedSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ObjectPickupServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        ObjectPickupClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void ObjectPickupClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnObjectPickupSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ObjectDropServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        ObjectDropClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void ObjectDropClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnObjectDropSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void AnyObjectTrashedServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        AnyObjectTrashedClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void AnyObjectTrashedClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnAnyObjectTrashedSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void FootStepsServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        FootStepsClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void FootStepsClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnFootStepsSignal>().Dispatch(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void WarningServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        WarningClientRpc(position, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void WarningClientRpc(Vector3 position, ClientRpcParams clientRpcParams)
    {
        // If this client is the owner, it already played the sound, so it should return.
        if (IsOwner) return;

        // Play the Recipe Success Signal for non-owner clients.
        Signals.Get<SoundSignalList.OnWarningSignal>().Dispatch(position);
    }
}
