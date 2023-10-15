using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI countdownText;

    private Animator animator;

    bool countingDown = false;
    float countdown;
    private const string NUMBER_POPUP = "NumberPopup";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
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
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            countingDown = true;
            countdown = KitchenGameManager.Instance.CountdownToStartTimer;
            countdownText.text = countdown.ToString("0");
        }
    }
    private void Update()
    {
        if (countingDown && countdown > 0.0f)
        {
            countdown -= Time.deltaTime;
            if (countdownText.text != countdown.ToString("0"))
            {
                countdownText.text = countdown.ToString("0");
                animator.SetTrigger(NUMBER_POPUP);
                SoundManager.Instance.PlayCountdownSound();
            }
        }
        else
        {
            countdownText.text = "";
            countingDown = false;
        }
    }
}
