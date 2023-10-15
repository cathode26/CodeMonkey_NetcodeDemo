using UnityEngine;
using UnityEngine.UI;

public class KeyBinding : MonoBehaviour // Class responsible for managing individual key bindings, including changing and displaying the binding
{
    public enum Binding // Enumeration for different key binding options, such as movement and interaction
    {
        MOVE_UP,
        MOVE_DOWN, 
        MOVE_LEFT, 
        MOVE_RIGHT,
        INTERACT, 
        INTERACT_ALTERNATE, 
        PAUSE
    }
    [SerializeField] 
    private Button changeBindingButton;
    [SerializeField] 
    private TMPro.TextMeshProUGUI bindingLabel;
    [SerializeField]
    private Binding binding;
    [SerializeField]
    private GameObject rebindPrompt;
    public Binding GetBinding() { return binding; }
    private void OnEnable()
    {
        changeBindingButton.onClick.AddListener(OnChangeBindingPressed);
    }
    private void OnDisable()
    {
        changeBindingButton.onClick.RemoveListener(OnChangeBindingPressed);
    }
    private void OnChangeBindingPressed()
    {
        rebindPrompt.SetActive(true); 
        GameInput.Instance.RebindBinding(this);
    }
    public void OnRebindComplete(string label)
    {
        rebindPrompt.SetActive(false);
        SetKeyBinding(label);
    }
    public void SetKeyBinding(string label)
    {
        bindingLabel.text = label;
    }
    public string GetKeyLabel()
    {
        return bindingLabel.text;
    }
}
