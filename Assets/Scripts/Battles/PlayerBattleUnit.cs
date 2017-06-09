using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player unit while in the Battle Scene
/// </summary>
public class PlayerBattleUnit : BattleUnit
{
    /// <summary>
    /// Defines the player unit
    /// </summary>
    void Awake()
    {
        this.health = this.maxHealth;
    } // Awake()

} // class
