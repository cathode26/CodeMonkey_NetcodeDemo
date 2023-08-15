using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        stoveCounter.OnPlayerRemovedObject += StoveCounter_OnPlayerRemovedObject;
        animator.SetBool(IS_FLASHING, false);
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
            animator.SetBool(IS_FLASHING, show);
        }
        else
        {
            animator.SetBool(IS_FLASHING, false);
        }
    }
    private void StoveCounter_OnPlayerRemovedObject(object sender, System.EventArgs e)
    {
        animator.SetBool(IS_FLASHING, false);
    }
}