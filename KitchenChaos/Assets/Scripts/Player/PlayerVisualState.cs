using UnityEngine;

public class PlayerVisualState : MonoBehaviour
{
    private bool _isWalking = false;
    public bool IsWalking { private set { _isWalking = value; } get { return _isWalking; } }
    public delegate void WalkingState(bool isWalking);
    public event WalkingState OnWalkingStateChanged;

    public void HandleMovement(MovementResult movementResult)
    {
        if (movementResult.ReceivedMovementInput)
        {
            if (movementResult.CanMove)
            {
                if (IsWalking == false)
                    OnWalkingStateChanged?.Invoke(true);

                IsWalking = true;
            }
            else
            {
                if (IsWalking == true)
                    OnWalkingStateChanged?.Invoke(false);
                IsWalking = false;
            }
        }
        else
        {
            if (IsWalking)
                OnWalkingStateChanged?.Invoke(false);
            IsWalking = false;
        }
    }
}
