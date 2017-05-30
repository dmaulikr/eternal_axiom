using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all dungeon enemies to define common interactions
/// </summary>
public abstract class BaseDungeonEnemy : MonoBehaviour
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
    /// True when the attack animation is still within "hit frames"
    /// </summary>
    public bool attackFramesActive = false;

    /// <summary>
    /// True when the enemy lost the battle
    /// </summary>
    [SerializeField]
    bool isDead = false;
    bool dontRepeat = false;

    void Update()
    {
        if(this.isDead && ! dontRepeat) {
            dontRepeat = true;
            this.Defeated();
        }
    }


    /// <summary>
    /// Player an enemy collided with neither attacking the other first
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == this.Player.gameObject) {
            this.encounterType = EncounterType.Normal;
            this.PlayerCollision();
        }        
    } // OnTriggerEnter

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
    /// Enemy attacked the player before the player could attack or collide with it
    /// </summary>
    public void AmbushedAttack()
    {
        this.Player.TriggerHurt();
        this.Controller.BattleEncounter(this, EncounterType.Ambushed);
    } // AmbushedAttack

    /// <summary>
    /// Collision with the player triggers a hurt state for both the enemy and the player
    /// Immediatly followed by a transition into the battle sequece
    /// </summary>
    public void PlayerCollision()
    {
        // Allow the player's attack to have precedence
        if(!this.Player.isAttacking) {
            this.TriggerHurt();
            this.Player.TriggerHurt();
            this.Controller.BattleEncounter(this, EncounterType.Normal);
        // Player is in range of attack
        } else {
            this.TriggerAttack();
        }
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
    public void AnimationEnd(){ }

    /// <summary>
    /// Called when the death animation is over
    /// Removes the enemy object after a few seconds
    /// </summary>
    public void DeathAnimationEnd()
    {
         Destroy(this.gameObject, 0.5f);
    }
} // class