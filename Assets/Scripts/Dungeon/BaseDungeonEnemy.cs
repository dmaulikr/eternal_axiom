using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Base class for all dungeon enemies to define common interactions
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(FieldOfView))]
public abstract class BaseDungeonEnemy : MonoBehaviour, ICollideable
{
    /// <summary>
    /// Types of enemy encounter
    /// </summary>
    public enum EncounterType
    {
        Normal,         // Enemy and player collided
        Ambushed,       // Enemy attacked the Player first
        PreEmptive,     // Player Attack the Enemy first
    } // EncounterType

    /// <summary>
    /// Stores which type of encounter the player had with this enemy
    /// </summary>
    public EncounterType encounterType;

    /// <summary>
    /// The prefab to use during a battle sequence
    /// </summary>
    public GameObject battlePrefab;

    /// <summary>
    /// A reference to the dungeonController
    /// </summary>
    DungeonController dungeonController;
    public DungeonController Controller
    {
        get
        {
            if(this.dungeonController == null) {
                this.dungeonController = FindObjectOfType<DungeonController>();
            }
            return this.dungeonController;
        }
    } // Controller

    /// <summary>
    /// Holds a reference to the player
    /// </summary>
    PlayerDungeon player;
    public PlayerDungeon Player
    {
        get
        {
            if(this.player == null) {
                this.player = FindObjectOfType<PlayerDungeon>();
            }
            return this.player;
        }
    } // Player

    /// <summary>
    /// A reference to the AnimatorController controller
    /// </summary>
    Animator animator;
    public Animator Animator
    {
        get {
            if(this.animator == null) {
                this.animator = GetComponent<Animator>();
            }
            return this.animator;
        }
    } // AnimatorController

    /// <summary>
    /// References the navigation mesh agent
    /// </summary>
    NavMeshAgent navAgent;
    protected NavMeshAgent NavAgent
    {
        get
        {
            if(this.navAgent == null) {
                this.navAgent = GetComponent<NavMeshAgent>();
            }
            return this.navAgent;
        }
    } // NavAgent

    /// <summary>
    /// A reference to the field of view component
    /// </summary>
    FieldOfView fow;
    protected FieldOfView FieldOfView
    {
        get
        {
            if(this.fow == null) {
                this.fow = GetComponent<FieldOfView>();
            }
            return this.fow;
        }
    }

    /// <summary>
    /// The states the enemy can be in
    /// </summary>
    public enum State
    {
        Wait,
        Idle,
        Patrol,
        Alert,
        Pursuit,
        Hurt,
        Attack,
        Encounter,
        Death
    } // State

    /// <summary>
    /// Holds the method to invoke based on the current state
    /// </summary>
    private MethodInfo currentStateMethod = null;

    /// <summary>
    /// Current state
    /// </summary>
    [SerializeField]
    private State state;
    public State EnemyState
    {
        get
        {
            return this.state;
        }

        set
        {
            Type type = this.GetType();

            // Locates the name of the method based on the name of the state given
            // Includes public and private methods
            MethodInfo method = type.GetMethod( 
                value.ToString(), 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            // Valid state, can change state
            if( method != null ) {
                this.state = value;
                this.currentStateMethod = method;
            }
        }
    } // EnemyState

    /// <summary>
    /// A collection of AnimatorController parameters associated with a state
    /// </summary>
    protected Dictionary<State, int> hashAnimatorParams = new Dictionary<State, int> {
        { State.Attack, Animator.StringToHash("Attack")},
        { State.Hurt, Animator.StringToHash("Hurt")},
        { State.Death, Animator.StringToHash("Death")},
    };

    /// <summary>
    /// A hash reference to the speed AnimatorController parameter
    /// </summary>
    protected readonly int hashSpeedParam = Animator.StringToHash("Speed");

    /// <summary>
    /// How many ticks this enemy has been in idle
    /// </summary>
    protected int idleCount = 0;

    /// <summary>
    /// Total time to wait while Idled in millisecons
    /// </summary>
    [SerializeField]
    protected int maxIdleTime = 250;

     /// <summary>
    /// A list of all the navigation points an enemy can move to
    /// </summary>
    [SerializeField]
    protected List<Transform> navigationPoints;

    /// <summary>
    /// The current navigation point index from <see cref="navigationPoints"/>
    /// </summary>
    protected int curNavPointIndex = 0;

    /// <summary>
    /// How close to the destination navigation point befoer we considered it as "arrived"
    /// </summary>
    [SerializeField]
    float navDestinationPad = 0.3f;

    /// <summary>
    /// How slowly or quickly to update the AnimatorController speed float parameter
    /// This is used to slowly change the mecanim rather than snapping in to
    /// </summary>
    [SerializeField]
    protected float speedDamp = 0.1f;

    /// <summary>
    /// How fast/smooth to rotate
    /// Higher values means faster less smooth turns
    /// </summary>
    [SerializeField]
    protected float rotationSpeed = 15f;

    /// <summary>
    /// The playerTransform this enemy will pursuit when in "alert/pursuit" mode
    /// </summary>
    [SerializeField]
    protected GameObject targetGO;
    
    /// <summary>
    /// Last position the playerTransform (a.k.a player) was sighted at
    /// </summary>
    private Vector3 lastKnownPlayerPosition;

    /// <summary>
    /// How fast this enemy moves while patrolling
    /// </summary>
    [SerializeField]
    protected float patrolSpeed = 1.5f;

    /// <summary>
    /// The mecanim speed that represents this enemy patrolling
    /// </summary>
    [SerializeField] 
    protected float patrolAnimationSpeed = 0.5f;

    /// <summary>
    /// How fast this enemy moves when pursuing its playerTransform
    /// </summary>
    [SerializeField]
    protected float pursuitSpeed = 2.0f;

    /// <summary>
    /// The mecanim speed that represents this enemy pursuing
    /// </summary>
    [SerializeField] 
    protected float pursuitAnimationSpeed = 1f;

    /// <summary>
    /// The current speed to set the animation mecanim to 
    /// The purpouse being to slowly transition to this speed and not snap into it
    /// </summary>
    private float currentAnimationSpeed = 0f;

    /// <summary>
    /// Field of View Color for when this enemy is not aware of the player's location
    /// </summary>
    [SerializeField]
    Color normalFOWColor = Color.blue;

    /// <summary>
    /// Field of View Color for when this enemy is on alert
    /// </summary>
    [SerializeField]
    Color alertFOWColor = Color.yellow;

    /// <summary>
    /// Field of View Color for when this enemy is actively pursuing the player
    /// </summary>
    [SerializeField]
    Color pursuitFOWColor = Color.red;

    /// <summary>
    /// Keeps tracks of how many ticks this enemy has been in alert mode
    /// </summary>    
    int alertCounter = 0;

    /// <summary>
    /// Maximum clicks this enemy will spend in alert mode
    /// </summary>
    [SerializeField]
    protected int maxAlertDelay = 125;

    /// <summary>
    /// True when this enemy's attack connected with the player's avatar
    /// </summary>
    protected bool isPlayerHurt = false;

    /// <summary>
    /// How close this enemy needs to be from the player to trigger an attack
    /// </summary>
    [SerializeField]
    protected float attackDistance = 1F;

    /// <summary>
    /// Initialize
    /// States in Idle and finds its playerTransform (a.k.a, the player)
    /// </summary>
    void Awake()
    {
        this.TransitionToState(State.Idle);
        this.targetGO = GameObject.FindObjectOfType<PlayerDungeon>().gameObject;
    }

    /// <summary>
    /// Calls the method associated with the current state
    /// </summary>
    void Update()
    {
        if(this.currentStateMethod != null) {
            this.currentStateMethod.Invoke(this, null);
        }
    }

    /// <summary>
    /// Applies physics
    /// Updates the field of view color
    /// </summary>
    void FixedUpdate()
    {
        this.Move();
        this.Rotate();
        this.UpdateFieldOfViewColor();
    }

    /// <summary>
    /// Updates this enemy's animation speed to simulate movement
    /// Speed is damped for smooth transitions
    /// </summary>
    void Move()
    {
        this.Animator.SetFloat(this.hashSpeedParam, 
                                this.currentAnimationSpeed, 
                                this.speedDamp, 
                                Time.deltaTime);
    }

    /// <summary>
    /// Rotates the gameobject to match the navagent's desired velocity
    /// </summary>
    void Rotate()
    {
        // Does not need to rotate
        if(this.NavAgent.desiredVelocity == Vector3.zero) {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(this.NavAgent.desiredVelocity);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation,
                                                  targetRotation,
                                                  this.rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Sets the color of the field of view based on the current state
    /// </summary>
    protected void UpdateFieldOfViewColor()
    {
        switch(this.state) {
            case State.Alert:
                this.FieldOfView.MeshColor = this.alertFOWColor;
                break;

            case State.Pursuit:
                this.FieldOfView.MeshColor = this.pursuitFOWColor;
                break;

            case State.Attack:
            case State.Encounter:
            case State.Death:
                // Leave it the color it currently is
                break;

            default:
                this.FieldOfView.MeshColor = this.normalFOWColor;
                break;
        }
    }

    /// <summary>
    /// Handles transitioning from state to state
    /// Sets this enemy's movement/animation speed
    /// Handles variable updates and cleanup
    /// </summary>
    /// <param name="state"></param>
    protected virtual void TransitionToState(State state)
    {
        // Ensures the FOW is enabled
        this.FieldOfView.drawField = true;

        switch(state) {
            case State.Idle:
            case State.Wait:
                this.NavAgent.Stop();
                this.NavAgent.speed = 0f;
                this.currentAnimationSpeed = 0f;
                break;

            case State.Patrol:
                this.NavAgent.Resume();
                this.NavAgent.speed = this.patrolSpeed;
                this.currentAnimationSpeed = this.patrolAnimationSpeed;
                break;

            case State.Alert:
            case State.Pursuit:
                this.NavAgent.Resume();
                this.NavAgent.speed = this.pursuitSpeed;
                this.currentAnimationSpeed = this.pursuitAnimationSpeed;
                break;

            case State.Attack:
                this.NavAgent.Stop();
                this.NavAgent.speed = 0f;
                this.NavAgent.velocity = Vector3.zero;
                this.currentAnimationSpeed = 0f;
                this.SetAnimatorTriggerByState(State.Attack);
                this.FieldOfView.drawField = false;
                break;

            case State.Encounter:
            case State.Death:
                this.NavAgent.Stop();
                this.NavAgent.speed = 0f;
                this.currentAnimationSpeed = 0f;
                this.FieldOfView.drawField = false;
                break;
        } // switch

        this.EnemyState = state;
    }

    /// <summary>
    /// Triggers the AnimatorController's trigger parameter for the given state as long
    /// as the animation being played is not already the desired one to avoid 
    /// setting the trigger multiple times
    /// </summary>
    /// <param name="state"></param>
    protected void SetAnimatorTriggerByState(State state)
    {
        if( this.Animator.GetCurrentAnimatorStateInfo(0).tagHash != this.hashAnimatorParams[state] ) {
            this.Animator.SetTrigger(this.hashAnimatorParams[state]);
        }
    }

    /// <summary>
    /// Stores the player's last "seen" location and returns true
    /// if the player is still within the field of view
    /// </summary>
    /// <returns>True: player is in the FOW</returns>
    protected bool IsPlayerInSight()
    {
        bool isVisible = this.FieldOfView.visibleTargets.Count > 0;
        
        if(isVisible) {
            Transform playerTransform = this.FieldOfView.visibleTargets[0];
            this.lastKnownPlayerPosition = playerTransform.position;
        }

        return isVisible;
    }

    /// <summary>
    /// Waits in Idle for a given Time
    /// Transitions to Patrol when the time expires
    /// Transitions to Alert when the player is in sight
    /// </summary>
    protected virtual void Idle()
    {
        if(this.IsPlayerInSight()) {
            this.TransitionToState(State.Alert);
            return;
        }

        this.idleCount++;
        if(this.idleCount >= this.maxIdleTime) {
            this.idleCount = 0;
            this.TransitionToState(State.Patrol);
        }
    }

    /// <summary>
    /// Continues to moves towards the current <see cref="curNavPointIndex"/>
    /// Transitions to Idle when the destination is reached or unknown
    /// Transitions to Alert when the player is sighted
    /// </summary>
    protected virtual void Patrol()
    {
        // Navpoint is unknown, return to Idle
        if(this.navigationPoints == null || this.navigationPoints.Count < 1) {
            this.TransitionToState(State.Idle);
            Debug.Log("Enemy: " + this.name + " does not have valid navigation points");
            return;
        }

        // Keep moving towards destination
        this.NavAgent.SetDestination(this.navigationPoints[this.curNavPointIndex].position);

        // Player spotted
        if(this.IsPlayerInSight()) {
            this.TransitionToState(State.Alert);
            return;
        }
       
        bool destinationReached = Vector3.Distance(this.navigationPoints[this.curNavPointIndex].position, 
                                                   this.transform.position) 
                                                   < this.navDestinationPad;
        // Destination Reached
        if(destinationReached) {
            this.curNavPointIndex++;

            if(this.curNavPointIndex >= this.navigationPoints.Count) {
                this.curNavPointIndex = 0;
            }

            this.TransitionToState(State.Idle);
        }
    }

    /// <summary>
    /// Continues to move towards the player's last known location 
    /// Transitions to Patrol if the alert time expires
    /// Transitions to Pursuit if the player is sighted
    /// Chooses the closest navigation point when returning to patrol
    /// </summary>
    protected virtual void Alert()
    {
        if(this.IsPlayerInSight()) {
            this.alertCounter = 0;
            this.TransitionToState(State.Pursuit);
            return;
        }

        // Keep moving towards the player's last position 
        this.NavAgent.SetDestination(this.lastKnownPlayerPosition);

        // Time's not up
        this.alertCounter++;
        if(this.alertCounter < this.maxAlertDelay) {
            return;
        }

        this.alertCounter = 0;
        this.curNavPointIndex = this.GetClosestNavPointIndex();        
        this.TransitionToState(State.Patrol);
    }

    /// <summary>
    /// Continues to move towards the player's last known position while the player is in sight
    /// Transitions to Attack if the player is close enough
    /// Transitions to Alert if the player is no longer in sight
    /// </summary>
    protected virtual void Pursuit()
    {
        if(!this.IsPlayerInSight()) {
            this.TransitionToState(State.Alert);
            return;
        }

        // Continue to pursue
        this.NavAgent.SetDestination(this.lastKnownPlayerPosition);

        // Close enough to attack
        float distanceToPlayer = Vector3.Distance(this.lastKnownPlayerPosition, this.transform.position);
        if(distanceToPlayer <= this.attackDistance) {
            this.TransitionToState(State.Attack);
        }
    }

    /// <summary>
    /// Waits until the attack animation has finished
    /// Transitions to Encounter if the attack connects with the player
    /// Transitions to Pursuit once the animation is completed
    /// </summary>
    protected virtual void Attack()
    {
        // Attack connected
        if(this.isPlayerHurt) {
            this.encounterType = EncounterType.Ambushed;
            this.TransitionToState(State.Encounter);
            return;
        }
        
        // Wait for animation to complete
        if(this.Animator.GetCurrentAnimatorStateInfo(0).tagHash == this.hashAnimatorParams[this.state]) {
            return;
        }

        // Resume pursuing the player
        this.TransitionToState(State.Pursuit);
    }

    /// <summary>
    /// Notifies the dungeon <see cref="Controller"/> to initiate a battle encounter
    /// based on what <see cref="encounterType"/> is set to
    /// Transitions to Wait
    /// </summary>
    protected virtual void Encounter()
    {
        this.Controller.BattleEncounter(this, this.encounterType);
        this.TransitionToState(State.Wait);
    }

    /// <summary>
    /// Place holder while the death sequence plays
    /// </summary>
    protected virtual void Death(){}

    /// <summary>
    /// A place holder for when the unit is waiting for another sequence to complete
    /// before transitionin to another
    /// Field of view is disabled during the wait
    /// </summary>
    protected virtual void Wait()
    {
        this.FieldOfView.drawField = false;
    }

    /// <summary>
    /// Finds the closest navigation point
    /// Returns the index of said navigation point from the <see cref="this.navPoints"/> list
    /// </summary>
    /// <returns></returns>
    protected int GetClosestNavPointIndex()
    {
        Transform previousNavPoint = this.navigationPoints[0];

        foreach(Transform navPoint in this.navigationPoints) {
            float prevDistance = Vector3.Distance(previousNavPoint.position,  this.transform.position);
            float distance = Vector3.Distance(navPoint.position,  this.transform.position);

            if(distance < prevDistance) {
                previousNavPoint = navPoint;
            }
        }

        return this.navigationPoints.IndexOf(previousNavPoint);
    }

    /// <summary>
    /// Triggers the hurt animation
    /// </summary>
    public void TriggerHurt()
    {
        this.SetAnimatorTriggerByState(State.Hurt);
    }

    /// <summary>
    /// Player attacked the enemy and the attack connected
    /// </summary>
    public void PlayerAttackConnected()
    {
        this.TriggerHurt();
        this.encounterType = EncounterType.PreEmptive;
        this.TransitionToState(State.Encounter);
    }

    /// <summary>
    /// Triggered when there's a collision with the player
    /// Collision could be from the enemy's attack or the player bumping into the enemy
    /// </summary>
    public void PlayerCollision(string colliderName)
    {
        switch(colliderName) {
            // Enemy's attack connected
            case "WeaponCollider":
                this.Player.TriggerHurt();
                this.isPlayerHurt = true;
                break;
            
            // Both enemy and player collided
            // Initiate a normal battle encounter
            case "EncounterCollider":
                this.TriggerHurt();
                this.Player.TriggerHurt();
                this.encounterType = EncounterType.Normal;
                this.TransitionToState(State.Encounter);
                break;
        } // switch
    }

    /// <summary>
    /// Triggers the defeated animation 
    /// Disables colliders to prevent collisions and triggers
    /// Transitions to Death state
    /// </summary>
    public void Defeated()
    {
        this.SetAnimatorTriggerByState(State.Death);

        // Disable collisions to allow the player to go through
        GetComponent<Rigidbody>().detectCollisions = false;

        // Disable all colliders to prevent any triggers
        foreach(Collider collider in GetComponentsInChildren<Collider>()) {
            collider.enabled = false;
        }

        this.TransitionToState(State.Death);
    }

    /// <summary>
    /// Called at the end of specific animations 
    /// </summary>
    public void AnimationEnd(){}

    /// <summary>
    /// Called when the death animation is over
    /// Removes the enemy object after a few seconds
    /// </summary>
    public void DeathAnimationEnd()
    {
         Destroy(this.gameObject, 0.5f);
    }

    /// <summary>
    /// Triggered on collision enter by a trigger collider
    /// Determines how to trat the collision based on the object that collided with it
    /// </summary>
    /// <param name="colliderName"></param>
    /// <param name="other"></param>
    public void OnCollision(string colliderName, Collider other)
    {
        if(other.gameObject == this.Player.gameObject) {
            this.PlayerCollision(colliderName);
        }
    }
} // class