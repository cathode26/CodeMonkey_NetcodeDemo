using System;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField]
    protected KitchenObjectSO kitchenObjectSO;
    public override void Interact(Player player)
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectsParent(player);

        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }
}
