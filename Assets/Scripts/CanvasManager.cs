using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the updating of the UI elements such as which to display
/// depending on the state of the game
/// </summary>
public class CanvasManager : MonoBehaviour
{
    [SerializeField]
    GameObject virtualDPad;

    [SerializeField]
    GameObject battleSystem;

    void Start()
    {
        this.BattleWon();
    }

    public void BattleEncounter()
    {
        if(this.virtualDPad != null) {
            this.virtualDPad.SetActive(false);
        }

        if(this.battleSystem != null) {
            this.battleSystem.SetActive(true);
        }
    }

    public void GameOver()
    {
        if(this.virtualDPad != null) {
            this.virtualDPad.SetActive(false);
        }

        if(this.battleSystem != null) {
            this.battleSystem.SetActive(false);
        }
    }

    public void BattleWon()
    {
        if(this.virtualDPad != null) {
            this.virtualDPad.SetActive(true);
        }

        if(this.battleSystem != null) {
            this.battleSystem.SetActive(false);
        }
    }
}
