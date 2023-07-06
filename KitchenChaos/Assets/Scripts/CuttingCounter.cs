using System;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField]
    private KitchenObjectSO cutKitchenObjectSO;

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
            GetKitchenObject().DestroySelf();
            KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
        }
    }
}
