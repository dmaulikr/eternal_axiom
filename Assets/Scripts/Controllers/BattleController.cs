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
    /// A reference to the enemy unit
    /// </summary>
    BattleUnit enemy;

    /// <summary>
    /// Keeps track of all the attacks that successfully connected
    /// </summary>
    int attacksConnected = 0;

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
    /// Hides the wordbank on scene load
    /// </summary>
    public void Init()
    {
        if(this.MessageScreen == null) {

            //if(string.IsNullOrEmpty(this.pathToMessageScreen)) {
            //    throw new System.Exception("Path to message screen is null or empty");
            //}

            //// Creates the message screen that holds results of the battle
            //GameObject messageScreenGO = Utility.LoadResource<GameObject>(this.pathToMessageScreen);
            //if(messageScreenGO == null) {
            //    throw new System.Exception(string.Format("Could not find the message screen prefab in {0}", this.pathToMessageScreen));
            //}
            //UIController.Instance.AddUI(UIDetails.Name.BattleMessage, messageScreenGO);
            this.MessageScreen = UIController.Instance.GetUIGameObject(UIDetails.Name.BattleMessage);
        }

        this.player = GameObject.FindGameObjectWithTag("BattlePlayer").GetComponent<BattleUnit>();
        this.enemy = GameObject.FindGameObjectWithTag("BattleEnemy").GetComponent<BattleUnit>();

        // Default to player
        this.unit = this.player;
    } // Init


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
    /// Hides the Action Button Menu and shows the word bank
    /// </summary>
	public void OnAttackButtonClick()
    {
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, false);
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerAttack, true);
    } // OnAttackButtonClick

    /// <summary>
    /// Resest all player selection 
    /// Changes back to the main battle ui
    /// </summary>
    void CancelAttack()
    {
        // Reset selected words color unless they are disabled
        // Dump all selected words as to start from 0 again
        foreach(WordContainer word in this.selectedWords) {
            if(word.GetComponent<Button>().interactable) {
                word.GetComponent<Button>().image.color = new Color(1, 1, 1, 1);
            }
        }

        this.Counter.ResetAttacks();
        this.ClearSelectedWords();
        this.ScriptureContainer.ResetVerse();
        this.EnableBattleCamera();

        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerAttack, false);
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, true);
    } // CancelAttack


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
        
        int index = this.selectedWords.IndexOf(null);
        
        if(index > -1) {
            
            this.selectedWords[index] = word;
        } else {
            this.selectedWords.Add(word);
            index = this.selectedWords.IndexOf(word);
        }
        
        word.GetComponent<Button>().image.color = new Color(1, 0.92f, 0.016f, 1);

        this.Counter.DecreaseRemainingAttacks();
        this.ScriptureContainer.SetWordAtPosition(index, word.WordText.text);
        this.BeginUnitAttackPhase();
    } // WordSelected


    /// <summary>
    /// Word being removed from the verse
    /// Re-calculated remaining words indexes
    /// </summary>
    /// <param name="word"></param>
    void WordDeselected(WordContainer word)
    {
        if(!this.selectedWords.Contains(word)) {
            return;
        }

        int index = this.selectedWords.IndexOf(word);

        // Setting the index to null, allows another to occupy its place
        this.selectedWords[ this.selectedWords.IndexOf(word) ] = null;

        this.ScriptureContainer.RemoveWordAtPosition(index);
        this.Counter.IncreaseRemainingAttacks();

        word.GetComponent<Button>().image.color = new Color(1, 1, 1, 1);
    } // WordDeselected


    /// <summary>
    /// Forces a deselection on all currently selected words
    /// Empties the selected words list
    /// </summary>
    void ClearSelectedWords()
    {
        foreach(WordContainer word in this.selectedWords) {
            if(word == null) {
                continue;
            }
            this.WordDeselected(word);
        }

        this.selectedWords.Clear();
    } // ClearSelectedWords


    /// <summary>
    /// Disables the UI
    /// Triggers the healing sequence
    /// </summary>
    public void Pray()
    {
        UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, false);
        UIController.Instance.SetUIStatus(UIDetails.Name.MainBattle, false);
        this.EnablePrayerCamera();
        this.player.TriggerHealing();
    } // Pray


    /// <summary>
    /// Called after a word is added to see if its time to launch the player attack phase
    /// Attack phase occurs when either the player has filled all remaining blanks or has
    /// consumed all of their remaining attacks or commits to attack
    /// </summary>
    public void BeginUnitAttackPhase(bool force = false)
    {
        // No words selected
        if(force && this.Counter.Current == this.Counter.Max) {
            return;
        }

        // Switch back to player
        this.unit = this.player;

        if(force || this.ScriptureContainer.IsVerseFull || this.Counter.noRemainingAttacks) {
            UIController.Instance.DisableAll();
            this.Counter.ResetAttacks();
            this.ValidatePlayerAttack();
        }
    } // BeginUnitAttackPhase


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
        // End of attack
        if(this.selectedWords.Count < 1 || this.unit == this.enemy) {
            this.EndAttackPhase();
            return;
        }

        // References the word so that we reset its index and disable it when it is correct
        WordContainer word = this.selectedWords[0];
        this.selectedWords.RemoveAt(0);        

        if(this.ScriptureContainer.IsNextCorrectWord()) {
            this.attacksConnected++;
            word.GetComponent<Button>().interactable = false;
            this.EnablePlayerAttackCamera();
            this.unit.Attack(this.enemy);
        } else {
            // Before triggering the enemy attack, the player may have caused damage
            if(this.attacksConnected > 0) {
                this.unit.EndAttack();
                this.unit.InflictDamage(this.attacksConnected);
            }

            this.EnemyAttack();
        }        
    } // ValidatePlayerAttack


    /// <summary>
    /// Enemy attacks the player
    /// </summary>
    void EnemyAttack()
    {
        this.EnableEnemyAttackCamera();
        this.attacksConnected = 1;
        this.unit = this.enemy;
        this.unit.Attack(this.player);
    } // EnemyAttack


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
    /// Ends the attack phase by calculating total damage
    /// Inflicting the damage
    /// Resetting the camera
    /// and Re-enabling the UI
    /// The verse is reset but keep in mind only the remaining indexes are updated to blanks
    /// </summary>
    void EndAttackPhase()
    {
        // Resets unit's attack and returns to idle animation
        this.unit.EndAttack();

        // Apply the total damage 
        this.unit.InflictDamage(this.attacksConnected);
        this.attacksConnected = 0;

        // Reset selecte words color unless they are disabled
        // Dump all selected words as to start from 0 again
        foreach(WordContainer word in this.selectedWords) {
            if(word.GetComponent<Button>().interactable) {
                word.GetComponent<Button>().image.color = new Color(1, 1, 1, 1);
            }
        }

        if(this.Counter != null) {
            this.Counter.ResetAttacks();
        }

        if( this.ScriptureContainer != null ) {
            this.ScriptureContainer.ResetVerse();
        }
        
        this.ClearSelectedWords();

        // Stop here if either one is dead
        if(this.player.IsDead() || this.enemy.IsDead() ) {
            return;
        }

        // Player is done attacking, let's allow the enemey to attack
        if(this.unit == this.player) {
            this.EnemyAttack();
        } else {
            UIController.Instance.DisableAll();
            UIController.Instance.SwitchToUI(UIDetails.Name.MainBattle);
            UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, true);
            this.EnableBattleCamera();
        }
        
    } // EndAttackPhase


    /// <summary>
    /// Player is done healing, now the enemy gets a chance to attack
    /// </summary>
    public void EndPrayPhase()
    {
        this.EnemyAttack();
        //this.EnableBattleCamera();
        //UIController.Instance.SetUIStatus(UIDetails.Name.MainBattle, true);
        //UIController.Instance.SetUIStatus(UIDetails.Name.PlayerTurn, true);
    } // EndPrayPhase


    /// <summary>
    /// Called by the player avatar when the attack animation is done
    /// If there are additional attacks, it triggers another attack
    /// Otherwise, resets attacks and sets animation back to idle
    /// </summary>
    public void AttackAnimationEnd(BattleUnit unit)
    {
        this.ValidatePlayerAttack();
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
                this.OnAttackButtonClick();
                break;
            case UIButton.Name.Word:
                // Ignore empty words
                WordContainer word = button.GetComponent<WordContainer>();
                if( ! string.IsNullOrEmpty(word.WordText.text) ) {
                    this.OnWordContainerClick(button.GetComponent<WordContainer>());
                }                
                break;
            case UIButton.Name.Commit:
                this.BeginUnitAttackPhase(true);
                break;
            case UIButton.Name.Cancel:
                this.CancelAttack();
                break;
        }
    } // OnButtonReleased
} // class