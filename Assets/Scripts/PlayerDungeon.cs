using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDungeon : MonoBehaviour
{
    /// <summary>
    /// How much to slow down the movement by
    /// </summary>
    [SerializeField]
    float moveSpeed = 0.5f;

    /// <summary>
    /// How quick the object is rotated
    /// </summary>
    [SerializeField]
    float turnDamp = 15f;    

    /// <summary>
    /// The type of force to use to move the rigid body
    /// </summary>
    [SerializeField]
    ForceMode force = ForceMode.Impulse;

    /// <summary>
    /// Virtual joystick UI
    /// </summary>
    VirtualJoystick joystick;
    VirtualJoystick Joystick
    {
        get
        {
            if(this.joystick == null && GameObject.Find("VirtualJoyStick") != null) {
                this.joystick = GameObject.Find("VirtualJoyStick").GetComponent<VirtualJoystick>();
            }
            return this.joystick;
        }
    }

    Rigidbody rgdbody;
    /// <summary>
    /// Returns the rigid body component
    /// </summary>
    Rigidbody Rigidbody
    {
        get {
            if(this.rgdbody == null) {
                this.rgdbody = GetComponent<Rigidbody>();
            }
            return this.rgdbody;
        }
    } // Rigidbody

    Animator animator;
    /// <summary>
    /// Returns the animator component
    /// </summary>
    Animator Animator
    {
        get {
            if(this.animator == null) {
                this.animator = GetComponent<Animator>();
            }
            return this.animator;
        }
    } // Animator

    /// <summary>
    /// Returns the transform of the main camera
    /// </summary>
    Transform mainCam;
    Transform CameraTransform
    {
        get
        {
            if(this.mainCam == null) {
                this.mainCam = Camera.main.transform;
            }
            return this.mainCam;
        }
    } // CameraTransform

    /// <summary>
    /// Holds the position and rotation to move the player to 
    /// </summary>
    [SerializeField]
    Vector3 MovementVector = new Vector3();

    /// <summary>
    /// Remains True while the player attack animation is playing
    /// </summary>
    public bool isAttacking = false;

    /// <summary>
    /// True when the attack animation is still within "hit frames"
    /// </summary>
    public bool attackFramesActive = false;

    /// <summary>
    /// True when the player has been hurt
    /// </summary>
    bool isHurt = false;

    /// <summary>
    /// When True causes the joystick to reset and all movement to stop
    /// Note: the animations (attack or hurt) will continue to play
    /// </summary>
    bool enemyEncountered = false;
    
    /// <summary>
    /// Gets player's movement and translates them into rotation and movement
    /// </summary>
    void Update()
    {
        // Joystick may not loaded. If so, stop execution
        if(this.Joystick == null || this.isAttacking || this.isHurt || this.enemyEncountered) {
            // Stop moving
            this.Rigidbody.velocity = Vector3.zero; 
            return;
        }

        this.MovementVector = this.GetMovementVector();

        // Rotate to face the desired direction
        this.Rotate(this.MovementVector);

        // Move
        this.Move(this.MovementVector);
    } // Update

    /// <summary>
    /// Triggers the attack animation
    /// Turns on the attacking flag to prevent movement while animation plays
    /// </summary>
    public void Attack()
    {
        // Can't attack while hurt
        if(this.isHurt) {
            return;
        }

        this.isAttacking = true;
        this.Animator.SetTrigger("Attack");
    } // Attack

    /// <summary>
    /// Stops movement and resets joystick to avoid continual movement
    /// </summary>
    public void EnemyEncountered()
    {
        this.attackFramesActive = false;
        this.enemyEncountered = true;
        this.Joystick.InputVector = Vector3.zero;
        this.Animator.SetFloat("MovingSpeed", 0f);
    } // EnemyEncountered

    /// <summary>
    /// Triggers the hurt animation
    /// Turns on the isHurt flag to avoid control during this time
    /// </summary>
    public void TriggerHurt()
    {
        this.isHurt = true;
        this.Animator.SetTrigger("Hurt");
    } // TriggerHurt

    /// <summary>
    /// Returns a movement vector based on the player's input
    /// </summary>
    /// <returns>Vector3 movement direction</returns>
    Vector3 GetMovementVector()
    {
        // Save keyboard input
        Vector3 dir = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")
        );

         // Override with joystick input
        if(this.Joystick.InputVector != Vector3.zero) {
            dir = this.Joystick.InputVector;
        }

        // Normalize to prevent odd movement
        if( dir.magnitude > 1) {
            dir.Normalize();
        }

        return dir;     
    } // GetMovementVector


     /// <summary>
    /// Moves/Rotates the rigid body based on player input
    /// </summary>
    void Move(Vector3 movementInput)
    {
        // Prevents the unit from continuos movement
        this.Rigidbody.velocity = Vector3.zero;

        // Save the force
        Vector3 moveForce = this.WorldToScreenCoordinates(
            movementInput * this.moveSpeed
        );

        // Sets the animation speed for the blend tree 
        float blendSpeed = Mathf.Max(Mathf.Abs(moveForce.x), Mathf.Abs(moveForce.z));        
        this.Animator.SetFloat("MovingSpeed", blendSpeed);

        // Apply the force
        this.Rigidbody.AddForce(moveForce, this.force);
    } // Move


    /// <summary>
    /// Rotates the unit to face the direction in which they are moving
    /// Rotation is based on the camera view and not worldspace
    /// </summary>
    /// <param name="horizontal"></param>
    /// <param name="vertical"></param>
    void Rotate(Vector3 MovementVector)
    {
        // Look at the direction moving first
        Vector3 targetDirection = this.WorldToScreenCoordinates(MovementVector);

        // Vector Zero raises a warning
        if(targetDirection != Vector3.zero) {
            // Convert to a quaternion to know how much to rotate
            // Vector3.up specifies which direction is "up" so as to rotate using that axis
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            // Calculate how much to rotate to get to face the targetDirection
            Quaternion newRotation = Quaternion.Lerp(this.Rigidbody.rotation, targetRotation, this.turnDamp * Time.deltaTime);

            // Apply the rotation
            this.Rigidbody.MoveRotation(newRotation);
        } // if

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

        // Make sure we have a camera first to translate
        if(this.CameraTransform != null) {
            screenCoords = this.CameraTransform.TransformDirection(worldCoords);

            // Prevents changes to the Y as to avoid "jumps"
            screenCoords.Set(screenCoords.x, 0f, screenCoords.z);

            // Since we removed the Y axis we lost length so we need to normalize the data
            screenCoords = screenCoords.normalized * worldCoords.magnitude;
        } // if

        return screenCoords;
    } // WorldToScreenCoordinates

    /// <summary>
    /// Triggered when the attack animation is within the hit frames
    /// </summary>
    public void HitFramesStart()
    {
        this.attackFramesActive = true;
    } // AttackConnects

    /// <summary>
    /// Triggered when the attack animation is no longer within the hit frames
    /// </summary>
    public void HitFramesEnd()
    {
        this.attackFramesActive = false;
    } // HitFramesEnd

    /// <summary>
    /// Triggered by an animation event during the last few frames of the animation
    /// </summary>
    public void AnimationEnd()
    {
        if(this.isAttacking) {
            this.isAttacking = false;
        }

        if(this.isHurt) {
            this.isHurt = false;
        }
    } // AnimationEnd
} // class