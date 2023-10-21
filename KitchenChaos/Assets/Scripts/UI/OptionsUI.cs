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
    private float soundEffectsVolume = 0;
    private float musicVolume = 0;

    public void Awake()
    {
        ShowUI();
        HideUI();
        Signals.Get<SoundSignalList.OnSoundEffectsVolumeChangedSignal>().AddListener(OnSoundEffectsVolumeChanged);
        Signals.Get<SoundSignalList.OnMusicVolumeChangedSignal>().AddListener(OnMusicVolumeChanged);
    }
    private void OnDestroy()
    {
        Signals.Get<SoundSignalList.OnSoundEffectsVolumeChangedSignal>().RemoveListener(OnSoundEffectsVolumeChanged);
        Signals.Get<SoundSignalList.OnMusicVolumeChangedSignal>().RemoveListener(OnMusicVolumeChanged);
    }
    private void Start()
    {
        UpdateVisual();
    }
    private void OnEnable()
    {
        GamePauseUI.ShowOptionsEvent += ShowUI;
        soundEffectsButton.onClick.AddListener(OnSoundEffectsVolumeButton);
        musicButton.onClick.AddListener(OnMusicVolumeButton);
        backButton.onClick.AddListener(OnBackButton);
    }
    private void OnDisable()
    {
        GamePauseUI.ShowOptionsEvent -= ShowUI;
        soundEffectsButton.onClick.RemoveListener(OnSoundEffectsVolumeButton);
        musicButton.onClick.RemoveListener(OnMusicVolumeButton);
        backButton.onClick.RemoveListener(OnBackButton);
    }
    private void OnSoundEffectsVolumeChanged(float soundEffectsVolume)
    {
        this.soundEffectsVolume = soundEffectsVolume;
        UpdateVisual();
    }
    private void OnMusicVolumeChanged(float musicVolume)
    {
        this.musicVolume = musicVolume;
        UpdateVisual();
    }
    private void OnSoundEffectsVolumeButton()
    {
        Signals.Get<SoundSignalList.OnChangeSoundEffectVolumeSignal>().Dispatch();
    }
    private void OnMusicVolumeButton()
    {
        Signals.Get<SoundSignalList.OnChangeMusicVolumeSignal>().Dispatch();
    }
    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(soundEffectsVolume);
        musicText.text = "Music: " + Mathf.Round(musicVolume);
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
