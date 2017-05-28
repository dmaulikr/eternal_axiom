using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the loading and configurations of the current Dungeon
/// Enables/Disables the player control UI
/// Transitions in/out of battles
/// </summary>
public class DungeonController : MonoBehaviour
{
    /// <summary>
    /// The canvas containing the player controls available while exploring the dungeon
    /// </summary>
    [SerializeField]
    GameObject DungeonCanvasGO;

    /// <summary>
    /// A reference to the player game object
    /// </summary>
    GameObject PlayerGO;


    /// <summary>
    /// Forces the screen to be black while assests are loaded
    /// Loads dungeon's configurations based on GameManager's instructions
    /// Spawns the DungeonUI Canvas and transitions in
    /// </summary>
	void Awake()
    {
        int level = GameManager.Instance.level;

        if (this.DungeonCanvasGO == null) {
            throw new System.Exception("DungeonCanvas GameObject not provided");
        }

        // Save the reference to the newly spawned instance and ensure it is active
        this.DungeonCanvasGO = Instantiate(this.DungeonCanvasGO);
        this.DungeonCanvasGO.name = "DungeonCanvas";
        this.DungeonCanvasGO.SetActive(true);

        this.PlayerGO = GameObject.FindGameObjectWithTag("Player");
    } // Awake


    /// <summary>
    /// Triggered on enemy-player collision
    /// Initiates the BattleSequence againts the given enemy
    /// </summary>
    /// <param name="enemyBattlePrefab">The enemy's battle sequence prefab</param>
    public void BattleEncounter(GameObject enemyBattlePrefab)
    {
        this.PlayerGO.GetComponent<PlayerDungeon>().enabled = false;
        this.DungeonCanvasGO.SetActive(false);
    } // BattleEncounter
}
