using UnityEngine;

public class KitchenCounter : MonoBehaviour
{
    [SerializeField]
    private KitchenCounter otherKitchenCounter;
    [SerializeField]
    private KitchenObjectSO kitchenObjectSO;
    [SerializeField]
    private Transform counterTopPoint;
    private KitchenObject kitchenObject;

    public void Interact()
    {
        if (kitchenObjectSO != null && kitchenObject == null)
        {
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab, counterTopPoint);
            kitchenObjectTransform.transform.localPosition = Vector3.zero;
            kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
            kitchenObject.SetKitchenCounter(this);
        }
        else if(kitchenObject != null)
        {
            kitchenObject.SetKitchenCounter(null);
            Destroy(kitchenObject.gameObject);
            kitchenObject = null;
        }
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            kitchenObject.transform.parent = counterTopPoint;
            kitchenObject.transform.localPosition = Vector3.zero;
            kitchenObject.SetKitchenCounter(this);
        }
    }
    public KitchenObject SwapKitchenObject(KitchenObject kitchenObject)
    {
        KitchenObject oldKitchenObject = this.kitchenObject;
        SetKitchenObject(kitchenObject);
        return oldKitchenObject;
    }
    public void SwapKitchenObject()
    {
        //Lets move the object to the other counter
        if (kitchenObject)
        {
            kitchenObject = otherKitchenCounter.SwapKitchenObject(kitchenObject);
            SetKitchenObject(kitchenObject);
        }
    }
}
