using UnityEngine;

public class BaseMovementCheat : IMovement
{
    private PlayerProperties _playerProperties;
    private Transform _transform;
    public float _speedMultiplier { get; set; } = 1.0f;

    public BaseMovementCheat(PlayerProperties playerProperties, Transform transform, float speedMultiplier)
    {
        _playerProperties = playerProperties;
        _transform = transform;
        _speedMultiplier = speedMultiplier;
    }
    public MovementResult HandleMovement((bool recievedMovementInput, Vector2 dir) movementData, float clientDeltaTime)
    {
        clientDeltaTime = clientDeltaTime * _speedMultiplier;

        if (movementData.recievedMovementInput)
        {
            //The DetermineMovementAbilityAndDirection function returns the allowed direction that the player can move in
            //It may change the direction it can move in if it is blocked
            (bool canMove, Vector3 movDir) tryMoveData = DetermineMovementAbilityAndDirection(movementData.dir, clientDeltaTime);
            //We always take the uneditted direction of the input as the rotation direction because rotation is any direction is allowed
            Vector3 rotateDir = new Vector3(movementData.dir.x, 0.0f, movementData.dir.y);
            if (tryMoveData.canMove)
                RotateAndMovePlayer(rotateDir, tryMoveData.movDir, clientDeltaTime);
            else
                RotatePlayer(rotateDir, clientDeltaTime);

            return new MovementResult() { ReceivedMovementInput = true, CanMove = tryMoveData.canMove, ClientDeltaTime = clientDeltaTime, Direction = movementData.dir };
        }
        else
        {
            return new MovementResult() { ReceivedMovementInput = false };
        }
    }
    public void MovePlayer(Vector3 movDir, float clientDeltaTime)
    {
        // Calculate the difference in direction between where the player is currently facing (transform.forward)
        // and the desired direction of movement (movDir)
        float angleDifference = Vector3.Angle(_transform.forward, movDir);
        float normalizeDifference = angleDifference / 180.0f; // change the angle so it is between 0 and 1

        // Adjust the player's speed based on the difference in direction.
        // The larger the angleDifference, the slower the player moves.
        float adjustedSpeed = Mathf.Lerp(0.0f, _playerProperties.MovementSpeed, 1 - normalizeDifference);
        _transform.position += movDir * clientDeltaTime * adjustedSpeed;
    }
    public void RotateAndMovePlayer(Vector3 rotationDir, Vector3 movDir, float clientDeltaTime)
    {
        RotatePlayer(rotationDir, clientDeltaTime);
        MovePlayer(movDir, clientDeltaTime);
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

