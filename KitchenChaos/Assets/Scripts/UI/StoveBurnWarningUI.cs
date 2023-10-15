using UnityEngine;

public class StoveBurnWarningUI : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private GameObject canvas;
    private void OnEnable()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        stoveCounter.OnPlayerRemovedObject += StoveCounter_OnPlayerRemovedObject;
        Hide();
    }
    private void OnDisable()
    {
        stoveCounter.OnProgressChanged -= StoveCounter_OnProgressChanged;
        stoveCounter.OnPlayerRemovedObject -= StoveCounter_OnPlayerRemovedObject;
    }
    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        if (e.progressNormalized < 1)
        {
            float burnShowProgressAmout = 0.5f;
            bool show = stoveCounter.IsCooked() && e.progressNormalized >= burnShowProgressAmout;
            if (show)
                Show();
            else
                Hide();
        }
        else
        {
            Hide();
        }
    }
    private void StoveCounter_OnPlayerRemovedObject(object sender, System.EventArgs e)
    {
        Hide();
    }
    private void Show()
    {
        canvas.SetActive(true);
    }
    private void Hide()
    {
        canvas.SetActive(false);
    }
}
