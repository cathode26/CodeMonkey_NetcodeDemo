using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyBindings : MonoBehaviour // Class responsible for managing the entire set of key bindings, including initialization and updates
{
    List<KeyBinding> bindings;

    private void Awake()
    {
        bindings = GetComponentsInChildren<KeyBinding>().ToList();
        InitBindings();
    }
    // Update is called once per frame
    void InitBindings()
    {
        foreach (var binding in bindings)
        {
            binding.SetKeyBinding(GameInput.Instance.GetBindingText(binding.GetBinding()));
        }
    }
}
