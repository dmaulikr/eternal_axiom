using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for all units that partake in a Battle scene
/// </summary>
public class BattleUnit : MonoBehaviour
{
    /// <summary>
    /// Current health
    /// </summary>
    internal int health;

    /// <summary>
    /// Total Health
    /// </summary>
    [SerializeField]
    internal int maxHealth;

    /// <summary>
    /// The current attack number in the sequence the unit is performing
    /// </summary>
    int currentAttack = 0;

    /// <summary>
    /// Total attacks the unit can perform in a sequence before
    /// having to reset back to one. 
    /// </summary>
    [SerializeField]
    internal int totalAttacks;

    /// <summary>
    /// How much damage the unit inflicts per attack
    /// </summary>
    [SerializeField]
    internal int attackPower;

    /// <summary>
    /// True when the unit is attacking
    /// </summary>
    bool isAttacking = false;

    /// <summary>
    /// True when the unit is healing
    /// </summary>
    bool isHealing = false;

    /// <summary>
    /// True when the unit is defending which halves the attack received
    /// </summary>
    bool isDefending = false;

    /// <summary>
    /// Current target being attacked
    /// </summary>
    internal BattleUnit target;

    /// <summary>
    /// A refrence to the animator controller
    /// </summary>
    Animator animator;
    Animator AnimatorController
    {
        get
        {
            if(this.animator == null) {
                this.animator = GetComponent<Animator>();
            }
            return this.animator;
        }
    } // AnimatorController

    /// <summary>
    /// A reference to the scene controller
    /// </summary>
    BattleController sceneController;
    BattleController SceneController
    {
        get
        {
            if(this.sceneController == null) {
                this.sceneController = FindObjectOfType<BattleController>();
            }
            return this.sceneController;
        }
    } // SceneController

    /// <summary>
    /// A reference to the health Text contained
    /// within the local canvas
    /// </summary>
    Text healthText;
    Text HealthText
    {
        get
        {
            if(this.healthText == null) {
                this.healthText = this.transform.FindChild("HealthCanvas").GetComponentInChildren<Text>();
            }
            return this.healthText;
        }
    } // HealthText

    /// <summary>
    /// The GameObject for the text that pops up to represent damage or healing
    /// </summary>
    [SerializeField]
    GameObject textPopup;


    /// <summary>
    /// Initializes unit
    /// </summary>
    void Start()
    {
        this.HealthText.text = this.health.ToString() + " / " + this.maxHealth.ToString();
        this.AnimatorController.SetFloat("Health", this.health);
    } // Start


    /// <summary>
    /// Triggers the attack sequence increasing the attack number
    /// each time it is trigger and resetting back to 1 when it has
    /// reached the total attack number. The target is stored to 
    /// trigger the hurt animation when the attack animation hits
    /// the "hurt" frame
    /// </summary>
    /// <param name="target">The unit being attacked</param>
    public void Attack(BattleUnit target)
    {
        this.isAttacking = true;
        this.target = target;
        this.currentAttack++;
        
        // Resets when the end is reached
        if(this.currentAttack > this.totalAttacks) {
            this.currentAttack = 1;
        }

        this.AnimatorController.SetFloat("AttackNumber", this.currentAttack);

        // Animation must be played manually as they do not loop
        // and the mechanim stays on the last frame after the previous animation ends
        this.AnimatorController.Play("Attack", -1, 0f);
        this.AnimatorController.SetTrigger("Attack");
    } // Attack


    /// <summary>
    /// Triggers the target to receive damage based on the multiplier given
    /// </summary>
    /// <param name="multiplier">how much to multiply the unit's attack power by</param>
    public void InflictDamage(int multiplier = 1)
    {
        if(this.target == null) {
            return;
        }
        multiplier = Mathf.Abs(multiplier);
        int damage = this.attackPower * multiplier;
        this.target.TakeDamage(damage);
    } // InflictDamage


    /// <summary>
    /// Called throughout the attack sequence when the attack "connects"
    /// with <see cref="target"/> to trigger a "damaged" animation on the target
    /// This is merely an animation update and does not inflict damage or 
    /// change the health. (Visual change only)
    /// </summary>
    public void AttackConnected()
    {
        if(this.target == null) {
            return;
        }
        this.target.AnimatorController.SetTrigger("Hurt");
    } // AttackConnected


    /// <summary>
    /// Triggers the animation to heal
    /// </summary>
    internal void TriggerHealing()
    {
        this.isHealing = true;
        this.AnimatorController.SetTrigger("Heal");
    } // TriggerHealing


    /// <summary>
    /// Restores the units health
    /// </summary>
    public void HealUnit()
    {
        // Turn on defend
        this.isDefending = true;

        // 30% Healing from max health
        int healing = Mathf.RoundToInt(this.maxHealth * 0.30f);
        this.health = Mathf.Min(this.health + healing, this.maxHealth);

        this.AnimatorController.SetFloat("Health", this.health);
        this.SpawnPopupPrefab(this.textPopup, healing.ToString());
        this.SceneController.EndPrayPhase();
        this.HealthText.text = this.health.ToString() + " / " + this.maxHealth.ToString();
    } // HealUnit


    /// <summary>
    /// Triggers the hurt animation displaying the damage taken
    /// and updating the <see cref="health"/> to reflect the new health
    /// </summary>
    /// <param name="damage"></param>
    internal void TakeDamage(int damage)
    {
        // Cut damage in half if the unit is defending
        // Defending last for a single attack
        if(this.isDefending) {
            this.isDefending = false;
            damage = Mathf.RoundToInt(damage * 0.5f);
        }

        this.health = Mathf.Max(0, this.health - damage);
        this.SpawnPopupPrefab(this.textPopup, damage.ToString());
        this.HealthText.text = this.health.ToString() + " / " + this.maxHealth.ToString();

        if(this.health < 1) {
            this.AnimatorController.SetTrigger("Death");
        }
    } // TakeDamage


    /// <summary>
    /// Handles the unit's attack end
    /// </summary>
    public void EndAttack()
    {
        this.isAttacking = false;
        this.currentAttack = 0;
        this.AnimatorController.SetFloat("AttackNumber", 0);
    } // EndAttack


    /// <summary>
    /// Triggered when an animation completes so long as it has the curve that calls this
    /// </summary>
    public void AnimationEnd()
    {
        if(this.isAttacking) {
            this.SceneController.AttackAnimationEnd(this);
        }
    } // AnimationEnd


    /// <summary>
    /// Spawns a Text UI prefab that plays an animation of text moving up and then down
    /// that fades away. What get's displays will be based on the "text" given
    /// </summary>
    /// <param name="prefab"> the text popup prefab 
    /// <param name="damage"> the text inflicted on the unit</param>
    void SpawnPopupPrefab(GameObject prefab, string text) 
    {
        if(prefab == null) {
            return;
        }

        Vector3 startPos     = this.transform.position;
        GameObject popupText = Instantiate(prefab, startPos, Quaternion.identity) as GameObject;

        // Needs to be attached to the canvas or else it won't load
        GameObject canvas = this.transform.FindChild("HealthCanvas").gameObject;

        if(popupText != null && canvas != null) {
            popupText.transform.SetParent(canvas.transform, false);

            TextPopUp popup = popupText.GetComponent<TextPopUp>();
            popup.setValue(text);
        } // if

    } // SpawnPopupPrefab


    /// <summary>
    /// Notifies the controller that a unit is dead
    /// </summary>
    public void DeathAnimationEnd()
    {
        this.SceneController.UnitDied(this);
    } // DeathAnimationEnd


    /// <summary>
    /// True when the unit's health is less than 1
    /// </summary>
    /// <returns></returns>
    public bool IsDead()
    {
        return this.health < 1;
    }

} // class
