using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackCounter : MonoBehaviour
{
    /// <summary>
    /// Total attacks the player can do
    /// </summary>
    [SerializeField]
    int maxAttacks = 0;
    public int Max
    {
        get
        {
            return this.maxAttacks;
        }
    } // Max

    /// <summary>
    /// Total remaining attacks
    /// </summary>
    int remainingAttacks = 0;
    public int Current
    {
        get
        {
            return this.remainingAttacks;
        }
    }

    /// <summary>
    /// The text that preceeds the count
    /// </summary>
    public string textPrefix = "Remaining: ";

    /// <summary>
    /// A reference to the child text ui
    /// </summary>
    Text remainingText;
    Text RemainingText
    {
        get
        {
            if(this.remainingText == null) {
                this.remainingText = GetComponentInChildren<Text>();
            }
            return this.remainingText;
        }
    } // RemainingText

    /// <summary>
    /// True when the <see cref="remainingAttacks"/> is less than 1
    /// </summary>
    public bool noRemainingAttacks
    {
        get
        {
            return this.remainingAttacks < 1;
        }
    } // noRemainingAttacks


    /// <summary>
    /// Sets the total attacks the player is currently allowed to do
    /// </summary>
    void Start ()
    {
        this.maxAttacks = GameObject.FindGameObjectWithTag("BattlePlayer").GetComponent<PlayerBattleUnit>().totalAttacks;
        this.remainingAttacks = this.maxAttacks;        
	} // Start
	
	/// <summary>
    /// Updates the text to notify the player of total remaining attacks
    /// </summary>
	void Update ()
    {
		this.RemainingText.text = this.textPrefix + this.remainingAttacks.ToString();
	} // Update


    /// <summary>
    /// Decreases remaining attacks by one
    /// </summary>
    public void DecreaseRemainingAttacks()
    {
        this.remainingAttacks = Mathf.Max(this.remainingAttacks - 1, 0);
    } // DecreaseRemainingAttacks


    /// <summary>
    /// Increases remaining attacks by one
    /// </summary>
    public void IncreaseRemainingAttacks()
    {
        this.remainingAttacks = Mathf.Min(this.remainingAttacks + 1, this.maxAttacks);
    } // IncreaseRemainingAttacks


    /// <summary>
    /// Resets the attack counter so that the player can have the max attacks again
    /// </summary>
    internal void ResetAttacks()
    {
       this.remainingAttacks = this.maxAttacks;
    } // ResetAttacks
} // class