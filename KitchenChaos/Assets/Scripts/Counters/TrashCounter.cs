
public class TrashCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject kitchenObject = player.GetKitchenObject();
            Signals.Get<ServerSoundSignalList.OnAnyObjectTrashedSignal>().Dispatch(kitchenObject.transform.position);
            kitchenObject.ReturnKitchenObject();
        }
    }
}
