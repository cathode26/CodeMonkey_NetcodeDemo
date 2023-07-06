using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;
    private KitchenCounter kitchenCounter;

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return KitchenObjectSO; 
    }
    public void SetKitchenCounter(KitchenCounter kitchenCounter)
    {
        this.kitchenCounter = kitchenCounter;
    }
    public KitchenCounter GetKitchenCounter()
    {
        return kitchenCounter;
    }
}
