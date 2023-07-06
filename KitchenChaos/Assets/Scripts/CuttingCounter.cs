using System;
using System.Linq;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField]
    private CuttingRecipeSO[] cuttingRecipesSO;

    public event EventHandler OnPlayerCutObject;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(this);
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
            //animate the cutting
            OnPlayerCutObject?.Invoke(this, EventArgs.Empty);
            //Lets start by destroying the food when we cut it
            KitchenObject kitchenObject = GetKitchenObject();
            
            //determine which cut scriptable object to create with the map
            KitchenObjectSO uncutKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
            if (uncutKitchenObjectSO)
            {
                CuttingRecipeSO cuttingRecipeSO = cuttingRecipesSO.FirstOrDefault(uncut => uncut.input == uncutKitchenObjectSO);
                if (cuttingRecipeSO)
                {
                    KitchenObjectSO cutKitchenObjectSO = cuttingRecipeSO.output;
                    if (cutKitchenObjectSO)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
                    }
                }
            }
        }
    }
}
