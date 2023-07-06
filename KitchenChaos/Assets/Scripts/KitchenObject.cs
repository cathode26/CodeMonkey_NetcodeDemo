using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;
    private IKitchenObjectParent kitchenObjectsParent;

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return KitchenObjectSO; 
    }
    public void SetKitchenObjectsParent(IKitchenObjectParent kitchenObjectsParent)
    {
        this.kitchenObjectsParent?.ClearKitchenObject();
        this.kitchenObjectsParent = kitchenObjectsParent;

        if (kitchenObjectsParent.HasKitchenObject())
            Debug.Log("Counter already has a kitchen object");

        kitchenObjectsParent.SetKitchenObject(this);

        transform.parent = kitchenObjectsParent.GetKitchenObjectFollowTransform();
        transform.localPosition = Vector3.zero; 
        transform.localRotation = Quaternion.identity;
    }
    public IKitchenObjectParent GetKitchenObjectsParent()
    {
        return kitchenObjectsParent;
    }
    public void DestroySelf()
    {
        kitchenObjectsParent.ClearKitchenObject();
        Destroy(gameObject);
    }
    public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        //replace with chopped item
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObject.SetKitchenObjectsParent(kitchenObjectParent);
        return kitchenObject;
    }
