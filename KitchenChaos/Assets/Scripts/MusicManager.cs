using UnityEngine;

public class MusicManager : MonoBehaviour // Singleton class responsible for managing the game's music and volume
{
    public static MusicManager Instance { get; private set; } // Singleton instance of the MusicManager class
    private const string PLAYER_PREF_MUSIC_VOLUME = "MUSIC_VOLUME";
    private AudioSource audioSource;
    private int volume = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("MusicManager is a singleton");

        audioSource = GetComponent<AudioSource>();
        volume = PlayerPrefs.GetInt(PLAYER_PREF_MUSIC_VOLUME, volume);
        audioSource.volume = volume;
    }
    public void ChangeVolume() // Method to change the music volume level
    {
        volume += 1;
        if (volume > 10)
            volume = 0;
        audioSource.volume = GetVolume();

        PlayerPrefs.SetInt(PLAYER_PREF_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }
    public float GetVolume() // Method to retrieve the current music volume level
    {
        return volume / 10.0f;
    }
}
