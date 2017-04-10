using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleScene : MonoBehaviour
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
    /// Contains the words selected/added to the verse
    /// </summary>
    List<WordContainer> selectedWords = new List<WordContainer>();


    /// <summary>
    /// Hides the wordbank on scene laod
    /// </summary>
    void Awake()
    {
        this.WordBank.SetActive(false);
        this.EnableBattleCamera();
    } // Awake


    /// <summary>
    /// Enables the Battle Camera
    /// Disables the Attack Camera
    /// </summary>
    void EnableBattleCamera()
    {
        GameObject.FindGameObjectWithTag("BattleCamera").GetComponent<Camera>().enabled = true;
        GameObject.FindGameObjectWithTag("AttackCamera").GetComponent<Camera>().enabled = false;
    } // EnableBattleCamera


    /// <summary>
    /// Enables the Attack Camera
    /// Disables the Battle Camera
    /// </summary>
    void EnableAttackCamera()
    {
        GameObject.FindGameObjectWithTag("BattleCamera").GetComponent<Camera>().enabled = false;
        GameObject.FindGameObjectWithTag("AttackCamera").GetComponent<Camera>().enabled = true;
    } // EnableAttackCamera


    /// <summary>
    /// Hides the Action Button Menu and shows the word bank
    /// </summary>
	public void OnAttackButtonClick()
    {
        this.ActionButtons.SetActive(false);
        this.WordBank.SetActive(true);
    } // OnAttackButtonClick


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
        
        // 0 based index hence +1 for UI to reflect the correct order
        word.SetIndex( (index + 1).ToString() );

        this.Counter.DecreaseRemainingAttacks();
        this.ScriptureContainer.SetWordAtPosition(index, word.WordText.text);

        this.BeginPlayerAttackPhase();
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
        word.SetIndex("");

        // Setting the index to null, allows another to occupy its place
        this.selectedWords[ this.selectedWords.IndexOf(word) ] = null;

        this.ScriptureContainer.RemoveWordAtPosition(index);
        this.Counter.IncreaseRemainingAttacks();
    } // WordDeselected


    /// <summary>
    /// Forces a deselection on all currently selected words
    /// </summary>
    void ClearSelectedWords()
    {
        foreach(WordContainer word in this.selectedWords) {
            if(word == null) {
                continue;
            }
            this.WordDeselected(word);
        }
    } // ClearSelectedWords


    /// <summary>
    /// Called after a word is added to see if its time to launch the player attack phase
    /// Attack phase occurs when either the player has filled all remaining blanks or has
    /// consumed all of their remaining attacks
    /// </summary>
    void BeginPlayerAttackPhase()
    {
        if(this.ScriptureContainer.IsVerseFull || this.Counter.noRemainingAttacks) {
            this.Counter.ResetAttacks();
            this.BattleSystemUI.SetActive(false);
            this.EnableAttackCamera();
            this.ValidateAttack();
        }
    } // BeginPlayerAttackPhase


    /// <summary>
    /// Called at the beginning of each attack to see if the 
    /// chosen word is valid and triggger an attack or stop
    /// the attack phase when it is not
    /// 
    /// We use <see cref="selectedWords"/> count to determine how many words are left
    /// for validation. Each time removing one thus preventing us from validating a "blank"
    /// </summary>
    void ValidateAttack()
    {
        // End of attack
        if(this.selectedWords.Count < 1) {
            this.EndAttackPhase();
            return;
        }

        // References the word so that we reset its index and disable it when it is correct
        WordContainer word = this.selectedWords[0];
        word.SetIndex("");
        this.selectedWords.RemoveAt(0);        

        if(this.ScriptureContainer.IsNextCorrectWord()) {            
            word.GetComponent<Button>().interactable = false;
            this.ValidateAttack();
        } else {
            this.EndAttackPhase();
        }
        
    } // ValidateAttack
    

    /// <summary>
    /// Ends the attack phase by calculating total damage
    /// Inflicting the damage
    /// Resetting the camera
    /// and Re-enabling the UI
    /// The verse is reset but keep in mind only the remaining indexes are updated to blanks
    /// </summary>
    void EndAttackPhase()
    {
        // Reset any remaining word
        foreach(WordContainer word in this.selectedWords) {
            word.SetIndex("");
        }

        // Dump all selected words as to start from 0 again
        this.selectedWords.Clear();
        this.ScriptureContainer.ResetVerse();
        this.EnableBattleCamera();
        this.BattleSystemUI.SetActive(true);        
        this.ActionButtons.SetActive(true);
        this.WordBank.SetActive(false);
    } // EndAttackPhase


    /// <summary>
    /// Called by the player avatar when the attack animation is done
    /// If there are additional attacks, it triggers another attack
    /// Otherwise, resets attacks and sets animation back to idle
    /// </summary>
    public void PlayerAttackAnimationEnd()
    {

    } // PlayerAttackAnimationEnd

} // class