using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles detecting hit collision againts the player
/// </summary>
public class EnemyHitBox : MonoBehaviour
{
    /// <summary>
    /// A reference to the player script
    /// </summary>
    PlayerDungeon player;
    PlayerDungeon Player
    {
        get
        {
            if(this.player == null) {
                this.player = FindObjectOfType<PlayerDungeon>();
                if(this.player == null) {
                    throw new System.Exception("Could not located player dungeon script");
                }
            }
            return this.player;
        }
    } // Player

	/// <summary>
    /// On collision with the player initiate the battle sequence
    /// </summary>
    /// <param name="other"></param>
	void OnTriggerStay(Collider other)
    {
        BaseDungeonEnemy enemy  = other.GetComponent<BaseDungeonEnemy>();

        if(this.Player.attackFramesActive && enemy != null) {
            // Turn off for now since we know about it
            this.Player.attackFramesActive = false;
            enemy.TriggerHurt();
        }        
    } // OnTriggerEnter
}
