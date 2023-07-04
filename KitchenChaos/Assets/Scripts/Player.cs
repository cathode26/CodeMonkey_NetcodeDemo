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
    private bool isWalking = false;
    public bool IsWalking { private set { isWalking = value; } get { return isWalking; } }
    public delegate void WalkingState(bool isWalking);
    public event WalkingState OnWalkingStateChanged;

    private void Update()
    {
        Vector2 inputVector = new Vector2(0,0);
        bool moved = false;

        // Check player input for movement and set the inputVector and moved accordingly
        if (Input.GetKey(KeyCode.W))
        {
            moved = true;
            inputVector.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moved = true; 
            inputVector.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moved = true;
            inputVector.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moved = true;
            inputVector.x += 1;
        }
        if (moved)
        {
            // Normalize the input vector to ensure consistent movement speed in all directions
            inputVector.Normalize();
            Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);

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
            if (IsWalking == false)
                OnWalkingStateChanged(true);
            IsWalking = true;

        }
        else
        {
            if (IsWalking == true)
                OnWalkingStateChanged(false);
            IsWalking = false;
        }
    }
}
