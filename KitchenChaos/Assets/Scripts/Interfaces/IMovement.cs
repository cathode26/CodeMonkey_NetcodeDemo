using UnityEngine;

public struct MovementResult
{
    public bool ReceivedMovementInput { get; set; }
    public bool CanMove { get; set; }
    public float ClientDeltaTime { get; set; }
    public Vector2 Direction { get; set; }
}
public interface IMovement
{
    public MovementResult HandleMovement((bool recievedMovementInput, Vector2 dir) movementData, float clientDeltaTime);
    public void RotateAndMovePlayer(Vector3 movDir, Vector3 rotationDir, float clientDeltaTime);
    public void RotatePlayer(Vector3 rotationDir, float clientDeltaTime);
    public (bool canMove, Vector3 movDir) DetermineMovementAbilityAndDirection(Vector2 inputVector, float clientDeltaTime);
    public void MovePlayer(Vector3 movDir, float clientDeltaTime);
}
