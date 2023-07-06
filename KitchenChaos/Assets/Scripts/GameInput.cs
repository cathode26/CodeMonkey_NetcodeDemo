using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    [SerializeField]
    private PlayerInputActions playerInputActions;
    //EventHandler is the built in C# standard for delegate
    //Event that is triggered when the player performs the interaction action (presses "E").
    public event EventHandler OnInteractAction;
    //Event that is triggered when the player performs the interaction action (presses "F").
    public event EventHandler OnInteractAlternativeAction;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        //Here, the input system is initialized and the Interact action is set up to trigger the OnInteractAction event when performed.
        playerInputActions.Player.Interact.performed += InteractPerformed;

        //Interact Alternative action is set up to trigger the OnInteractAction event when performed.
        playerInputActions.Player.InteractAlternate.performed += InteractAlternativePerformed;
    }
    private void InteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //? null condition operator, followed by Invoke, because you can't put the function parenthesis after the ? null condition operator
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    private void InteractAlternativePerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //? null condition operator, followed by Invoke, because you can't put the function parenthesis after the ? null condition operator
        OnInteractAlternativeAction?.Invoke(this, EventArgs.Empty);
    }
    public (bool, Vector2) GetMovementVectorNormalized()
    {
        //This method retrieves the direction of the movement from the input system and checks whether the movement action is currently being performed (i.e., the movement keys are being pressed).
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        bool moved = playerInputActions.Player.Move.IsPressed();
        return (moved, inputVector);
    }
}
