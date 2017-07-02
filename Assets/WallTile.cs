using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the showing and fading of a wall tile
/// </summary>
public class WallTile : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the Animator Component
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
}
