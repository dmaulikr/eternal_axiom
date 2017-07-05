using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves and Rotates the player avatar based on player input 
/// using an isometric perspective
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class IsometricPlayerController : MonoBehaviour
{
    /// <summary>
    /// Movement speed
    /// </summary>
    [SerializeField]
    float moveSpeed = 3f;

    /// <summary>
    /// Turning speed
    /// </summary>
    [SerializeField]
    float turnSpeed = 20f;

    /// <summary>
    /// How to slow down the animation transition
    /// </summary>
    [SerializeField]
    float animationDamp = .1f;

    /// <summary>
    /// The physics force to apply when moving the rigid body
    /// </summary>
    [SerializeField]
    ForceMode forceMode = ForceMode.VelocityChange;

    /// <summary>
    /// Stores the direction the player wants to move
    /// </summary>
    [SerializeField]
    Vector3 inputVector = Vector3.zero;

    /// <summary>
    /// Stores the vector3 destination the avatar is moving towards
    /// This is only updated once the player has reached the destination
    /// </summary>
    [SerializeField]
    Vector3 desiredPosition = Vector3.zero;
    
    /// <summary>
    /// Returns the rigid body component
    /// </summary>
    Rigidbody Rigidbody
    {
        get {
            if(this.rigidbody == null) {
                this.rigidbody = GetComponent<Rigidbody>();
            }
            return this.rigidbody;
        }
    } // Rigidbody
    new Rigidbody rigidbody;

    /// <summary>
    /// References the AnimatorController component
    /// </summary>
    Animator animator;
    Animator AnimatorController
    {
        get
        {
            if(this.animator == null) {
                this.animator = this.transform.GetChild(0).gameObject.GetComponent<Animator>();
            }
            return this.animator;
        }
    }

    /// <summary>
    /// A reference to the Virtual DPad 
    /// </summary>
    VirtualDPadController dpad;
    VirtualDPadController DPad
    {
        get
        {
            if(this.dpad == null) {
                this.dpad = FindObjectOfType<VirtualDPadController>();
            }
            return this.dpad;
        }
    }
    
    /// <summary>
    /// A reference to the Dungeon Controller Class
    /// </summary>
    IsometricDungeonGenerator dungeonController;
    IsometricDungeonGenerator DungeonController
    {
        get
        {
            if(this.dungeonController == null) {
                this.dungeonController = FindObjectOfType<IsometricDungeonGenerator>();
            }
            return this.dungeonController;
        }
    }

    /// <summary>
    /// How close to the desired destination before considering it as "reached"
    /// </summary>
    [SerializeField]
    float distanceToCenter = 0.1f;

    /// <summary>
    /// How close to center of the tile to allow the player to change the desired
    /// destination before actually "stopping"
    /// </summary>
    [SerializeField]
    float distanceToContinueMoving = 0.3f;

    /// <summary>
    /// Keeps track of the direction in degrees that the player is facing
    /// </summary>
    float directionInDegrees;

    /// <summary>
    /// True when this object is moving to a new tile
    /// </summary>
    bool isMoving = false;

    /// <summary>
    /// Triggered when an enemy sees the player
    /// Stops the avatar from responding to the player's input
    /// </summary>
    bool isSpotted = false;
    public bool IsSpotted
    {
        set
        {
            this.isSpotted = value;
        }
    }

    /// <summary>
    /// Player's current faith
    /// </summary>
    [SerializeField]
    int faith = 100;
    public int Faith
    {
        get
        {
            return this.faith;
        }
    }

    public int MaxFaith
    {
        get
        {
            return 100;
        }
    }


    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        this.desiredPosition = new Vector3(
            this.transform.position.x,
            0f,
            this.transform.position.z)
        ;
    }

    /// <summary>
    /// Updates the player's input
    /// </summary>
    void Update()
    {
        if(!this.isSpotted) {
            this.SavePlayerInput();
        }
    }

    /// <summary>
    /// Processes the player's input as rotation and movement
    /// </summary>
    void FixedUpdate()
    {
        this.Rotate(this.inputVector);
        this.Move(this.desiredPosition);
    }

    /// <summary>
    /// Saves a Vector3 for moving based in horizontal/vertical movement 
    /// </summary>
    void SavePlayerInput()
    {
        float h = 0f; // Horizontal
        float v = 0f; // Vertical
        float newDirection  = this.directionInDegrees;
        Vector3 newPosition = this.desiredPosition;

        // Keyboard Input
        if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");

        // Virtual D-Pad Input
        } else if(this.DPad != null) {
            h = this.DPad.Input["Horizontal"];
            v = this.DPad.Input["Vertical"];
        }

        // True when the either input is not 0
        bool inputGiven = (h != 0 || v != 0);
        
        // Horizontal takes prescedence
        if(h != 0 && v != 0) {
            v = 0f;
        }

        //// If the player was moving horizontally before 
        //// and decided to move vertically while still holding the horizontal buttons down
        //// then we will respect the new direction, and vice-versa
        
        //// Horizontal Movement
        //if(this.directionInDegrees == 90f || this.directionInDegrees == 270f) {
        //    if(v != 0 ) {
        //        h = 0;
        //    }
        //}

        //// Vertical Movement
        //if(this.directionInDegrees == 0f || this.directionInDegrees == 180f) {
        //    if(h != 0 ) {
        //        v = 0;
        //    }
        //}

        // Get the new direction the player is moving
        if(h < 0) {
            newDirection = 270f;
        } else if (h > 0) {
            newDirection = 90f;
        }

        if(v < 0) {
            newDirection = 180f;
        } else if (v > 0) {
            newDirection = 0f;
        }

        // Distance lets us know of the player is close enough to the destination to:
        // - Allow continue moving in the direction
        // - or to be considered as "arrived" and allow a change in direction
        float distance = Vector3.Distance(this.desiredPosition, this.transform.position);

        // While the player is moving the only input they are allowed to do is
        // to continue to move in the same direction
        if(this.isMoving && inputGiven && newDirection == this.directionInDegrees) {
            if(distance <= this.distanceToContinueMoving) {
                newPosition = new Vector3(
                    Mathf.Floor(this.transform.position.x) + h,
                    0f,
                    Mathf.Floor(this.transform.position.z) + v
                );
                this.inputVector = new Vector3(h, 0f, v);
            }
        }

        // If the player is not moving then they can move in the given direction
        // only after they are facing said direction
        if(!this.isMoving) {            
            // Update rotation
            if(newDirection != this.directionInDegrees) {
                this.directionInDegrees = newDirection;
                this.inputVector = new Vector3(h, 0f, v);
            // Update destination
            } else {
                newPosition = new Vector3(
                    Mathf.Floor(this.transform.position.x) + h,
                    0f,
                    Mathf.Floor(this.transform.position.z) + v
                );
                this.inputVector = new Vector3(h, 0f, v);
            }
        } // ! this.isMoving

        // Checks if the new position is available
        if(this.DungeonController.IsPositionWalkable(newPosition)) {
            this.desiredPosition = newPosition;
        }
    } // if

    /// <summary>
    /// Moves/Rotates the rigid body based on player input
    /// </summary>
    void Move(Vector3 desiredPosition)
    {
        float distance = Vector3.Distance(desiredPosition, this.transform.position);

        // Made it
        if(distance <= this.distanceToCenter) {
            this.isMoving = false;
            this.Rigidbody.velocity = Vector3.zero;
            this.transform.position = desiredPosition;
            this.AnimatorController.SetFloat("MovingSpeed", 0f, this.animationDamp, Time.fixedDeltaTime);
            return;
        }

        this.isMoving = true;
        this.AnimatorController.SetFloat("MovingSpeed", 1f, this.animationDamp, Time.fixedDeltaTime);
        Vector3 newPosition = Vector3.Lerp(this.transform.position, desiredPosition, this.moveSpeed * Time.fixedDeltaTime);
        this.Rigidbody.MovePosition(newPosition);
    } // Move


    /// <summary>
    /// Rotates the unit to face the direction in which they are moving
    /// Rotation is based on the camera view and not worldspace
    /// </summary>
    /// <param name="horizontal"></param>
    /// <param name="vertical"></param>
    void Rotate(Vector3 MovementVector)
    {
        // We get an error if this is zero
        if(MovementVector == Vector3.zero) {
            return;
        }

        // Calculate and smooth rotate to target
        Quaternion targetRotation = Quaternion.LookRotation(MovementVector, Vector3.up);
        Quaternion newRotation = Quaternion.Lerp(this.Rigidbody.rotation, 
                                                 targetRotation, 
                                                 this.turnSpeed * Time.fixedDeltaTime);

        // Apply the rotation
        this.Rigidbody.MoveRotation(newRotation);
    } // Rotate


    /// <summary>
    /// Translates the world coordinates to screen coordinates
    /// Use this to ensure the intended the directions is translated correctly
    /// </summary>
    /// <param name="worldCoords"></param>
    /// <returns></returns>
    Vector3 WorldToScreenCoordinates(Vector3 worldCoords)
    {
        Vector3 screenCoords = worldCoords;

        screenCoords = Camera.main.transform.TransformDirection(worldCoords);

        // Prevents changes to the Y as to avoid "jumps"
        screenCoords.Set(screenCoords.x, 0f, screenCoords.z);

        // Since we removed the Y axis we lost length so we need to normalize the data
        screenCoords = screenCoords.normalized * worldCoords.magnitude;

        return screenCoords;
    } // WorldToScreenCoordinates

    /// <summary>
    /// Triggered by the enemy's attack to display the Hurt animation
    /// </summary>
    public void TriggerHurt()
    {
        this.AnimatorController.SetTrigger("Hurt");
    }

    /// <summary>
    /// Triggered when certain animations complete
    /// </summary>
    public void AnimationEnd()
    {

    }
}