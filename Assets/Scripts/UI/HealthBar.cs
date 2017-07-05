using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player's health bar script
/// Changes to display the player's current health (a.k.a faith)
/// </summary>
public class HealthBar : MonoBehaviour
{
    /// <summary>
    /// A reference to the player script
    /// </summary>
    [SerializeField]
    IsometricPlayerController target;

    /// <summary>
    /// init
    /// </summary>
    void Awake()
    {
        this.target = FindObjectOfType<IsometricPlayerController>();
    }

    /// <summary>
    /// Update UI to show health
    /// </summary>
    void Update()
    {
        if(this.target != null) {
            Vector3 localScale = this.transform.localScale;
            float xscale = (float)this.target.Faith / (float)this.target.MaxFaith;
            this.transform.localScale = new Vector3(xscale, localScale.y, localScale.z);
        }
    }

}
