using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    // This script should be attached to an empty root node.
    // The actual player model/mesh should be a child of the root node.
    // This allows you to scale the model independently, while only moving and rotating the root.
    //Private, public, protected are called the accessors

    //This Singleton instance allows other classes to easily access the Player's properties and methods without having a direct reference to it.
    public static Player Instance { get; private set; }

    //Event that is triggered when the player's selected counter is changed.
    public event EventHandler<OnSelectedKitchenCounterChangedEventArgs> OnSelectedKitchenCounterChanged;
    public class OnSelectedKitchenCounterChangedEventArgs : EventArgs
    {
        public EmptyKitchenCounter selectedKitchenCounter;
    }

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
    [SerializeField]
    LayerMask countersLayerMask;

    private Vector3 lastInteractionDirection = Vector3.zero; 
    private bool isWalking = false;
    private float interactDistance = 2.0f;
    private EmptyKitchenCounter selectedCounter;

    public bool IsWalking { private set { isWalking = value; } get { return isWalking; } }
    public delegate void WalkingState(bool isWalking);
    public event WalkingState OnWalkingStateChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.Log("Error: Instance of Player is not null"); 
    }

    private void OnEnable()
    {
        gameInput.OnInteractAction += GameInputOnInteractAction;
    }
    private void OnDisable()
    {
        gameInput.OnInteractAction -= GameInputOnInteractAction;
    }
    /*  The GameInputOnInteractAction event is called from the GameInput when the user presses e
        The selectedCounter is set when the user is in front of a counter
        If both are true, then we can call the interact function
    */
    private void GameInputOnInteractAction(object sender, System.EventArgs e)
    {
        //Check for null operator ?
        selectedCounter?.Interact();
    }
    private void Update()
    {
        //Handles player movement according to user input.
        HandleMovement();
        //Handles interaction with objects in front of the player.
        HandleInteractions();
    }
    private void HandleInteractions()
    {
        //The input vector is either the direction of the movement or the direction of the last interaction if the player is not moving.
        //This is used to determine what object the player is facing and possibly interacting with.
        (bool, Vector2) movementData = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir;
        if (movementData.Item1)
            moveDir = new Vector3(movementData.Item2.x, 0.0f, movementData.Item2.y);
        else
            moveDir = lastInteractionDirection;

        //interact with the counter, we will see if we are touching it with a raycast
        //this only returns the first object it hits To get everything it hits, then use RaycastAll
        //The next option is to put objects in a particular layerMask, and then you can use the layerMask to filter the objects
        if (Physics.Raycast(transform.position, moveDir, out RaycastHit info, interactDistance))
        {
            lastInteractionDirection = moveDir;
            if (info.transform.TryGetComponent(out EmptyKitchenCounter clearCounter))
            {
                if (clearCounter != selectedCounter)
                    SetSelectedKitchenCounter(clearCounter);
            }
            else
            {
                SetSelectedKitchenCounter(null);
            }
        }
        else
        {
            SetSelectedKitchenCounter(null);
        }
    }
    private void HandleMovement()
    {
        //Controls the movement and rotation of the player according to user input.
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
                    OnWalkingStateChanged?.Invoke(true);
                IsWalking = true;
            }
        }
        else
        {
            if (IsWalking == true)
                OnWalkingStateChanged?.Invoke(false);
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

    private void SetSelectedKitchenCounter(EmptyKitchenCounter clearCounter)
    {
        //Changes the currently selected kitchen counter and triggers the OnSelectedKitchenCounterChanged event.
        selectedCounter = clearCounter;
        OnSelectedKitchenCounterChanged?.Invoke(this, new OnSelectedKitchenCounterChangedEventArgs
        {
            selectedKitchenCounter = clearCounter
        });
    }
}