using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static IHasProgress;

public class CuttingCounter : BaseCounter, IHasProgress
{
    [SerializeField]
    private CuttingRecipeSO[] cuttingRecipesSO;

    public event EventHandler OnStartProgress;
    public event EventHandler OnEndProgress;
    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    private int cuttingProgress = 0;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(this);
            cuttingProgress = 0;

            if (IsKitchenObjectCuttable(kitchenObject))
            {
                OnStartProgress?.Invoke(this, EventArgs.Empty);
                StartProgressServerRpc();
            }
        }
        else if (HasKitchenObject() && !player.HasKitchenObject() && !player.WaitingOnNetwork)
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
            OnEndProgress?.Invoke(this, EventArgs.Empty);
            EndProgressServerRpc();
        }
        else if (HasKitchenObject() && player.HasKitchenObject())
        {
            KitchenObject maybePlateObject = GetKitchenObject();
            PlateKitchenObject plateKitchenObject = maybePlateObject as PlateKitchenObject;
            KitchenObject maybeIngredient = player.GetKitchenObject();

            if (plateKitchenObject == null)
            {
                maybeIngredient = maybePlateObject;
                maybePlateObject = player.GetKitchenObject();
                plateKitchenObject = maybePlateObject as PlateKitchenObject;
            }

            if (plateKitchenObject)
            {
                if (plateKitchenObject.TryAddIngredient(maybeIngredient))
                    OnEndProgress?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public override void InteractAlternative(Player player)
    {
        if (HasKitchenObject())
        {
            //Lets start by destroying the food when we cut it
            KitchenObject kitchenObject = GetKitchenObject();

            //determine which cut scriptable object to create with the map
            KitchenObjectSO uncutKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
            if (uncutKitchenObjectSO)
            {
                CuttingRecipeSO cuttingRecipeSO = cuttingRecipesSO.FirstOrDefault(uncut => uncut.input == uncutKitchenObjectSO);
                if (cuttingRecipeSO)
                {
                    cuttingProgress++;
                    //animate the cutting
                    float progress = (float)cuttingProgress / (float)cuttingRecipeSO.cuttingProgressMax;
                    OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs() { progressNormalized = progress });
                    Signals.Get<ServerSoundSignalList.OnChoppedSignal>().Dispatch(player.transform.position);

                    KitchenObjectSO cutKitchenObjectSO = cuttingRecipeSO.output;
                    if (cutKitchenObjectSO && cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
                    {
                        GetKitchenObject().ReturnKitchenObject();
                        KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
                    }
                    CutKitchenObjectServerRpc(progress);
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void StartProgressServerRpc(ServerRpcParams serverRpcParams = default)
    {
        StartProgressClientRpc(ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void StartProgressClientRpc(ClientRpcParams clientRpcParams)
    {
        //Begin the UI
        OnStartProgress?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc(RequireOwnership = false)]
    private void EndProgressServerRpc(ServerRpcParams serverRpcParams = default)
    {
        EndProgressClientRpc(ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void EndProgressClientRpc(ClientRpcParams clientRpcParams)
    {
        //End the UI
        OnEndProgress?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc (RequireOwnership = false)]
    private void CutKitchenObjectServerRpc(float progress, ServerRpcParams serverRpcParams = default)
    {
        CutKitchenObjectClientRpc(progress, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void CutKitchenObjectClientRpc(float progress, ClientRpcParams clientRpcParams)
    {
        //animate the cutting
        OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs() { progressNormalized = progress });
    }
    private bool IsKitchenObjectCuttable(KitchenObject kitchenObject)
    {
        KitchenObjectSO maybeUncutKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
        CuttingRecipeSO cuttingRecipeSO = cuttingRecipesSO.FirstOrDefault(uncut => uncut.input == maybeUncutKitchenObjectSO);
        return cuttingRecipeSO != null;
    }
}
