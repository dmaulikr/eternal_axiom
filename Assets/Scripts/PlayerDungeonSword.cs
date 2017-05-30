using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDungeonSword : MonoBehaviour
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
            enemy.PlayerAttackConnected();
        }        
    } // OnTriggerEnter
}
