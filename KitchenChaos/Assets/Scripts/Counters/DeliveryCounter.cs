using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    [SerializeField]
    private DeliveryManager deliveryManager;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                if (deliveryManager.DeliverPlate(plateKitchenObject, out string recipeName))
                {
                    Debug.Log("Delivered " + recipeName);
                    player.GetKitchenObject().DestroySelf();
                }
                else
                {
                    Debug.Log("Incorrect Recipe");
                }
            }
        }
    }
}
