using System;
using System.Linq;
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
            isCooking = true;
            KitchenObjectSO maybeUncookedKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
            cookingRecipeSO = cookingRecipesSO.FirstOrDefault(uncooked => uncooked.input == maybeUncookedKitchenObjectSO);
            //animate the cooking
            OnPlayerSetObject?.Invoke(this, new OnPlayerCookingEventArgs() { state = cookingRecipeSO.cookState });
            //show the progress bar
            OnStartProgress?.Invoke(this, EventArgs.Empty);
        }
        else if (HasKitchenObject() && !player.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
            OnPlayerRemovedObject?.Invoke(this, EventArgs.Empty);
            isCooking = false;
            OnEndProgress?.Invoke(this, EventArgs.Empty);
        }
    }
    private void Update()
    {
        if (isCooking)
        {
            cookingProgress += Time.deltaTime;
            //animate the progress bar
            OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs() { progressNormalized = cookingProgress / cookingRecipeSO.cookTime });
            KitchenObjectSO cookedKitchenObjectSO = cookingRecipeSO.output;
            if (cookedKitchenObjectSO && cookingProgress >= cookingRecipeSO.cookTime)
            {
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(cookedKitchenObjectSO, this);

                cookingRecipeSO = cookingRecipesSO.FirstOrDefault(cooked => cooked.input == cookedKitchenObjectSO);
                cookingProgress = 0.0f;

                if(cookingRecipeSO.cookState == CookingRecipeSO.State.Burned)
                {
                    OnPlayerSetObject?.Invoke(this, new OnPlayerCookingEventArgs() { state = cookingRecipeSO.cookState });
                    isCooking = false;
                }
            }
        }
    }
    private bool IsKitchenObjectCookable(KitchenObject kitchenObject)
    {
        KitchenObjectSO maybeUncookedKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
        CookingRecipeSO cookingRecipeSO = cookingRecipesSO.FirstOrDefault(uncooked => uncooked.input == maybeUncookedKitchenObjectSO);
        return cookingRecipeSO != null;
    }
}
