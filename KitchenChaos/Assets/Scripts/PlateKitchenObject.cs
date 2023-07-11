using System;
using System.Collections.Generic;
using System.Linq;
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

        // Add the ingredient to the plate and destroy the ingredient object
        platedFoods.Add(ingredient.GetKitchenObjectSO());
        ingredient.DestroySelf();

        OnIngredientAddedEvent.Invoke(this, new OnIngredientAddedEventArgs() { kitchenObjectSO = ingredient.GetKitchenObjectSO() });

        return true;
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
    public HashSet<KitchenObjectSO> GetPlatedFoods()
    {
        return platedFoods; 
    }
}