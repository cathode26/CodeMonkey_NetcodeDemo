using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] 
    private PlateKitchenObject plateKitchenObject;
    [SerializeField]
    private GameObject IconTemplate;

    private void OnEnable()
    {
        plateKitchenObject.OnIngredientAddedEvent += PlateKitchenObject_OnIngredientAddedEvent;
    }
    private void OnDisable()
    {
        plateKitchenObject.OnIngredientAddedEvent -= PlateKitchenObject_OnIngredientAddedEvent;
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
    }
}
