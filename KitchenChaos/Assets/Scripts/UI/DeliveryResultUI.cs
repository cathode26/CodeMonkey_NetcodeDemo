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
        Signals.Get<GameSignalList.OnRecipeSuccessSignal>().AddListener(DeliveryManager_OnRecipeSuccessChanged);
        Signals.Get<GameSignalList.OnRecipeFailedSignal>().AddListener(DeliveryManager_OnRecipeFailedChanged);
    }
    private void OnDisable()
    {
        Signals.Get<GameSignalList.OnRecipeSuccessSignal>().RemoveListener(DeliveryManager_OnRecipeSuccessChanged);
        Signals.Get<GameSignalList.OnRecipeFailedSignal>().RemoveListener(DeliveryManager_OnRecipeFailedChanged);
    }
    private void DeliveryManager_OnRecipeFailedChanged()
    {
        Signals.Get<SoundSignalList.OnRecipeFailedSignal>().Dispatch(transform.position);
        ui.SetActive(true);
        animator.SetTrigger(POPUP);
        backgroundImage.color = failedColor;
        iconImage.sprite = failedSprite;
        messageText.text = "DELIVERY\nFAILED";
    }
    private void DeliveryManager_OnRecipeSuccessChanged()
    {
        Signals.Get<SoundSignalList.OnRecipeSuccessSignal>().Dispatch(transform.position);
        ui.SetActive(true);
        animator.SetTrigger(POPUP);
        backgroundImage.color = successColor;
        iconImage.sprite = successSprite;
        messageText.text = "DELIVERY\nSUCCESS";
    }
}
