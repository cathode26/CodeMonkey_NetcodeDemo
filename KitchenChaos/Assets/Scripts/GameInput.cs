using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour // Singleton class responsible for managing game input, including key bindings and player interactions
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";
    public static GameInput Instance { get; private set; } // Singleton instance of the GameInput class
    [SerializeField]
    private PlayerInputActions playerInputActions;
    //EventHandler is the built in C# standard for delegate
    //Event that is triggered when the player performs the interaction action (presses "E").
    public event EventHandler OnInteractAction;
    //Event that is triggered when the player performs the interaction action (presses "F").
    public event EventHandler OnInteractAlternativeAction;
    public event EventHandler OnPauseAction;

    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));

        playerInputActions.Player.Enable();

        //Here, the input system is initialized and the Interact action is set up to trigger the OnInteractAction event when performed.
        playerInputActions.Player.Interact.performed += InteractPerformed;

        //Interact Alternative action is set up to trigger the OnInteractAction event when performed.
        playerInputActions.Player.InteractAlternate.performed += InteractAlternativePerformed;

        playerInputActions.Player.Pause.performed += Pause_performed;

       
    }
    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= InteractPerformed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternativePerformed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        playerInputActions.Dispose();
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
    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty); 
    }
    public string GetBindingText(KeyBinding.Binding binding)
    {
        switch (binding)
        {
            default:
            case KeyBinding.Binding.MOVE_UP:
                return playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case KeyBinding.Binding.MOVE_DOWN:
                return playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case KeyBinding.Binding.MOVE_LEFT:
                return playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case KeyBinding.Binding.MOVE_RIGHT:
                return playerInputActions.Player.Move.bindings[4].ToDisplayString();
            case KeyBinding.Binding.INTERACT:
                return playerInputActions.Player.Interact.bindings[0].ToDisplayString();
            case KeyBinding.Binding.INTERACT_ALTERNATE:
                return playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
            case KeyBinding.Binding.PAUSE:
                return playerInputActions.Player.Pause.bindings[0].ToDisplayString();
        }
    }
    public void RebindBinding(KeyBinding keyBinding)
    {
        playerInputActions.Player.Disable();
        InputActionRebindingExtensions.RebindingOperation rebindingOperation = null;

        switch (keyBinding.GetBinding())
        {
            case KeyBinding.Binding.MOVE_UP:
                rebindingOperation = playerInputActions.Player.Move.PerformInteractiveRebinding(1);
                break;
            case KeyBinding.Binding.MOVE_DOWN:
                rebindingOperation = playerInputActions.Player.Move.PerformInteractiveRebinding(2);
                break;
            case KeyBinding.Binding.MOVE_LEFT:
                rebindingOperation = playerInputActions.Player.Move.PerformInteractiveRebinding(3);
                break;
            case KeyBinding.Binding.MOVE_RIGHT:
                rebindingOperation = playerInputActions.Player.Move.PerformInteractiveRebinding(4);
                break;
            case KeyBinding.Binding.INTERACT:
                rebindingOperation = playerInputActions.Player.Interact.PerformInteractiveRebinding(0);
                break;
            case KeyBinding.Binding.INTERACT_ALTERNATE:
                rebindingOperation = playerInputActions.Player.InteractAlternate.PerformInteractiveRebinding(0);
                break;
            case KeyBinding.Binding.PAUSE:
                rebindingOperation = playerInputActions.Player.Pause.PerformInteractiveRebinding(0);
                break;
        }
        
        rebindingOperation?.OnComplete(callback =>
        {
            playerInputActions.Player.Enable();
            keyBinding.OnRebindComplete(GetBindingText(keyBinding.GetBinding()));
            PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
            callback.Dispose();
        }).Start();
    }
}
