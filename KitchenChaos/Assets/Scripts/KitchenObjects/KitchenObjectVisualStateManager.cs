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
    // Counts the number of local visibility changes to manage overrides of network visibility.
    private int visibilitySetLocally = 0;

    protected virtual void Awake()
    {
        // Initialize the mapping for each state to its visuals and deactivate all visuals initially.
        foreach (KitchenVisual visualState in visualStates)
        {
            stateToKitchenVisual[visualState.State] = visualState;
            foreach(GameObject visual in visualState.Visuals)
                visual.gameObject.SetActive(false);
        }
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
    // Handles changes in visibility from network updates.
    public void VisibilityChanged(bool visible)
    {
        if (visibilitySetLocally > 0)
        {
            // If there have been local changes, decrement the counter.
            visibilitySetLocally--;
        }
        else
        {
            // Otherwise, apply the network visibility change.
            SetVisibility(visible);
        }
    }
    // Sets the visibility locally, incrementing the local change counter.
    public void SetVisibilityLocal(bool visible)
    {
        visibilitySetLocally++;
        SetVisibility(visible);
    }
    // Applies the visibility setting to the current visual state's game objects.
    private void SetVisibility(bool visible)
    {
        if (stateToKitchenVisual.TryGetValue(currentState, out KitchenVisual kitchenVisual))
        {
            foreach (GameObject visual in kitchenVisual.Visuals)
                visual.SetActive(visible);
        }
    }
}
