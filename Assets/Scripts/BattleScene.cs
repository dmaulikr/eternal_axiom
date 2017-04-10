using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// Contains the words selected/added to the verse
    /// </summary>
    List<WordContainer> selectedWords = new List<WordContainer>();


    /// <summary>
    /// Hides the wordbank on scene laod
    /// </summary>
    void Awake()
    {
        this.RenderWordBank(false);
    } // Awake


    /// <summary>
    /// Toggles the Action Buttons to show based on the balue of render
    /// </summary>
    /// <param name="render">True: show | Else: hide</param>
    void RenderActionButtons(bool render = true)
    {
        this.ActionButtons.SetActive(render);
    } // RenderActionButtons


    /// <summary>
    /// Toggles the Word Bank to show based on the balue of render
    /// </summary>
    /// <param name="render">True: show | Else: hide</param>
    void RenderWordBank(bool render = true)
    {
        this.WordBank.SetActive(render);
    } // RenderWordBank


    /// <summary>
    /// Hides the Action Button Menu and shows the word bank
    /// </summary>
	public void OnAttackButtonClick()
    {
        this.RenderActionButtons(false);
        this.RenderWordBank(true);
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
        word.SetIndex(index + 1);

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
        word.SetIndex(0);

        // Setting the index to null, allows another to occupy its place
        this.selectedWords[ this.selectedWords.IndexOf(word) ] = null;

        this.ScriptureContainer.RemoveWordAtPosition(index);
        this.Counter.IncreaseRemainingAttacks();
    } // WordDeselected


    /// <summary>
    /// Called after a word is added to see if its time to launch the player attack phase
    /// Attack phase occurs when either the player has filled all remaining blanks or has
    /// consumed all of their remaining attacks
    /// </summary>
    void BeginPlayerAttackPhase()
    {
        if(this.ScriptureContainer.isVerseFull || this.Counter.noRemainingAttacks) {
            this.Counter.ResetAttacks();
            this.ClearSelectedWords();
        }
    } // BeginPlayerAttackPhase


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
    /// Called by the player avatar when the attack animation is done
    /// If there are additional attacks, it triggers another attack
    /// Otherwise, resets attacks and sets animation back to idle
    /// </summary>
    public void PlayerAttackAnimationEnd()
    {

    } // PlayerAttackAnimationEnd

} // class