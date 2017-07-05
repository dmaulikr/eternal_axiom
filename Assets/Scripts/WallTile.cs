using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the showing and fading of a wall tile
/// </summary>
public class WallTile : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the AnimatorController Component
    /// </summary>
    Animator animator;
    Animator AnimatorController
    {
        get
        {
            if(this.animator == null) {
                this.animator = GetComponentInChildren<Animator>();
            }
            return this.animator;
        }
    }

    /// <summary>
    /// True: revelas the wall
    /// False: hides the wall
    /// </summary>
    public bool HideWall
    {
        set
        {
            this.AnimatorController.SetBool("FadeOut", value);
        }
    }
    
    /// <summary>
    /// While the player is close enough to this wall and this wall
    /// is obstructing the player's view, we will hide the wall
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player") {
            this.HideWall = true;
        }
    }

    /// <summary>
    /// Hides the player since the player is far enough that this wall 
    /// is no longer obstructing it
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player") {
            this.HideWall = false;
        }
    }
}
