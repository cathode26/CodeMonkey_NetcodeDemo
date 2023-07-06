using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField]
    protected KitchenObjectSO kitchenObjectSO;
    public override void Interact(Player player)
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectsParent(player);
    }
}
