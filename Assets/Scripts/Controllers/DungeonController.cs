using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Handles the loading and configurations of the current Dungeon
/// Enables/Disables the player control UI
/// Transitions in/out of battles
/// </summary>
public class DungeonController : BaseController
{
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
        UIController.Instance.SetUIStatus(UIDetails.Name.Dungeon, true);
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
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.Battle);
        UIController.Instance.SwitchToUI(UIDetails.Name.MainBattle);
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, true);
    } // BattleEncounter

    /// <summary>
    /// Processes the action for the button pressed
    /// </summary>
    /// <param name="button"></param>
    public override void OnButtonPressed(UIButton button)
    {
        Debug.Log(name + " on Pressed for " + button);
    } // OnButtonPressed

    /// <summary>
    /// Processes the action for the button released
    /// </summary>
    /// <param name="button"></param>
    public override void OnButtonReleased(UIButton button)
    {
        Debug.Log(name + " on release for " + button);
    } // OnButtonReleased
} // class