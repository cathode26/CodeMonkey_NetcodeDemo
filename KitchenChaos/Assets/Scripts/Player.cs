using UnityEngine;

public class Player : MonoBehaviour
{
    // This script should be attached to an empty root node.
    // The actual player model/mesh should be a child of the root node.
    // This allows you to scale the model independently, while only moving and rotating the root.
    //Private, public, protected are called the accessors
    [SerializeField]
    private float movementSpeed = 5.0f;
    [SerializeField]
    private float rotationSpeed = 5.0f;
    [SerializeField]
    float playerRadius = 0.35f;
    [SerializeField]
    float playerHeight = 2.0f;
    [SerializeField] 
    private GameInput gameInput;

    private bool isWalking = false;
    public bool IsWalking { private set { isWalking = value; } get { return isWalking; } }
    public delegate void WalkingState(bool isWalking);
    public event WalkingState OnWalkingStateChanged;


    private void Update()
    {
        (bool, Vector2) movementData = gameInput.GetMovementVectorNormalized();
        
        if (movementData.Item1)
        {
            (bool, Vector3) TryMoveData = DetermineMovementAbilityAndDirection(movementData.Item2);
            bool canMove = TryMoveData.Item1;
            Vector3 movDir = TryMoveData.Item2;

            if (canMove)
            {
                MoveAndRotatePlayer(movDir);
                if (IsWalking == false)
                    OnWalkingStateChanged(true);
                IsWalking = true;
            }
        }
        else
        {
            if (IsWalking == true)
                OnWalkingStateChanged(false);
            IsWalking = false;
        }
    }

    /* 
     * Input: the input vector from unity's new input system
     * Function will rotate the players transform and move the players transform by the movDir
     */
    private void MoveAndRotatePlayer(Vector3 movDir)
    {
        //Rotate the forward vector, slowly by deltaTime
        //Smoothly rotate the player to face the direction of movement using Spherical Linear Interpolation (Slerp)
        transform.forward = Vector3.Slerp(transform.forward, movDir, Time.deltaTime * rotationSpeed);

        // Calculate the difference in direction between where the player is currently facing (transform.forward)
        // and the desired direction of movement (movDir)
        float angleDifference = Vector3.Angle(transform.forward, movDir);
        float normalizeDifference = angleDifference / 180.0f; // change the angle so it is between 0 and 1

        // Adjust the player's speed based on the difference in direction.
        // The larger the angleDifference, the slower the player moves.
        float adjustedSpeed = Mathf.Lerp(0.0f, movementSpeed, 1 - normalizeDifference);
        transform.position += movDir * Time.deltaTime * adjustedSpeed;
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
    private (bool, Vector3) DetermineMovementAbilityAndDirection(Vector2 inputVector)
    {
        Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = movementSpeed * Time.deltaTime;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDir, moveDistance);

        if (!canMove && movDir.x != 0.0f && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, new Vector3(movDir.x, 0, 0), moveDistance))
        {
            canMove = true;
            movDir.z = 0;
            movDir.Normalize();
        }
        else if (!canMove && movDir.z != 0.0f && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, new Vector3(0, 0, movDir.z), moveDistance))
        {
            canMove = true;
            movDir.x = 0;
            movDir.Normalize();
        }

        return (canMove, movDir);
    }
}
