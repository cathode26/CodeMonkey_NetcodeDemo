using System;
using System.Linq;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField]
    private CuttingRecipeSO[] cuttingRecipesSO;

    public event EventHandler OnPlayerCutObject;
    private int cuttingProgress = 0;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(this);
            cuttingProgress = 0;
        }
        else if (HasKitchenObject() && !player.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
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
                    //animate the cutting
                    OnPlayerCutObject?.Invoke(this, EventArgs.Empty);
                    cuttingProgress++;
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
}
