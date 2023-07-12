using System;

public class TrashCounter : BaseCounter
{
    public static event Action OnAnyObjectTrashed;
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.DestroySelf();
            OnAnyObjectTrashed?.Invoke();
        }
    }
}
