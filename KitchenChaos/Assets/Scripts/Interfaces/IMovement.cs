using UnityEngine;

public interface IMovement
{
    public void MoveAndRotatePlayer(Vector3 movDir, Vector3 rotationDir, float clientDeltaTime);
    public void RotatePlayer(Vector3 rotationDir, float clientDeltaTime);
    public (bool canMove, Vector3 movDir) DetermineMovementAbilityAndDirection(Vector2 inputVector, float clientDeltaTime);
}
