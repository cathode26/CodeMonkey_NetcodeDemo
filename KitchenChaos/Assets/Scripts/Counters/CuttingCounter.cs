using System;
using System.Linq;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField]
    private CuttingRecipeSO[] cuttingRecipesSO;

    public event EventHandler OnPlayerSetCuttableObject;
    public event EventHandler OnPlayerRemovedObject;
    public event EventHandler<OnPlayerCutEventArgs> OnPlayerCutObject;
    private int cuttingProgress = 0;

    public class OnPlayerCutEventArgs : EventArgs
    {
        public float percentCut;
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(this);
            cuttingProgress = 0;

            if (IsKitchenObjectCuttable(kitchenObject))
                OnPlayerSetCuttableObject?.Invoke(this, EventArgs.Empty);
        }
        else if (HasKitchenObject() && !player.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
            OnPlayerRemovedObject?.Invoke(this, EventArgs.Empty);
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
                    OnPlayerCutObject?.Invoke(this, new OnPlayerCutEventArgs() { percentCut = (float)cuttingProgress / (float)cuttingRecipeSO.cuttingProgressMax });

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
