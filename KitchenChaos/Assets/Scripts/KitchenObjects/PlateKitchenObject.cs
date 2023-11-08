using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    [SerializeField]
    private Transform plateTopPoint;  // The top point of the plate where ingredients are visually added
    [SerializeField]
    private RecipeBookSO recipeBookSO;  // The recipe associated with this plate

    HashSet<KitchenObjectSO> platedFoods = new HashSet<KitchenObjectSO>();  // A set of all food added to this plate

    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAddedEvent;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    public event EventHandler OnRecipeCompleteEvent;

    // Try to add an ingredient to the plate
    public bool TryAddIngredient(KitchenObject ingredient)
    {
        // If the ingredient isn't part of the recipe or has already been added, do nothing
        if (!IsPartOfRecipe(platedFoods, ingredient) || platedFoods.Contains(ingredient.GetKitchenObjectSO()))
            return false;

        KitchenObjectSO ingredientSO = ingredient.GetKitchenObjectSO();
        //Remove the gameobject because we are switching to a plated visualization
        ingredient.ReturnKitchenObject();

        int kitchenObjectId = KitchenGameMultiplayer.Instance.GetKitchenObjectId(ingredientSO);
        // Add the ingredient to the plate
        platedFoods.Add(ingredientSO);
        OnIngredientAddedEvent.Invoke(this, new OnIngredientAddedEventArgs() { kitchenObjectSO = ingredientSO });

        AddIngredientServerRpc(kitchenObjectId);
        return true;
    }
    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectId, ServerRpcParams serverRpcParams = default)
    {
        AddIngredientClientRpc(kitchenObjectId, ClientRpcManager.Instance.GetClientsExcludeSender(serverRpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectId, ClientRpcParams clientRpcParams)
    {
        KitchenObjectSO ingredientSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSO(kitchenObjectId);
        platedFoods.Add(ingredientSO);
        OnIngredientAddedEvent.Invoke(this, new OnIngredientAddedEventArgs() { kitchenObjectSO = ingredientSO });
    }
    // Check if a kitchen object is part of the associated recipe
    public bool IsPartOfRecipe(HashSet<KitchenObjectSO> platedFoods, KitchenObject kitchenObject)
    {
        //Loop through the platingRecipeSOs and create and filter out all of the PlatingRecipeSO list that dont contain all of the platedFoods
        List<PlatingRecipeSO> recipeSOs = new List<PlatingRecipeSO>();
        foreach (PlatingRecipeSO platingRecipeSO in recipeBookSO.recipes)
        {
            if (!platingRecipeSO.input.Contains(kitchenObject.GetKitchenObjectSO()))
                continue;

            if (platedFoods.All(food => platingRecipeSO.input.Contains(food)))
                return true;
        }
        return false;
    }
    override public void SetVisibility(bool visible)
    {
        base.SetVisibility(visible);
        if (visible == false)
        {
            OnRecipeCompleteEvent?.Invoke(this, EventArgs.Empty);
            platedFoods.Clear();
        }
    }
    override public void SetVisibilityLocal(bool visible)
    {
        base.SetVisibilityLocal(visible);
        if (visible == false)
        {
            OnRecipeCompleteEvent?.Invoke(this, EventArgs.Empty);
            platedFoods.Clear();
        }
    }
    public HashSet<KitchenObjectSO> GetPlatedFoods()
    {
        return platedFoods; 
    }
}