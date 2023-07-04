using UnityEngine;

public class GameInput : MonoBehaviour
{
    [SerializeField]
    private PlayerInputActions playerInputActions;
    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }
    public (bool, Vector2) GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        bool moved = playerInputActions.Player.Move.IsPressed();
        return (moved, inputVector);
    }
}
