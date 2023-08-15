using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{

    private const string POPUP = "Popup";
    [SerializeField] private GameObject ui;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Color successColor;
    [SerializeField] private Color failedColor;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failedSprite;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        ui.SetActive(false);
    }

    private void OnEnable()
    {
        DeliveryManager.OnRecipeSuccessChanged += DeliveryManager_OnRecipeSuccessChanged;
        DeliveryManager.OnRecipeFailedChanged += DeliveryManager_OnRecipeFailedChanged;
    }
    private void OnDisable()
    {
        DeliveryManager.OnRecipeSuccessChanged -= DeliveryManager_OnRecipeSuccessChanged;
        DeliveryManager.OnRecipeFailedChanged -= DeliveryManager_OnRecipeFailedChanged;
    }

    private void DeliveryManager_OnRecipeFailedChanged()
    {
        ui.SetActive(true);
        animator.SetTrigger(POPUP);
        backgroundImage.color = failedColor;
        iconImage.sprite = failedSprite;
        messageText.text = "DELIVERY\nFAILED";
    }

    private void DeliveryManager_OnRecipeSuccessChanged()
    {
        ui.SetActive(true);
        animator.SetTrigger(POPUP);
        backgroundImage.color = successColor;
        iconImage.sprite = successSprite;
        messageText.text = "DELIVERY\nSUCCESS";
    }
}
