using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] 
    private PlateKitchenObject plateKitchenObject;
    [SerializeField]
    private GameObject IconTemplate;
    private List<GameObject> icons = new List<GameObject>();

    private void OnEnable()
    {
        plateKitchenObject.OnIngredientAddedEvent += PlateKitchenObject_OnIngredientAddedEvent;
        plateKitchenObject.OnRecipeCompleteEvent += PlateKitchenObject_OnRecipeCompleteEvent;
    }
    private void OnDisable()
    {
        plateKitchenObject.OnIngredientAddedEvent -= PlateKitchenObject_OnIngredientAddedEvent;
        plateKitchenObject.OnRecipeCompleteEvent -= PlateKitchenObject_OnRecipeCompleteEvent;
    }
    private void PlateKitchenObject_OnIngredientAddedEvent(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        GameObject icon = Instantiate(IconTemplate, transform);
        if (icon == null)
        {
            Debug.LogError("Failed to instantiate IconTemplate.");
            return;
        }

        IconSetter iconSetter = icon.GetComponent<IconSetter>();
        if (iconSetter == null)
        {
            Debug.LogError("IconSetter component missing in instantiated icon.");
            return;
        }

        iconSetter.SetSprite(e.kitchenObjectSO.sprite);
        icon.SetActive(true);
        icons.Add(icon);
    }
    private void PlateKitchenObject_OnRecipeCompleteEvent(object sender, EventArgs e)
    {
        foreach(GameObject icon in icons) 
            Destroy(icon);
        icons.Clear();
    }
}
