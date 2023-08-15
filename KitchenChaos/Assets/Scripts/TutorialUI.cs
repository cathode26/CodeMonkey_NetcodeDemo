using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject ui;

    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
    }
    private void OnEnable()
    {
        Show();
    }
    private void KitchenGameManager_OnStateChanged()
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
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
