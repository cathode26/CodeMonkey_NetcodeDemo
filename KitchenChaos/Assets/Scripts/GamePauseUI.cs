using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField]
    private GameObject ui;
    [SerializeField]
    private Button resumeButton;
    [SerializeField]
    private Button mainMenuButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() => KitchenGameManager.Instance.TogglePauseGame());
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.SceneNames.MainMenuScene);
        });
    }
    private void OnEnable()
    {
        KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnPaused += KitchenGameManager_OnGameUnPaused;
    }
    private void OnDisable()
    {
        KitchenGameManager.Instance.OnGamePaused -= KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnPaused -= KitchenGameManager_OnGameUnPaused;
    }
    private void KitchenGameManager_OnGameUnPaused()
    {
        Hide();
    }
    private void KitchenGameManager_OnGamePaused()
    {
        Show();
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