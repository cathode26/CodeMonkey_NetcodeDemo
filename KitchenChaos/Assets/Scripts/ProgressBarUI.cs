using UnityEngine;
using UnityEngine.UI;
using static IHasProgress;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] 
    private Image barImage;
    [SerializeField]
    private GameObject progressCounterGO;
    [SerializeField]
    private GameObject progressBar;
    private IHasProgress progressCounter;

    private void Awake()
    {
        progressCounter = progressCounterGO.GetComponent<IHasProgress>();
        if (progressCounter == null)
            Debug.LogError("ProgressBarUI: progressCounter is missing from progressCounterGO");
    }
    private void OnEnable()
    {
        if (progressCounter != null)
        {
            progressCounter.OnProgressChanged += OnProgressChanged;
            progressCounter.OnStartProgress += OnStartProgress;
            progressCounter.OnEndProgress += OnEndProgress;
        }
        else
        {
            Debug.LogError("ContainerCounterVisual: Failed to add Listeners to events");
        }
    }
    private void OnDisable()
    {
        if (progressCounter != null)
        {
            progressCounter.OnProgressChanged -= OnProgressChanged;
            progressCounter.OnStartProgress -= OnStartProgress;
            progressCounter.OnEndProgress -= OnEndProgress;
        }
        else
        {
            Debug.LogError("ContainerCounterVisual: Failed to remove Listeners to events");
        }
    }
    private void OnProgressChanged(object sender, OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
    }
    private void OnStartProgress(object sender, System.EventArgs e)
    {
        EnableProgressBar();
    }
    private void OnEndProgress(object sender, System.EventArgs e)
    {
        DisableProgressBar();
    }
    private void EnableProgressBar()
    {
        progressBar.SetActive(true);
        barImage.fillAmount = 0.0f;
    }
    private void DisableProgressBar()
    {
        progressBar.SetActive(false);
    }
}
