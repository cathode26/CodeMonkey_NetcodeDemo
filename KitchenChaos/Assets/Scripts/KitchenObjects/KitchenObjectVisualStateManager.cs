using System.Collections.Generic;
using UnityEngine;

// Represents the visual states that a kitchen object can be in.
public enum KitchenObjectState
{
    Default,
    Predicted
}
// Holds the visuals for a specific state of a kitchen object.
[System.Serializable]
public class KitchenVisual
{
    public KitchenObjectState State;
    public GameObject[] Visuals;
}
// Manages the visual state of a kitchen object, toggling between its default and predicted states.
public class KitchenObjectVisualStateManager : MonoBehaviour
{
    // Visual representations for each state. Set these up in the Unity Inspector.
    public KitchenVisual[] visualStates;

    // Tracks the current visual state of the kitchen object.
    private KitchenObjectState currentState = KitchenObjectState.Default;
    // Maps each state to its corresponding visual representation for quick access.
    private Dictionary<KitchenObjectState, KitchenVisual> stateToKitchenVisual = new Dictionary<KitchenObjectState, KitchenVisual> ();
    public bool IsVisible { get; private set; }

    protected virtual void Awake()
    {
        // Initialize the mapping for each state to its visuals and deactivate all visuals initially.
        foreach (KitchenVisual visualState in visualStates)
        {
            stateToKitchenVisual[visualState.State] = visualState;
            foreach(GameObject visual in visualState.Visuals)
                visual.gameObject.SetActive(false);
        }
        IsVisible = false;
    }
    // Changes the current state of the visuals to the new state.
    private void SetState(KitchenObjectState newState)
    {
        if (stateToKitchenVisual.TryGetValue(newState, out KitchenVisual kitchenVisual))
        {
            // Deactivate current visuals and activate the new ones.
            SetVisibility(false);
            currentState = newState;
            SetVisibility(true);
        }
    }
    // Call this method to switch to the default visuals.
    public void ShowDefaultVisuals()
    {
        SetState(KitchenObjectState.Default);
    }
    // Call this method to switch to the predicted visuals.
    public void ShowPredictedVisuals()
    {
        SetState(KitchenObjectState.Predicted);
    }
    public bool ShowingPredictedVisuals()
    {
        return KitchenObjectState.Predicted == currentState;
    }
    // Sets the visibility locally, incrementing the local change counter.
    public void SetVisibility(bool visible)
    {
        IsVisible = visible;
        if (stateToKitchenVisual.TryGetValue(currentState, out KitchenVisual kitchenVisual))
        {
            foreach (GameObject visual in kitchenVisual.Visuals)
                visual.SetActive(visible);
        }
    }
}
