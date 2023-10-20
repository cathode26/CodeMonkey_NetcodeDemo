using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    [SerializeField]
    private RecipeBookSO recipeBookSO;  // The recipe associated with this plate
    public static event System.Action<PlatingRecipeSO> OnAddPlatingRecipeChanged;
    public static event System.Action<PlatingRecipeSO> OnRemovedPlatingRecipeChanged;

    private List<PlatingRecipeSO> waitingOnPlatesSO = new List<PlatingRecipeSO>();
    private float spawnRecipeTimer = 0.0f;
    private float spawnRecipeTimerMax = 15.0f;
    private int waitingRecipesMax = 4;

    public RecipeBookSO RecipeBookSO { get => recipeBookSO; private set => recipeBookSO = value; }

    private void Update()
    {
        if (!IsServer)
            return;

        spawnRecipeTimer += Time.deltaTime;
        if (KitchenGameManager.Instance.IsGamePlaying() && spawnRecipeTimer >= spawnRecipeTimerMax && waitingOnPlatesSO.Count < waitingRecipesMax)
        {
            System.Guid guid = System.Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            int seed = System.BitConverter.ToInt32(bytes, 0);
            Random.InitState(seed);
            spawnRecipeTimer = 0.0f;
            SpawnNewWaitingRecipeClientRpc(Random.Range(0, recipeBookSO.recipes.Count));
        }
    }
    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int index)
    {
        PlatingRecipeSO platingRecipeSO = recipeBookSO.recipes[index];
        waitingOnPlatesSO.Add(platingRecipeSO);
        OnAddPlatingRecipeChanged.Invoke(platingRecipeSO);
        Debug.Log("Make a " + platingRecipeSO.recipeName);
    }
    public bool DeliverPlate(PlateKitchenObject plateKitchenObject, out string recipeName)
    {
        HashSet<KitchenObjectSO> platedFoods = plateKitchenObject.GetPlatedFoods();
        for(int i=0; i<waitingOnPlatesSO.Count; ++i)
        {
            PlatingRecipeSO platingRecipeSO = waitingOnPlatesSO[i];
            if (platingRecipeSO.input.Count == platedFoods.Count && platedFoods.All(food => platingRecipeSO.input.Contains(food)))
            {
                recipeName = platingRecipeSO.recipeName;
                OnDeliveredPlateServerRpc(i);
                return true;
            }
        }
        OnDeliveryFailedServerRpc();
        recipeName = "Wrong Recipe";
        return false;
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnDeliveryFailedServerRpc()
    {
        OnDeliveryFailedClientRpc();
    }
    [ClientRpc]
    public void OnDeliveryFailedClientRpc()
    {
        Signals.Get<GameSignalList.OnRecipeFailedSignal>().Dispatch();
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnDeliveredPlateServerRpc(int index)
    {
        OnDeliveredPlateClientRpc(index);
    }
    [ClientRpc]
    public void OnDeliveredPlateClientRpc(int index)
    {
        PlatingRecipeSO platingRecipeSO = waitingOnPlatesSO[index];
        OnRemovedPlatingRecipeChanged.Invoke(platingRecipeSO);
        waitingOnPlatesSO.Remove(platingRecipeSO);
        Signals.Get<GameSignalList.OnRecipeSuccessSignal>().Dispatch();
    }
}
