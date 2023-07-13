using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI countdownText;

    bool countingDown = false;
    float countdown;
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
            countdownText.text = countdown.ToString("0");
        }
        else
        {
            countdownText.text = "";
            countingDown = false;
        }
    }

}
