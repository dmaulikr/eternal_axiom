using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the Goblin enemy's AI while in the Dungeon
/// </summary>
public class GolbinDungeonAI : MonoBehaviour
{
    /// <summary>
    /// A reference to the dungeonController
    /// </summary>
    DungeonController dungeonController;


    /// <summary>
    /// Stores a reference to the DungeonController
    /// </summary>
    void Awake()
    {
        this.dungeonController = GameObject.FindObjectOfType<DungeonController>();
    } // Awake

     
    /// <summary>
    /// On collision with the player initiate the battle sequence
    /// </summary>
    /// <param name="other"></param>
	void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player") {
            this.dungeonController.BattleEncounter(this.gameObject);
            this.enabled = false;
        }        
    } // OnTriggerEnter
}
