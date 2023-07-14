using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField]
    private Image timeImage;
    [SerializeField]
    private GameObject ui;

    private void OnEnable()
    {
        KitchenGameManager.Instance.OnStateChanged += Instance_OnStateChanged;
    }
    private void OnDisable()
    {
        KitchenGameManager.Instance.OnStateChanged -= Instance_OnStateChanged;
    }
    private void Instance_OnStateChanged()
    {
        if (KitchenGameManager.Instance.IsGamePlaying())
            ui.SetActive(true);
        else
            ui.SetActive(false);
    }
    private void Update()
    {
        if(KitchenGameManager.Instance.IsGamePlaying())
            timeImage.fillAmount = KitchenGameManager.Instance.GetPlayingTimerNormalized();
    }
}
