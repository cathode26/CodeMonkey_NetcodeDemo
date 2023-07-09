using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [SerializeField]
    private PlateKitchenObject plateKitchenObject;

    //map the kitchenObjectSO to a gameobject
    [Serializable]
    struct KitchenObjectSOGO
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject kitchenObjectGO;
    }
    [SerializeField]
    private List<KitchenObjectSOGO> kitchenObjectSOGOs = new List<KitchenObjectSOGO>(); 

    private void OnEnable()
    {
        plateKitchenObject.OnIngredientAddedEvent += PlateKitchenObject_OnIngredientAdded;
        plateKitchenObject.OnRecipeCompleteEvent += PlateKitchenObject_OnRecipeCompleteEvent;
    }
    private void PlateKitchenObject_OnRecipeCompleteEvent(object sender, EventArgs e)
    {
        foreach (KitchenObjectSOGO kitchenObjectSOGO in kitchenObjectSOGOs)
            kitchenObjectSOGO.kitchenObjectGO.SetActive(false);
    }
    private void OnDisable()
    {
        plateKitchenObject.OnIngredientAddedEvent -= PlateKitchenObject_OnIngredientAdded;
    }
    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        KitchenObjectSOGO kitchenObjectSOGO = kitchenObjectSOGOs.FirstOrDefault(pair => pair.kitchenObjectSO == e.kitchenObjectSO);
        kitchenObjectSOGO.kitchenObjectGO.SetActive(true);
    }
}
