using TMPro;
using UnityEngine;

public class KeyBindingDisplay : MonoBehaviour
{
    [SerializeField]
    private KeyBinding keyBindingReference; // Reference to the KeyBinding object
    [SerializeField]
    private TextMeshProUGUI keyBindingText;

    private void OnEnable()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        // Retrieve the binding text from the KeyBinding object and update the UI Text component
        keyBindingText.text = keyBindingReference.GetKeyLabel();
    }
}
