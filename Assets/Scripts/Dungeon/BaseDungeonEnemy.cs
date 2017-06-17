using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

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
        Normal,         // Both collided with neither attacking the other
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
    /// Holds a reference to the player script for the avatar in the dungeon
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
    /// Returns the animator component
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
    } // Animator

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
    /// True when the attack animation is still within "hit frames"
    /// </summary>
    public bool isAttacking = false;

    /// <summary>
    /// True when the enemy lost the battle
    /// </summary>
    [SerializeField]
    bool isDead = false;

    /// <summary>
    /// Don't repeat the death trigger
    /// </summary>
    bool dontRepeat = false;

    /// <summary>
    /// The states the unit can be
    /// </summary>
    public enum State
    {
        Wait,
        Idle,
        Patrol,
        Alert,
        Pursuit,
        Scout,
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
    /// Current unit state
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

            // States will typically be "private" methods but we want to add 
            // the flexibility of having public methods too
            // Finally, we must provide "Instance" to invoke private methods
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
    /// Animation tags for the matching state
    /// </summary>
    protected Dictionary<State, string> animationTags = new Dictionary<State, string> {
        { State.Idle, "Idle" },
        { State.Attack, "Attack" },
        { State.Hurt, "Hurt" },
        { State.Death, "Death" },
    };

    /// <summary>
    /// A collection of hash animator triggers based on the state
    /// </summary>
    protected Dictionary<State, int> hashAnimTriggers = new Dictionary<State, int> {
        { State.Attack, Animator.StringToHash("Attack")},
        { State.Hurt, Animator.StringToHash("Hurt")},
        { State.Death, Animator.StringToHash("Death")},
    };

    /// <summary>
    /// A hash reference to the speed animator parameter
    /// </summary>
    protected readonly int hashSpeedParam = Animator.StringToHash("Speed");

    /// <summary>
    /// How many ticks the unit has been in idle
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
    protected List<Transform> navPoints;

    /// <summary>
    /// The current navigation point being moved to
    /// </summary>
    protected int curNavIndex = 0;

    /// <summary>
    /// How far to the destination before it is considered as "arrived"
    /// </summary>
    [SerializeField]
    protected float distancePad = 0.3f;

    /// <summary>
    /// How to transition into the next speed
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
    /// Prevents multiple triggers of the coroutine to change current nav point index
    /// </summary>
    protected bool isChangeLocationTriggered = false;

    /// <summary>
    /// How long in seconds the unit waits before changing to the next nav point
    /// </summary>
    protected float patrolDelayTime = 2f;

    /// <summary>
    /// The target this unit will pursuit when in "alert" mode
    /// </summary>
    [SerializeField]
    protected GameObject targetGO;

    /// <summary>
    /// A list of directions in degrees for the unit to scout
    /// flags indicate whether the unit has scouted that direction or not
    /// </summary>
    Dictionary<float, bool> scoutedDirections = new Dictionary<float, bool>() {
        {0f, false},
        {90f, false},
        {180f, false},
        {360f, false},
    };

    /// <summary>
    /// Increased on each tick the unit is scouting a specific location
    /// </summary>
    int scoutCount = 0;

    /// <summary>
    /// Total time the unit faces a scouted direction in millisecons
    /// </summary>
    int maxScoutTime = 2000;
    private bool isScoutingADirection;
    private Vector3 lastPlayerPosition;

    /// <summary>
    /// How fast the unit moves while patrolling
    /// </summary>
    [SerializeField]
    protected float patrolSpeed = 1.5f;

    /// <summary>
    /// The mechanim speed that represents the unit patrolling
    /// </summary>
    [SerializeField] 
    protected float patrolAnimationSpeed = 0.5f;

    /// <summary>
    /// How fast the unit moves when pursuing its target
    /// </summary>
    [SerializeField]
    protected float pursuitSpeed = 2.0f;

    /// <summary>
    /// The mechanim speed that represents the unit pursuing
    /// </summary>
    [SerializeField] 
    protected float pursuitAnimationSpeed = 1f;

    /// <summary>
    /// The current speed to set the animation mechanim to 
    /// The purpouse being to slowly transition to this speed and not snap into it
    /// </summary>
    private float currentAnimationSpeed = 0f;

    /// <summary>
    /// Field of View Color for when the unit is not aware of the player's location
    /// </summary>
    [SerializeField]
    Color normalFOWColor = Color.blue;

    /// <summary>
    /// Field of View Color for when the unit is scouting for the player
    /// </summary>
    [SerializeField]
    Color alertFOWColor = Color.yellow;

    /// <summary>
    /// Field of View Color for when the unit is actively pursuing the player
    /// </summary>
    [SerializeField]
    Color pursuitFOWColor = Color.red;

    /// <summary>
    /// Keeps tracks of how many ticks the unit has been in alert mode
    /// </summary>
    [SerializeField]
    protected int alertCounter = 0;

    /// <summary>
    /// Maximum clicks the unit will spend in alert mode
    /// </summary>
    protected int maxAlertDelay = 100;


    /// <summary>
    /// Initialize
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
        //if(this.isDead && !dontRepeat) {
        //    dontRepeat = true;
        //    this.Defeated();
        //}
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
    /// Updates the units animation speed to simulate movement
    /// </summary>
    void Move()
    {
        this.Animator.SetFloat(this.hashSpeedParam, 
                                this.currentAnimationSpeed, 
                                this.speedDamp, 
                                Time.deltaTime);
    } // Move

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
    } // Rotate

    /// <summary>
    /// Sets the color of the field of view based on the current state
    /// </summary>
    protected void UpdateFieldOfViewColor()
    {
        switch(this.state) {
            case State.Alert:
            case State.Scout:
                this.FieldOfView.MeshColor = this.alertFOWColor;
                break;
            case State.Pursuit:
                this.FieldOfView.MeshColor = this.pursuitFOWColor;
                break;
            default:
                this.FieldOfView.MeshColor = this.normalFOWColor;
                break;
        }
    }

    /// <summary>
    /// Handles transitioning from state to state
    /// Sets the unit's movement/animation speed
    /// Handles variable updates and cleanup
    /// </summary>
    /// <param name="state"></param>
    protected virtual void TransitionToState(State state)
    {
        switch(state) {
            case State.Idle:
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
        } // switch

        this.EnemyState = state;
    }

    /// <summary>
    /// Tr
    /// </summary>
    /// <param name="state"></param>
    protected void SetAnimatorTriggerByState(State state)
    {
        if( this.animator.GetCurrentAnimatorStateInfo(0).tagHash != this.hashAnimTriggers[state] ) {
            this.animator.SetTrigger(this.hashAnimTriggers[state]);
        }
    }

    /// <summary>
    /// Stores the player's last "seen" location and returns true
    /// if the player is still within in the field of view
    /// </summary>
    /// <returns></returns>
    protected bool IsPlayerInSight()
    {
        bool isVisible = this.FieldOfView.visibleTargets.Count > 0;
        
        if(isVisible) {
            Transform target = this.FieldOfView.visibleTargets[0];
            this.lastPlayerPosition = target.position;
        }

        return isVisible;
    }

    /// <summary>
    /// Handles the Idle sequence
    /// If player is spotted while in Idle mode triggers Alert
    /// When idle time is over a transition to Patrol is triggered
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
    /// Handles the Patrol sequence
    /// While the unit has not reach the current patrol point the unit moves to it
    /// When the point is reached, the navigation points moves to the next available point
    /// and a transition to Idle is triggered
    /// If the player is spotted it triggers a transition to engage the player
    /// </summary>
    protected virtual void Patrol()
    {
        // Navpoint is unknown - return to Idle
        if(this.navPoints == null || this.navPoints.Count < 1) {
            this.TransitionToState(State.Idle);
            return;
        }

        // Player spotted
        if(this.IsPlayerInSight()) {
            this.TransitionToState(State.Alert);
            return;
        }
       
        bool destinationReached = Vector3.Distance(this.navPoints[this.curNavIndex].position, 
                                                   this.transform.position) 
                                                   < this.distancePad;
        // Destination Reached
        if(destinationReached) {
            this.curNavIndex++;

            if(this.curNavIndex >= this.navPoints.Count) {
                this.curNavIndex = 0;
            }

            this.TransitionToState(State.Idle);

        // Resume/Continue moving
        } else {
            this.NavAgent.SetDestination(this.navPoints[this.curNavIndex].position);
        }
    }

    /// <summary>
    /// Handles the Alert sequence
    /// While the player is in sight but too far to pursue the enemy will go to the player's last
    /// known position. If the player is close enough a transition to pursue is triggered
    /// If player's last position is reached and player is not in sight then a transition to
    /// Scout is triggered
    /// </summary>
    protected virtual void Alert()
    {
        if(this.IsPlayerInSight()) {
            this.alertCounter = 0;
            this.TransitionToState(State.Pursuit);
            return;
        }

        // Keep moving towards the player's last position for a few seconds
        this.NavAgent.SetDestination(this.lastPlayerPosition);

        this.alertCounter++;
        if(this.alertCounter < this.maxAlertDelay) {
            return;
        }

        ////then transition to patrol if the player is not found
        ////Reach the player's last known location before returning to patrol
        //bool destinationReached = Vector3.Distance(this.navPoints[this.curNavIndex].position,
        //                                           this.transform.position)
        //                                           < 1f;

        //if(!destinationReached) {
        //    this.NavAgent.SetDestination(this.lastPlayerPosition);
        //    return;
        //}

        this.alertCounter = 0;
        // Find closest navigation point to set as the current destination
        // Change to Idle
        Transform previousNavPoint = this.navPoints[0];

        foreach(Transform navPoint in this.navPoints) {
            float prevDistance = Vector3.Distance(previousNavPoint.position,  this.transform.position);
            float distance = Vector3.Distance(navPoint.position,  this.transform.position);

            if(distance < prevDistance) {
                previousNavPoint = navPoint;
            }
        }

        this.curNavIndex = this.navPoints.IndexOf(previousNavPoint);
        this.TransitionToState(State.Patrol);       
    }

    /// <summary>
    /// Handles the Pursuit sequence
    /// While the player remains visible the enemy runs towards the player's last
    /// known visible position. 
    /// If the player is within attacking range then a transition to Attack is triggered
    /// If the enemy reaches the player's last known destination and the player remains
    /// unseen for a given amount of time then a transition to Scout is triggered
    /// </summary>
    protected virtual void Pursuit()
    {
        if( this.IsPlayerInSight() ) { 
            this.NavAgent.SetDestination(this.lastPlayerPosition);
        } else {
            this.TransitionToState(State.Alert);
        }
    }

    /// <summary>
    /// Handles the Scout sequence
    /// During Scout mode, the enemy looks around to try and spot the player
    /// When the player is spotted it triggers a transition to engage the player
    /// If the player remains unspotted for a given time the enemy transitions to Patrol
    /// </summary>
    protected virtual void Scout()
    {
        if(!this.isScoutingADirection) {
            this.isScoutingADirection = true;

            bool isScoutDone = this.scoutedDirections.Values.Distinct().Count() == 1;
            
            if(isScoutDone) {
                this.TransitionToState(State.Patrol);
                return;
            } else {
                float curRotation = this.scoutedDirections.First(pair => pair.Value == false).Key;
                this.scoutedDirections[curRotation] = true;
                this.transform.rotation = Quaternion.Euler(new Vector3(0f, curRotation, 0f));
            }           
        }        

        if(this.IsPlayerInSight()) {
            this.TransitionToState(State.Alert);
            return;
        }

        this.scoutCount++;
        if(this.scoutCount >= this.maxScoutTime) {
            this.scoutCount = 0;
            this.isScoutingADirection = false;
        }
    }
    
    /// <summary>
    /// Handles the Attack sequence
    /// Triggers the attack animation (once)
    /// Rotates to face the player while the player is still visible
    /// Waits for the animation to complete 
    /// If the player is hit then a transition to Encounter is triggered
    /// If the animation is completed, the player is still seen, and the attack
    /// did not connected then a transition to Pursuit is tiggered
    /// </summary>
    protected virtual void Attack()
    {

    }

    /// <summary>
    /// Handles the Hurt sequence
    /// This occurs when the player attack connects with the enemy
    /// </summary>
    protected virtual void Hurt()
    {

    }

    /// <summary>
    /// Handles the Encounter sequence
    /// Encounters may occur when:
    ///     - Enemy attack connects with the player (ambushed)
    ///     - Player attack connects with enemy (pre-emptive)
    ///     - Collision with player (normal)
    /// </summary>
    protected virtual void Encounter()
    {

    }

    /// <summary>
    /// Handles the Death sequence
    /// Triggers the Death animation
    /// Disables physics and collider
    /// Waits for the animation to complete
    /// </summary>
    protected virtual void Death()
    {

    }

    /// <summary>
    /// Handles the Wait sequence
    /// A wait sequence is used to prevent the unit from triggering
    /// another action while waiting for a previous one to complete
    /// More or less a place holder to halt all other actions
    /// </summary>
    protected virtual void Wait(){}

    /// <summary>
    /// Triggers the enemy's hurt animation
    /// </summary>
    public void TriggerHurt()
    {
        this.Animator.SetTrigger("Hurt");
    } // TriggerHurt
    
    /// <summary>
    /// Triggers the enemy's attack animation
    /// </summary>
    public void TriggerAttack()
    {
        this.Animator.SetTrigger("Attack");
    } // TriggerHurt

    /// <summary>
    /// Player successfully connected an attack with this enemy before the enemy could
    /// and before there was a collision between the two
    /// </summary>
    public void PlayerAttackConnected()
    {
        this.TriggerHurt();
        this.Controller.BattleEncounter(this, EncounterType.PreEmptive);
    } // PlayerAttackConnected
    
    /// <summary>
    /// Enemy attacked the player before the player could attack or collide with it
    /// </summary>
    public void AmbushedAttack()
    {
        this.Player.TriggerHurt();
        this.Controller.BattleEncounter(this, EncounterType.Ambushed);
    } // AmbushedAttack

    /// <summary>
    /// Triggered by the player when it enter into collision with any of the enemy's trigger collider
    /// Based on which collider the collision occured with the proper action is apply
    /// </summary>
    public void PlayerCollision(string colliderName)
    {
        switch(colliderName) {
            // Enemy's attack connected
            case "WeaponCollider":
                this.Player.TriggerHurt();
                this.Controller.BattleEncounter(this, EncounterType.Ambushed);
                break;

            // Enemy has spotted the player and will engage
            case "EngageCollider":
                this.TriggerAttack();
                break;
        
            // Player Spotted
            case "VisionCollider":
                this.state = State.Alert;
                break;
            
            // Collision without attacking has happened
            case "EncounterCollider":
                // Player may have bumped the enemy while it was also attacking it
                if(this.isAttacking) {
                    return;
                }
                this.TriggerHurt();
                this.Player.TriggerHurt();
                this.Controller.BattleEncounter(this, EncounterType.Normal);
                break;
        } // switch
    } // PlayerCollision

    /// <summary>
    /// Plays death animation
    /// </summary>
    public void Defeated()
    {
        this.isDead = true;
        this.Animator.SetTrigger("Death");

        // Remove rigidbody to prevent further collision detection
        // and to avoid falling through the floor after disabling the colliders
        Destroy(GetComponent<Rigidbody>());

        // Disable all colliders to prevent any triggers
        foreach(Collider collider in GetComponentsInChildren<Collider>()) {
            collider.enabled = false;
        }
    } // Defeated

    /// <summary>
    /// Called at the end of specific animations 
    /// </summary>
    public void AnimationEnd()
    {
        if(this.isAttacking) {
            this.isAttacking = false;
        }        
    }

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
    } // OnCollision
} // class