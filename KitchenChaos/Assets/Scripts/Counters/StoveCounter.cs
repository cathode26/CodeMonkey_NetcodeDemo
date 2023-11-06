using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static IHasProgress;

public class StoveCounter : BaseCounter, IHasProgress
{
    [SerializeField]
    private CookingRecipeSO[] cookingRecipesSO;
    private CookingRecipeSO cookingRecipeSO;

    public event EventHandler<OnPlayerCookingEventArgs> OnPlayerSetObject;
    public event EventHandler OnPlayerRemovedObject;
    public event EventHandler OnStartProgress;
    public event EventHandler OnEndProgress;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    private float cookingProgress = 0;
    private bool isCooking = false;
    private float latencyDelay = 0;

    public class OnPlayerCookingEventArgs : EventArgs
    {
        public CookingRecipeSO.State state;
    }
    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject() && IsKitchenObjectCookable(player.GetKitchenObject()))
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(this);
            cookingProgress = 0;
            if (IsServer)
                latencyDelay = LatencyManager.Instance.GetLongestRoundTripTime() / 2.0f;

            isCooking = true;
            KitchenObjectSO maybeUncookedKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
            cookingRecipeSO = cookingRecipesSO.FirstOrDefault(uncooked => uncooked.input == maybeUncookedKitchenObjectSO);
            //animate the cooking
            OnPlayerSetObject?.Invoke(this, new OnPlayerCookingEventArgs() { state = cookingRecipeSO.cookState });
            //show the progress bar
            OnStartProgress?.Invoke(this, EventArgs.Empty);
            OnStartProgressServerRpc(LatencyManager.Instance.GetAverageRoundTripTime() / 2.0f, GetCookingRecipeSOIndex(cookingRecipeSO));
        }
        else if (HasKitchenObject() && !player.HasKitchenObject() && !player.WaitingOnNetwork)
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
            OnPlayerRemovedObject?.Invoke(this, EventArgs.Empty);
            isCooking = false;
            OnEndProgress?.Invoke(this, EventArgs.Empty);
            OnEndProgressServerRpc();
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
                {
                    OnPlayerRemovedObject?.Invoke(this, EventArgs.Empty);
                    isCooking = false;
                    OnEndProgress?.Invoke(this, EventArgs.Empty);
                    OnEndProgressServerRpc();
                }
            }
        }
    }
    private void Update()
    {
        if (isCooking)
        {
            cookingProgress += Time.deltaTime;
            //animate the progress bar
            OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs() { progressNormalized = cookingProgress / (latencyDelay + cookingRecipeSO.cookTime) });
            KitchenObjectSO cookedKitchenObjectSO = cookingRecipeSO.output;
            if (cookedKitchenObjectSO && cookingProgress >= (latencyDelay + cookingRecipeSO.cookTime))
            {
                if (IsServer)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(cookedKitchenObjectSO, this);
                }
                cookingRecipeSO = cookingRecipesSO.FirstOrDefault(cooked => cooked.input == cookedKitchenObjectSO);
                cookingProgress = 0.0f;

                if (cookingRecipeSO.cookState == CookingRecipeSO.State.Burned)
                {
                    OnPlayerSetObject?.Invoke(this, new OnPlayerCookingEventArgs() { state = cookingRecipeSO.cookState });
                    isCooking = false;
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnStartProgressServerRpc(float latency, int recipeSOIndex, ServerRpcParams serverRpcParams = default)
    {
        OnStartProgressClientRpc(latency, serverRpcParams.Receive.SenderClientId, recipeSOIndex, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void OnStartProgressClientRpc(float latency, ulong clientId, int recipeSOIndex, ClientRpcParams clientRpcParams)
    {
        if (IsServer)
            latencyDelay = LatencyManager.Instance.GetLongestRoundTripTime() / 2.0f;
        cookingProgress = LatencyManager.Instance.GetAverageRoundTripTime() / 2.0f + latency;
        cookingRecipeSO = cookingRecipesSO[recipeSOIndex];
        isCooking = true;
        //animate the cooking
        OnPlayerSetObject?.Invoke(this, new OnPlayerCookingEventArgs() { state = cookingRecipeSO.cookState });
        //show the progress bar
        OnStartProgress?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnEndProgressServerRpc(ServerRpcParams serverRpcParams = default)
    {
        OnEndProgressClientRpc(ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void OnEndProgressClientRpc(ClientRpcParams clientRpcParams)
    {
        OnPlayerRemovedObject?.Invoke(this, EventArgs.Empty);
        isCooking = false;
        OnEndProgress?.Invoke(this, EventArgs.Empty);
    }
    private bool IsKitchenObjectCookable(KitchenObject kitchenObject)
    {
        KitchenObjectSO maybeUncookedKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
        CookingRecipeSO cookingRecipeSO = cookingRecipesSO.FirstOrDefault(uncooked => uncooked.input == maybeUncookedKitchenObjectSO);
        return cookingRecipeSO != null;
    }
    public bool IsCooked()
    {
        if(cookingRecipeSO)
            return cookingRecipeSO.cookState == CookingRecipeSO.State.Cooked;
        else
            return false;
    }
    private int GetCookingRecipeSOIndex(CookingRecipeSO cookingRecipeSO)
    {
        for (int i = 0; i < cookingRecipesSO.Length; i++)
        {
            if (cookingRecipesSO[i] == cookingRecipeSO)
                return i;
        }
        Debug.LogError("StoveCounter.GetCookingRecipeSOIndex Couldnt find CookingRecipeSO " + cookingRecipeSO);
        return -1;
    }
}
