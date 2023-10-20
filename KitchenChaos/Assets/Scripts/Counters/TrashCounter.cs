using SoundSignalList;

public class TrashCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            Signals.Get<OnAnyObjectTrashedSignal>().Dispatch(kitchenObject.transform.position);
            kitchenObject.DestroySelf();
        }
    }
}
