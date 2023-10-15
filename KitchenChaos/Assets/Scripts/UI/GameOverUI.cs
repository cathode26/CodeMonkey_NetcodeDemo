using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI recipesDeliveredText;
    [SerializeField]
    GameObject ui; 
    private int successfulRecipesAmount = 0;
    

    private void Awake()
    {
        recipesDeliveredText.text = "0";
    }
    private void Start()
    {
        Hide();
    }
    private void OnEnable()
    {
        KitchenGameManager.Instance.OnStateChanged += Instance_OnStateChanged;
        DeliveryManager.OnRemovedPlatingRecipeChanged += DeliveryManager_OnRemovedPlatingRecipeChanged;
    }
    private void OnDisable()
    {
        KitchenGameManager.Instance.OnStateChanged -= Instance_OnStateChanged;
        DeliveryManager.OnRemovedPlatingRecipeChanged -= DeliveryManager_OnRemovedPlatingRecipeChanged;
    }
    private void Instance_OnStateChanged()
    {
        if (KitchenGameManager.Instance.IsGameOver())
            Show();
        else
            Hide();
    }
    private void DeliveryManager_OnRemovedPlatingRecipeChanged(PlatingRecipeSO obj)
    {
        successfulRecipesAmount++;
        recipesDeliveredText.text = successfulRecipesAmount.ToString();
    }
    private void Show()
    {
        ui.SetActive(true);
    }
    private void Hide()
    {
        ui.SetActive(false);
    }
}
