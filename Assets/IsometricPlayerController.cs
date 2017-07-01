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
    public float moveSpeed = 3f;

    /// <summary>
    /// Turning speed
    /// </summary>
    public float turnSpeed = 20f;

    /// <summary>
    /// The physics force to apply when moving the rigid body
    /// </summary>
    [SerializeField]
    private ForceMode forceMode = ForceMode.VelocityChange;

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

    Animator animator;
    Animator Animator
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
    float distancePadding = 0.3f;

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
        this.SavePlayerInput();
        this.DungeonController.ShowHideWalls(this.desiredPosition);
    }

    /// <summary>
    /// Processes the player's input as rotation and movement
    /// </summary>
    void FixedUpdate()
    {
        this.Rotate(this.inputVector);
        this.Move(this.inputVector);
    }

    /// <summary>
    /// Saves a Vector3 for moving based in horizontal/vertical movement 
    /// </summary>
    void SavePlayerInput()
    {
        // Player is still moving, ignore input
        if(this.transform.position != this.desiredPosition) {
            return;
        }
        
        float h = 0f; // Horizontal
        float v = 0f; // Vertical

        // Keyboard Input
        if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            h = -Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        // Virtual D-Pad Input
        } else if(this.DPad != null) {
            h = -this.DPad.Input["Horizontal"];
            v = this.DPad.Input["Vertical"];
        } 

        // Horizontal takes prescedence
        if(h != 0 && v != 0) {
            v = 0f;
        }

        // Saves the direction the player input
        // Due to the perspective of the camera, the horizontal axis is inverted
        this.inputVector = new Vector3(v, 0f, h);
                
        // New tile to move to
        Vector3 newPosition = new Vector3(
            Mathf.Floor(this.transform.position.x) + v,
            0f,
            Mathf.Floor(this.transform.position.z) + h
        );

        if(this.DungeonController.IsPositionWalkable(newPosition)) {
            this.desiredPosition = newPosition;
        }
    }

     /// <summary>
    /// Moves/Rotates the rigid body based on player input
    /// </summary>
    void Move(Vector3 movementInput)
    {
        float distance = Vector3.Distance(this.desiredPosition, this.transform.position);

        // Made it
        if(distance <= this.distancePadding) {
            this.Rigidbody.velocity = Vector3.zero;
            this.transform.position = this.desiredPosition;
            this.Animator.SetFloat("Speed", 0f);
            return;
        }

        this.Animator.SetFloat("Speed", 1f);
        Vector3 newPosition = Vector3.Lerp(this.transform.position, this.desiredPosition, this.moveSpeed * Time.fixedDeltaTime);
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
        if(this.inputVector == Vector3.zero) {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(MovementVector, Vector3.up);

        // Calculate how much to rotate to get to face the targetDirection
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
}