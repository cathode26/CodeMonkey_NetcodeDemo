using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    [SerializeField]
    private PlatingRecipeSO platingRecipeSO;  // The recipe associated with this plate
    [SerializeField]
    private Transform plateTopPoint;  // The top point of the plate where ingredients are visually added
    HashSet<KitchenObjectSO> platedFood = new HashSet<KitchenObjectSO>();  // A set of all food added to this plate

    // Try to add an ingredient to the plate
    public bool TryAddIngredient(KitchenObject ingredient)
    {
        // If the ingredient isn't part of the recipe or has already been added, do nothing
        if (!IsPartOfRecipe(ingredient) || platedFood.Contains(ingredient.GetKitchenObjectSO()))
            return false;

        // Add the ingredient to the plate and destroy the ingredient object
        platedFood.Add(ingredient.GetKitchenObjectSO());
        ingredient.DestroySelf();

        // If the recipe is complete, create the output food object on the plate
        if (platedFood.Count == platingRecipeSO.input.Count)
            Instantiate(platingRecipeSO.output.prefab, plateTopPoint);

        return true;
    }

    // Check if a kitchen object is part of the associated recipe
    private bool IsPartOfRecipe(KitchenObject kitchenObject)
    {
        KitchenObjectSO addItemSO = kitchenObject.GetKitchenObjectSO();
        KitchenObjectSO matched = platingRecipeSO.input.FirstOrDefault(recipeItem => recipeItem == addItemSO);
        return matched != null;
    }
}