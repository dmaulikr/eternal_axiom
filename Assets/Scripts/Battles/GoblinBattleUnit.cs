using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinBattleUnit : BattleUnit
{
	/// <summary>
    /// Defines the unit
    /// </summary>
    void Awake()
    {
        this.health = 100;
        this.maxHealth = 100;
        this.totalAttacks = 1;
        this.attackPower = 25;
    } // Awake()
} // class