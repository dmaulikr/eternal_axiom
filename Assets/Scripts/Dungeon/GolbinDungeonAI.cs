using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the Goblin enemy's AI while in the Dungeon
/// </summary>
public class GolbinDungeonAI : BaseDungeonEnemy
{
    /// <summary>
    /// Finds the target to follow when in Alert state
    /// </summary>
    void Start()
    {
        this.targetGO = GameObject.FindObjectOfType<PlayerDungeon>().gameObject;
    } // Start

    /// <summary>
    /// Updates the navigation agent to move to the next nav point when they
    /// are close enough to the current destination
    /// </summary>
    void Update()
    {
        if(this.navPoints == null || this.navPoints.Count < 1) {
            return;
        }

        bool isCloseEnough = Vector3.Distance(this.navPoints[this.curNavIndex].position, 
                                              this.transform.position) 
                                              < this.distancePad;

        // Keep moving to the current nav point
        if(!isCloseEnough) {
            this.Move();
            this.Rotate();

        // Stop moving and trigger a transition to next nav point
        } else {
            if(this.state == State.Patrol) {
                this.NavAgent.velocity = Vector3.zero;
                this.Animator.SetFloat("Speed", 0f);
            
                // Call is delayed to allow animations to play for a bit before transitioning
                if(!this.isChangeLocationTriggered) {
                    this.isChangeLocationTriggered = true;
                    StartCoroutine(ChangeLocation());
                }
            }
        }
    } // Update

    /// <summary>
    /// Triggers the navmesh to move to a destination
    /// Updates the animator speed to animate the movement based on the speed of the nav agent
    /// </summary>
    void Move()
    {
        float speed = 0f;

        switch( this.state ) {
            case State.Idle:
                this.NavAgent.velocity = Vector3.zero;
                this.Animator.SetFloat("Speed", 0f);
                break;

            case State.Patrol:
                this.NavAgent.speed = 1.5f;
                this.NavAgent.SetDestination(this.navPoints[this.curNavIndex].position);
                speed = this.NavAgent.speed * this.speedDamp;
                break;

            case State.Alert:
                this.NavAgent.speed = 3f;
                this.NavAgent.SetDestination(this.targetGO.transform.position);
                speed = this.NavAgent.speed * this.speedDamp;
                break;
        }
        
        this.Animator.SetFloat("Speed", speed);
    } // Move

    /// <summary>
    /// Rotates the gameobject to match the navagent's desired velocity
    /// </summary>
    void Rotate()
    {
        if(this.NavAgent.desiredVelocity == Vector3.zero) {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(this.NavAgent.desiredVelocity);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, 
                                                  targetRotation, 
                                                  this.rotationSpeed * Time.deltaTime);
    } // Rotate

    /// <summary>
    /// Moves the nav point index to the next available index
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeLocation()
    {
        yield return new WaitForSeconds(this.patrolDelayTime);
        this.curNavIndex++;
        if(this.curNavIndex >= this.navPoints.Count) {
            this.curNavIndex = 0;
        }
        this.isChangeLocationTriggered = false;
    } // ChangeLocation
} 