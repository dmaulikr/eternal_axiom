using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the actions and player interface during a battle sequence
/// </summary>
public class BattleController : BaseController
{
    /// <summary>
    /// A reference to the word bank and remaining attack UI container
    /// </summary>
    GameObject wordBank;
    GameObject WordBank
    {
        get {
            if(this.wordBank == null) {
                this.wordBank = GameObject.FindGameObjectWithTag("WordBank");
            }
            return this.wordBank;
        }
    } // WordBank

    /// <summary>
    /// A reference to the ActionButtons UI container
    /// </summary>
    GameObject actionButtons;
    GameObject ActionButtons
    {
        get {
            if(this.actionButtons == null) {
                this.actionButtons = GameObject.FindGameObjectWithTag("ActionButtons");
            }
            return this.actionButtons;
        }
    } // ActionButtons

    /// <summary>
    /// A reference to the attack counter which keeps tracks of the 
    /// total remaining attacks the player has
    /// </summary>
    AttackCounter attackCounter;
    AttackCounter Counter
    {
        get
        {
            if(this.attackCounter == null) {
                this.attackCounter = FindObjectOfType<AttackCounter>();
            }
            return this.attackCounter;
        }
    } // Counter

    /// <summary>
    /// Holds a reference to the Scripture container
    /// which has access to the verse and bible reference
    /// </summary>
    Scripture scripture;
    Scripture ScriptureContainer
    {
        get
        {
            if(this.scripture == null) {
                this.scripture = FindObjectOfType<Scripture>();
            }
            return this.scripture;
        }
    } // ScriptureContainer

    /// <summary>
    /// A reference to the Battle System UI GameObject
    /// </summary>
    GameObject battleSystemUi;
    GameObject BattleSystemUI
    {
        get
        {
            if(this.battleSystemUi == null) {
                this.battleSystemUi = GameObject.FindGameObjectWithTag("BattleSystemUI");
            }
            return this.battleSystemUi;
        }
    } // BattleSystemUI

    /// <summary>
    /// Path to where the MessageScreenPrefab exists
    /// </summary>
    [SerializeField]
    string pathToMessageScreen = "Battle/Messages/MessageScreen";

    /// <summary>
    /// Holds a reference to the message screen
    /// </summary>
    GameObject MessageScreen;

    /// <summary>
    /// Contains the words selected/added to the verse
    /// </summary>
    List<WordContainer> selectedWords = new List<WordContainer>();

    /// <summary>
    /// The current unit performing an action
    /// </summary>
    BattleUnit unit;

    /// <summary>
    /// A reference to the player unit
    /// </summary>
    BattleUnit player;

    /// <summary>
    /// Contains a reference to the player gameobject prefab
    /// </summary>
    [SerializeField]
    GameObject playerPrefab;

    /// <summary>
    /// A reference to the enemy unit
    /// </summary>
    BattleUnit enemy;

    /// <summary>
    /// Keeps track of all the attacks that successfully connected
    /// </summary>
    int attacksConnected = 0;

    /// <summary>
    /// Forces a turn skip for the enemy when False
    /// </summary>
    bool enemyCanAttack = true;

    /// <summary>
    /// Location to spawn the player on battle sequence init
    /// </summary>
    [SerializeField]
    Transform playerSpawnPoint;

    /// <summary>
    /// Location to spawn the enemy on battle sequence init
    /// </summary>
    [SerializeField]
    Transform enemySpawnPoint;

    /// <summary>
    /// Holds a refernece to the dungeon controller
    /// </summary>
    DungeonController dController;
    DungeonController dungeonController
    {
        get
        {
            if(this.dController == null) {
                this.dController = FindObjectOfType<DungeonController>();
            }
            return this.dController;
        }
    } // dungeonController


    /// <summary>
    /// Initializes the battle sequence 
    /// Spawns the player and enemy(ies)
    /// Sets the camera and UI
    /// Decideds the starting unit based on the encounter type
    /// </summary>
    /// <param name="enemyPrefab">Enemy encountered</param>
    /// <param name="encounterType">Type of encounter</param>
    public void Init(GameObject enemyPrefab, BaseDungeonEnemy.EncounterType encounterType)
    {
        UIController.Instance.DisableAll();

        // Holds the message to display victory or death
        if(this.MessageScreen == null) {
            this.MessageScreen = UIController.Instance.GetUIGameObject(UIDetails.Name.BattleMessage);
        }

        GameObject playerGO = Instantiate(this.playerPrefab, this.transform, false);
        playerGO.transform.position = this.playerSpawnPoint.position;
        this.player = playerGO.GetComponent<BattleUnit>();

        GameObject enemyGO = Instantiate(enemyPrefab, this.transform, false);
        enemyGO.transform.position = this.enemySpawnPoint.position;
        this.enemy = enemyGO.GetComponent<BattleUnit>();        
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.Battle);
        
        switch(encounterType) {
            // First turn is the enemy only
            case BaseDungeonEnemy.EncounterType.Ambushed:
                this.InitEnemyTurn();
                break;
            
            // First turn is the player only
            case BaseDungeonEnemy.EncounterType.PreEmptive:
                this.InitPlayerTurn();
                this.enemyCanAttack = false;
                break;
            
            // Random chance of enemy attacking first 
            // When player goes first, enemy still has a turn
            default:
                if(Random.Range(0, 7) == 1) {
                    this.InitEnemyTurn();
                } else {
                    this.InitPlayerTurn();
                }
                break;
        } // switch
        
    } // Init


    /// <summary>
    /// Sets the ui and camera for the player to initiate their turn
    /// </summary>
    void InitPlayerTurn()
    {
        this.unit = this.player;
        this.EnableBattleCamera();
        UIController.Instance.DisableAll();
        UIController.Instance.SwitchToUI(UIDetails.Name.MainBattle);
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, true);
    } // InitPlayerTurn

    /// <summary>
    /// Disables the UI 
    /// Triggers the enemy to attack if it can
    /// Sets the can attack boolean back to true when it cannot
    /// </summary>
    void InitEnemyTurn()
    {
        this.unit = this.enemy;

        if(!this.enemyCanAttack) {
            this.enemyCanAttack = true;
            this.EndAttackPhase();
            return;
        }

        UIController.Instance.DisableAll();
        this.EnableEnemyAttackCamera();
        this.EnemyAttack();
    } // InitEnemyTurn


    /// <summary>
    /// Enemy attacks the player
    /// </summary>
    void EnemyAttack()
    {
        // Skip enemy turn
        if(!this.enemyCanAttack) {
            this.enemyCanAttack = true;
            this.InitPlayerTurn();
            return;
        }

        this.EnableEnemyAttackCamera();
        this.attacksConnected = 1;
        this.enemy.Attack(this.player);
    } // EnemyAttack

    /// <summary>
    /// Disables the UI
    /// Changes the camera to prayer mode
    /// Triggers the healing sequence
    /// </summary>
    public void Pray()
    {
        UIController.Instance.DisableAll();
        this.EnablePrayerCamera();
        this.player.TriggerHealing();
    } // Pray

    /// <summary>
    /// Enables the attack menu
    /// Ensures all active buttons are marked as "unselected" by clearing the selected words
    /// </summary>
	public void OpenAttackMenu()
    {
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, false);
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerAttack, true);

        this.Counter.ResetAttacks();
        this.DeselectAllWords();

        // Ensure all words are visually shown as deselected
        foreach(WordContainer word in FindObjectsOfType<WordContainer>()) {
            if(word.GetComponent<Button>().interactable) {
                word.GetComponent<Button>().image.color = new Color(1, 1, 1, 1);
            }
        }
    } // OpenAttackMenu

    /// <summary>
    /// Forces a deselection on all currently selected words
    /// Empties the selected words list
    /// </summary>
    void DeselectAllWords()
    {
        foreach(WordContainer word in this.selectedWords) {
            this.WordDeselected(word);
        }

        this.selectedWords.Clear();
    } // DeselectAllWords

    /// <summary>
    /// Clears any player actions and resets to main battle ui
    /// </summary>
    void CloseAttackMenu()
    {
        this.Counter.ResetAttacks();
        this.DeselectAllWords();
        this.ScriptureContainer.ResetVerse();
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerAttack, false);
        this.InitPlayerTurn();
    } // CloseAttackMenu


    /// <summary>
    /// Enables the Battle Camera
    /// Disables the Attack Camera
    /// </summary>
    void EnableBattleCamera()
    {
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.Battle);
    } // EnableBattleCamera


    /// <summary>
    /// Enables the prayer camera
    /// </summary>
    void EnablePrayerCamera()
    {
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.Prayer);
    } // EnablePrayerCamera


    /// <summary>
    /// Enables the Attack Camera
    /// </summary>
    void EnablePlayerAttackCamera()
    {
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.PlayerAttack);
    } // EnablePlayerAttackCamera


    /// <summary>
    /// Enables the Battle Camera
    /// </summary>
    void EnableEnemyAttackCamera()
    {
        CameraController.Instance.SwitchToCamera(CameraDetails.Name.EnemyAttack);
    } // EnablePlayerBattleCamera


    /// <summary>
    /// Handles add/removing words from the verse
    /// </summary>
    public void OnWordContainerClick(WordContainer word)
    {
        // Remove it
        if( this.selectedWords.Contains(word)) {
            this.WordDeselected(word);
        // Add it
        } else {
            this.WordSelected(word);
        }
    } // OnWordContainerClick


    /// <summary>
    /// Word being added to the verse
    /// The word will be added to the end of the list as long as
    /// there are no other open spots before then. These "opened spots"
    /// are marked by a NULL value which means another word used to be there
    /// Triggers the check for the player attack phase
    /// </summary>
    /// <param name="word"></param>
    void WordSelected(WordContainer word)
    {
        // No more remaining attacks
        if(this.Counter.Current == 0) {
            return;
        }
        
        // Find a vacant spot first before trying to add to the list
        int index = this.selectedWords.IndexOf(null);
        
        if(index > -1) {            
            this.selectedWords[index] = word;
        } else {
            this.selectedWords.Add(word);
            index = this.selectedWords.IndexOf(word);
        }
        
        // Shows the word is selected
        word.GetComponent<Button>().image.color = new Color(1, 0.92f, 0.016f, 1);

        this.Counter.DecreaseRemainingAttacks();
        this.ScriptureContainer.SetWordAtPosition(index, word.WordText.text);
        this.CheckForAutoPlayerAttackStart();
    } // WordSelected


    /// <summary>
    /// Word being removed from the verse
    /// Re-calculated remaining words indexes
    /// </summary>
    /// <param name="word"></param>
    void WordDeselected(WordContainer word)
    {
        if( word == null || ! this.selectedWords.Contains(word) ) {
            return;
        }

        int index = this.selectedWords.IndexOf(word);

        // Setting the word to null, allows another to occupy its place
        this.selectedWords[ this.selectedWords.IndexOf(word) ] = null;
        this.ScriptureContainer.RemoveWordAtPosition(index);
        this.Counter.IncreaseRemainingAttacks();

        // A non-interactable words is a word marked as "correct"
        // thefore we do not mark it as deselected
        if(word.GetComponent<Button>().interactable) {
            word.GetComponent<Button>().image.color = new Color(1, 1, 1, 1);
        }
    } // WordDeselected   


    /// <summary>
    /// Checks if player meets criteria to auto start attack sequence
    /// </summary>
    public void CheckForAutoPlayerAttackStart()
    {
        if(this.ScriptureContainer.IsVerseFull || this.Counter.noRemainingAttacks) {
            this.PlayerAttackPhase();
        }
    } // CheckForAutoPlayerAttackStart


    /// <summary>
    /// As long as the player has selected at least one word then the attack sequence is launched
    /// </summary>
    void PlayerAttackPhase()
    {
        // No words selected - cannot attack
        if(this.Counter.Current == this.Counter.Max) {
            return;
        }      

         UIController.Instance.DisableAll();
         this.Counter.ResetAttacks();
         this.ValidatePlayerAttack();
    } // PlayerAttackPhase


    /// <summary>
    /// Called at the beginning of each attack to see if the 
    /// chosen word is valid and triggger an attack or stop
    /// the attack phase when it is not
    /// 
    /// We use <see cref="selectedWords"/> count to determine how many words are left
    /// for validation. Each time removing one thus preventing us from validating a "blank"
    /// </summary>
    void ValidatePlayerAttack()
    {
        // No more attacks left
        if(this.selectedWords.Count < 1) {
            this.EndAttackPhase();
            return;
        }

        // Stores a reference so that we can remove the word from the list
        // but still interact with the word
        WordContainer word = this.selectedWords[0];
        this.selectedWords.RemoveAt(0);        

        // Correct Word
        if(this.ScriptureContainer.IsNextCorrectWord()) {
            this.attacksConnected++;
            this.EnablePlayerAttackCamera();
            this.player.Attack(this.enemy);
            word.GetComponent<Button>().interactable = false;

        // Incorrect Word - Trigger enemy's turn
        } else {
            this.EndAttackPhase();
            //// Stop the player from attacking
            //this.player.EndAttack();
            
            //// Player gots some hits in before selecting a wrong word
            //if(this.attacksConnected > 0) {
            //    this.player.InflictDamage(this.attacksConnected);
            //}
            
            //this.InitEnemyTurn();
        }
    } // ValidatePlayerAttack

    /// <summary>
    /// Ends the attack phase by calculating total damage
    /// Inflicting the damage
    /// Resetting the camera
    /// and Re-enabling the UI
    /// The verse is reset but keep in mind only the remaining indexes are updated to blanks
    /// </summary>
    void EndAttackPhase()
    {
        this.EnableBattleCamera();

        // Resets unit's attack and returns to idle animation
        this.unit.EndAttack();

        // Apply the total damage 
        if(this.attacksConnected > 0) {
            this.unit.InflictDamage(this.attacksConnected);
            this.attacksConnected = 0;
        }

        if(this.Counter != null) {
            this.Counter.ResetAttacks();
        }

        if( this.ScriptureContainer != null ) {
            this.ScriptureContainer.ResetVerse();
        }
        
        this.NextRound();       
    } // EndAttackPhase

    /// <summary>
    /// Checks for GameOver/Victory
    /// Triggers the init phase of the opposing unit of the current unit
    /// </summary>
    void NextRound()
    {
        // Stop here if either one is dead
        // When the death animation is completed, the victory/game over is auto triggered
        if(this.player.IsDead() || this.enemy.IsDead() ) {
            return;
        }

        // Player is done attacking, let's allow the enemey to attack
        if(this.unit == this.player) {
            this.InitEnemyTurn();
        } else {
            this.InitPlayerTurn();
        }
    } // NextRound


    /// <summary>
    /// Returns the target for the current unit
    /// </summary>
    /// <param name="currentUnit">The current unit performing an action</param>
    /// <returns></returns>
    BattleUnit GetTargetUnit(BattleUnit currentUnit)
    {
        BattleUnit target;

        if(currentUnit == this.player) {
            target = this.enemy;
        } else {
            target = this.player;
        }

        return target;
    } // GetTargetUnit


    /// <summary>
    /// Player is done healing, now the enemy gets a chance to attack
    /// </summary>
    public void EndPrayPhase()
    {
        this.InitEnemyTurn();
    } // EndPrayPhase


    /// <summary>
    /// Called by the player avatar when the attack animation is done
    /// If there are additional attacks, it triggers another attack
    /// Otherwise, resets attacks and sets animation back to idle
    /// </summary>
    public void AttackAnimationEnd(BattleUnit unit)
    {
        if(this.unit == this.player) {
            this.ValidatePlayerAttack();
        } else {
            this.EndAttackPhase();
        }
    } // AttackAnimationEnd

    /// <summary>
    /// Called when a unit has died to determine game over or game won
    /// </summary>
    /// <param name="unit"></param>
    internal void UnitDied(BattleUnit unit)
    {
        if(unit == this.player) {
            this.Defeated();
        } else {
            this.Victory();
        }        
    } // UnitDied

    /// <summary>
    ///  Handles player defeated
    /// </summary>
    void Defeated()
    {
        string message = "You've succumbed to sin...";
        this.MessageScreen.SetActive(true);
        this.MessageScreen.transform.FindChild("Message").GetComponent<Text>().text = message;
    } // Defeated
    
    /// <summary>
    /// Handles player vitory
    /// </summary>
    void Victory()
    {
        UIController.Instance.DisableAll();
        Destroy(this.player.gameObject);
        Destroy(this.enemy.gameObject);
        this.dungeonController.BattleEnd();
    } // Victory

    /// <summary>
    /// Reloads the level
    /// </summary>
    public void Reload()
    {
        UIController.Instance.DisableAll();
        this.dungeonController.ResetDungeon();
    } // Reload

    /// <summary>
    /// Abstract override
    /// </summary>
    /// <param name="button"></param>
    public override void OnButtonPressed(UIButton button){}

    /// <summary>
    /// Processes the action for the button released
    /// </summary>
    /// <param name="button"></param>
    public override void OnButtonReleased(UIButton button)
    {
        switch(button.buttonName) {
            case UIButton.Name.Pray:
                this.Pray();
                break;
            case UIButton.Name.Attack:
                this.OpenAttackMenu();
                break;
            case UIButton.Name.Word:
                // Ignore empty words
                WordContainer word = button.GetComponent<WordContainer>();
                if( ! string.IsNullOrEmpty(word.WordText.text) ) {
                    this.OnWordContainerClick(button.GetComponent<WordContainer>());
                }                
                break;
            case UIButton.Name.Commit:
                this.PlayerAttackPhase();
                break;
            case UIButton.Name.Cancel:
                this.CloseAttackMenu();
                break;
        } // switch
    } // OnButtonReleased
} // class