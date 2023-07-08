using System;
using System.Linq;
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
                OnStartProgress?.Invoke(this, EventArgs.Empty);
        }
        else if (HasKitchenObject() && !player.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
            OnEndProgress?.Invoke(this, EventArgs.Empty);
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
                if(plateKitchenObject.TryAddIngredient(maybeIngredient))
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
                    OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs() { progressNormalized = (float)cuttingProgress / (float)cuttingRecipeSO.cuttingProgressMax });

                    KitchenObjectSO cutKitchenObjectSO = cuttingRecipeSO.output;
                    if (cutKitchenObjectSO && cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
                    }
                }
            }
        }
    }
    private bool IsKitchenObjectCuttable(KitchenObject kitchenObject)
    {
        KitchenObjectSO maybeUncutKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
        CuttingRecipeSO cuttingRecipeSO = cuttingRecipesSO.FirstOrDefault(uncut => uncut.input == maybeUncutKitchenObjectSO);
        return cuttingRecipeSO != null;
    }
}
