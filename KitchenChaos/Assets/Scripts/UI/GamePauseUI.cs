using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour // Class responsible for managing the game's pause menu UI, including resume and main menu buttons
{
    [SerializeField]
    private GameObject ui;
    [SerializeField]
    private Button resumeButton;
    [SerializeField]
    private Button mainMenuButton;
    [SerializeField]
    private Button optionsButton;

    public static event Action ShowOptionsEvent; // Event to handle the display of options menu within the pause menu

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
        OptionsUI.OnBackButtonEvent += KitchenGameManager_OnGamePaused;
        optionsButton.onClick.AddListener(ShowOptions);
    }
    private void OnDisable()
    {
        KitchenGameManager.Instance.OnGamePaused -= KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnPaused -= KitchenGameManager_OnGameUnPaused;
        OptionsUI.OnBackButtonEvent -= KitchenGameManager_OnGamePaused;
        optionsButton.onClick.RemoveListener(ShowOptions);
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
    private void ShowOptions()
    {
        if (ShowOptionsEvent != null)
        {
            ShowOptionsEvent.Invoke();
            Hide();
        }
    }
}