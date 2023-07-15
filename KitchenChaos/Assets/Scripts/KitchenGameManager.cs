using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance;
    public event Action OnStateChanged;
    public event Action OnGamePaused;
    public event Action OnGameUnPaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }
    [SerializeField]
    private State state;
    private float waitingToStartTimer = 1.0f;
    private float countdownToStartTimer = 3.0f;
    private float gamePlayingTimer = 60.0f;
    private float gamePlayingTimerCur;
    private bool isPaused = false;

    public float CountdownToStartTimer { get => countdownToStartTimer; private set => countdownToStartTimer = value; }

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }
    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0.0f)
                {
                    state = State.CountdownToStart;
                    OnStateChanged?.Invoke();
                }
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0.0f)
                {
                    gamePlayingTimerCur = gamePlayingTimer;
                    state = State.GamePlaying;
                    OnStateChanged?.Invoke();
                }
                break;
            case State.GamePlaying:
                gamePlayingTimerCur -= Time.deltaTime;
                if (gamePlayingTimerCur < 0.0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke();
                }
                break;
            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }
    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }
    public float GetPlayingTimerNormalized()
    {
        return gamePlayingTimerCur / gamePlayingTimer;
    }
    private void GameInput_OnPauseAction(object sender, System.EventArgs e)
    {
        TogglePauseGame();
    }
    public void TogglePauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0.0f;
            OnGamePaused?.Invoke();
        }
        else
        {
            Time.timeScale = 1.0f;
            OnGameUnPaused?.Invoke();
        }
        isPaused = !isPaused;
    }
}
