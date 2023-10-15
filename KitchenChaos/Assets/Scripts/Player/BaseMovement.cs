using UnityEngine;

public class BaseMovement : IMovement
{
    private PlayerProperties _playerProperties;
    Transform _transform;
    public BaseMovement(PlayerProperties playerProperties, Transform transform)
    {
        _playerProperties = playerProperties;
        _transform = transform;
    }
    public void MoveAndRotatePlayer(Vector3 movDir, Vector3 rotationDir, float clientDeltaTime)
    {
        RotatePlayer(rotationDir, clientDeltaTime);

        // Calculate the difference in direction between where the player is currently facing (transform.forward)
        // and the desired direction of movement (movDir)
        float angleDifference = Vector3.Angle(_transform.forward, movDir);
        float normalizeDifference = angleDifference / 180.0f; // change the angle so it is between 0 and 1

        // Adjust the player's speed based on the difference in direction.
        // The larger the angleDifference, the slower the player moves.
        float adjustedSpeed = Mathf.Lerp(0.0f, _playerProperties.MovementSpeed, 1 - normalizeDifference);
        _transform.position += movDir * clientDeltaTime * adjustedSpeed;
    }
    public void RotatePlayer(Vector3 rotationDir, float clientDeltaTime)
    {
        _transform.forward = Vector3.Slerp(_transform.forward, rotationDir, clientDeltaTime * _playerProperties.RotationSpeed);
    }
    /* 
    * Input: the input vector from unity's new input system
    * Output: a tuple containing a bool indicating if the player can move, and a Vector3 that represents the validated movement in 3d
    * 
    * This function will attempt to find a valid direction that the player can travel by 
    * 1) Creating a Capsule around the player
    * 2) Checking if the capsule when moved in the direction of the input vector, collides with any game object 
    * If the player can't move in the desired direction, the function tries alternative directions by considering each axis individually.
    * It does this by modifying the 3D representation of the input vector, effectively "removing" the axis that caused the collision.
    * This way, the function validates the movement direction and also handles the conversion of the 2D input vector to a 3D vector.
    */
    public (bool canMove, Vector3 movDir) DetermineMovementAbilityAndDirection(Vector2 inputVector, float clientDeltaTime)
    {
        Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = _playerProperties.MovementSpeed * clientDeltaTime;

        bool canMove = !Physics.CapsuleCast(_transform.position, _transform.position + Vector3.up * _playerProperties.PlayerHeight, _playerProperties.PlayerRadius, movDir, moveDistance);

        if (!canMove && movDir.x != 0.0f && !Physics.CapsuleCast(_transform.position, _transform.position + Vector3.up * _playerProperties.PlayerHeight, _playerProperties.PlayerRadius, new Vector3(movDir.x, 0, 0), moveDistance))
        {
            canMove = true;
            movDir.z = 0;
            movDir.Normalize();
        }
        else if (!canMove && movDir.z != 0.0f && !Physics.CapsuleCast(_transform.position, _transform.position + Vector3.up * _playerProperties.PlayerHeight, _playerProperties.PlayerRadius, new Vector3(0, 0, movDir.z), moveDistance))
        {
            canMove = true;
            movDir.x = 0;
            movDir.Normalize();
        }

        return (canMove, movDir);
    }
}
