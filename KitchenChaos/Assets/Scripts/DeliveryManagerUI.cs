using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField]
    private Transform container;

    [SerializeField]
    private Transform recipeTemplate;

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        DeliveryManager.OnAddPlatingRecipeChanged += DeliveryManager_OnAddPlatingRecipeChanged;
        DeliveryManager.OnRemovedPlatingRecipeChanged += DeliveryManager_OnRemovedPlatingRecipeChanged;
    }
    private void OnDisable()
    {
        DeliveryManager.OnAddPlatingRecipeChanged -= DeliveryManager_OnAddPlatingRecipeChanged;
        DeliveryManager.OnRemovedPlatingRecipeChanged -= DeliveryManager_OnRemovedPlatingRecipeChanged;
    }
    private void DeliveryManager_OnRemovedPlatingRecipeChanged(PlatingRecipeSO obj)
    {
        foreach (Transform child in container)
        {
            if (child.GetComponent<RecipeUISetter>().GetText() == obj.recipeName)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }
    private void DeliveryManager_OnAddPlatingRecipeChanged(PlatingRecipeSO obj)
    {
        Transform recipeTransform = Instantiate(recipeTemplate, container);
        recipeTransform.GetComponent<RecipeUISetter>().SetText(obj);
        recipeTransform.gameObject.SetActive(true);
    }
}
