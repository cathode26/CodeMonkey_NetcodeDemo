using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour // Class responsible for managing the game's options UI, such as sound effects and music buttons
{
    [SerializeField] private Button soundEffectsButton; // Button to toggle sound effects
    [SerializeField] private Button musicButton; // Button to toggle music
    [SerializeField] private Button backButton; // Button to navigate back from the options menu
    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private GameObject ui;
    public static event Action OnBackButtonEvent; // Event to handle the back button event in the options menu

    public void Awake()
    {
        ShowUI();
        HideUI();
    }
    private void Start()
    {
        UpdateVisual();
    }
    private void OnEnable()
    {
        GamePauseUI.ShowOptionsEvent += ShowUI;
        soundEffectsButton.onClick.AddListener(OnSoundEffectsVolume);
        musicButton.onClick.AddListener(OnMusicVolume);
        backButton.onClick.AddListener(OnBackButton);
    }
    private void OnDisable()
    {
        GamePauseUI.ShowOptionsEvent -= ShowUI;
        soundEffectsButton.onClick.RemoveListener(OnSoundEffectsVolume);
        musicButton.onClick.RemoveListener(OnMusicVolume);
        backButton.onClick.RemoveListener(OnBackButton);
    }
    private void OnSoundEffectsVolume()
    {
        SoundManager.Instance.ChangeVolume();
        UpdateVisual();
    }
    private void OnMusicVolume()
    {
        MusicManager.Instance.ChangeVolume();
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);
    }
    private void ShowUI()
    {
        ui.SetActive(true);
    }
    private void HideUI()
    {
        ui.SetActive(false); 
    }
    private void OnBackButton()
    {
        if (OnBackButtonEvent != null)
        {
            HideUI();
            OnBackButtonEvent();
        }
    }
}
