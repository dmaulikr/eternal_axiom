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
    /// Current enemy the player is/was batteling
    /// </summary>
    BaseDungeonEnemy encounteredEnemy;

    /// <summary>
    /// A reference to the battle controller
    /// </summary>
    BattleController battleController;

    /// <summary>
    /// Prevents the coroutine from triggering multiple battle starts
    /// </summary>
    bool isBattleStarted = false;

    /// <summary>
    /// How long to delay the transition into battle
    /// </summary>
    [SerializeField]
    float battleStartDelay = 0.5f;


    /// <summary>
    /// Forces the screen to be black while assests are loaded
    /// Loads dungeon's configurations based on GameManager's instructions
    /// Spawns the DungeonUI Canvas and transitions in
    /// </summary>
    void Awake()
    {
        int level = GameManager.Instance.level;        
        UIController.Instance.SetUIStatus(UIDetails.Name.Dungeon, true);
        this.battleController = GameObject.FindObjectOfType<BattleController>();
    } // Awake

    /// <summary>
    /// Processes the action for the button pressed
    /// </summary>
    /// <param name="button"></param>
    public override void OnButtonPressed(UIButton button)
    {
        switch(button.buttonName) {
            case UIButton.Name.Attack:
                this.PlayerAttack();
                break;
        }
    } // OnButtonPressed

    /// <summary>
    /// Abstract override
    /// </summary>
    public override void OnButtonReleased(UIButton button){}

    /// <summary>
    /// Triggers the player's attack
    /// </summary>
    void PlayerAttack()
    {
        this.Player.Attack();   
    } // PlayerAttack

    /// <summary>
    /// Triggered on enemy-player collision
    /// Initiates the BattleSequence againts the given enemy
    /// </summary>
    /// <param name="enemy">The enemy encountered</param>
    public void BattleEncounter(BaseDungeonEnemy enemy, BaseDungeonEnemy.EncounterType type)
    {
        this.encounteredEnemy = enemy;
        this.Player.EnemyEncountered();

        // Call is delayed to allow animations to play for a bit before transitioning
        if(!this.isBattleStarted) {
            this.isBattleStarted = true;
            StartCoroutine(StartBattle(enemy.battlePrefab, type));
        }        
    } // BattleEncounter

    /// <summary>
    /// Triggers the battle sequence
    /// </summary>
    IEnumerator StartBattle(GameObject enemyPrefab, BaseDungeonEnemy.EncounterType encounterType)
    {
        yield return new WaitForSeconds(this.battleStartDelay);
        this.Player.enabled = false;
        this.battleController.Init(enemyPrefab, encounterType);
    } // StartBattle

    /// <summary>
    /// Called once the victory sequence for a battle is done to resume dungeon exploration
    /// </summary>
    public void BattleEnd()
    {
        this.isBattleStarted = false;
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.Main);

        this.encounteredEnemy.enabled = true;
        this.encounteredEnemy.Defeated();        

        this.Player.enabled = true;
        this.Player.BattleSequenceCompleted();
        UIController.Instance.SwitchToUI(UIDetails.Name.Dungeon);
    } // BattleEnd

    /// <summary>
    /// Called when the player has lost to perform a "soft reset"
    /// </summary>
    public void ResetDungeon()
    {
        this.isBattleStarted = false;
        this.Player.enabled = true;
        this.Player.BattleSequenceCompleted();
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.Main);
        UIController.Instance.SwitchToUI(UIDetails.Name.Dungeon);
    } // ResetDungeon
} // class