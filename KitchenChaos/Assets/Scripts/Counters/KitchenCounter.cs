using UnityEngine;

public class KitchenCounter : BaseCounter
{
    [SerializeField]
    private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(this);
        }
        else if (HasKitchenObject() && !player.HasKitchenObject() && !player.WaitingOnNetwork)
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectsParent(player);
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
                plateKitchenObject.TryAddIngredient(maybeIngredient);
        }
    }
}
