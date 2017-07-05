using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the behavior of an enemy in and outside of battle
/// depending on the state of the enemy
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class IsometricEnemy : MonoBehaviour
{
    /// <summary>
    /// How many tiles ahead can the enemy see
    /// This helps determine if an enemy is within it sight
    /// </summary>
    [SerializeField]
    int sightDistance = 1;

    /// <summary>
    /// How high from the ground should the line of sight be cast
    /// </summary>
    [SerializeField]
    float sighHeight = .5f;

    /// <summary>
    /// A reference to the AnimatorController controller
    /// </summary>
    Animator animator;

    /// <summary>
    /// The prefab to show when the player is spotted
    /// </summary>
    [SerializeField]
    GameObject exclamationPrefab;

    /// <summary>
    /// A reference to the local canvas where expresions are instantiated
    /// </summary>
    Canvas localCanvas;

    /// <summary>
    /// A reference to the local audio source 
    /// </summary>
    AudioSource aduioSource;

    /// <summary>
    /// A reference to the player script
    /// </summary>
    IsometricPlayerController player;

    /// <summary>
    /// A reference to the dungeon controller
    /// </summary>
    IsometricDungeonGenerator dungeonController;

    /// <summary>
    /// Prevents from calling the co-routine more than once
    /// </summary>
    [SerializeField]
    bool expressionTriggered = false;

    /// <summary>
    /// Init
    /// </summary>
    void Start()
    {
        this.player = FindObjectOfType<IsometricPlayerController>();
        this.aduioSource = GetComponent<AudioSource>();
        this.animator = GetComponent<Animator>();
        this.localCanvas = GetComponentInChildren<Canvas>();
        this.dungeonController = FindObjectOfType<IsometricDungeonGenerator>();
    }
    
    /// <summary>
    /// Behavior
    /// </summary>
    void Update ()
    {
        if(this.IsPlayerInSight() && !this.expressionTriggered) {
            this.expressionTriggered = true;
            this.animator.SetTrigger("Attack");
            this.player.IsSpotted = true;
            StartCoroutine("SpawnExpresion", this.exclamationPrefab);
        }
	}

    /// <summary>
    /// Returns True if nothing is obstructing the view between this unit and the player
    /// </summary>
    /// <returns></returns>
    bool IsPlayerInSight()
    {
        bool inSight = false;
        RaycastHit hitInfo;

        Vector3 origin    = new Vector3(this.transform.position.x, this.sighHeight, this.transform.position.z);
        Vector3 direction = this.transform.forward;        

        if( Physics.Raycast(origin, direction, out hitInfo, this.sightDistance)) {            
            if(hitInfo.collider.tag == "Player") {
                inSight = true;
            }
        }

        return inSight;
    }

    /// <summary>
    /// Instantiate the given prefab
    /// Waits a few seconds
    /// Removes the expresion prefab
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    IEnumerator SpawnExpresion(GameObject prefab)
    {
        this.aduioSource.Play();
        yield return new WaitForEndOfFrame();

        GameObject go = Instantiate(prefab, this.localCanvas.transform, false);
        
        yield return new WaitForSeconds(1f);
        Destroy(go);
    }

    /// <summary>
    /// Triggered when enemy attack connects with the player
    /// </summary>
    public void HurtPlayer()
    {
        this.player.TriggerHurt();
    }

    /// <summary>
    /// Triggered when the enemy's enounter attack has finished
    /// to begin the battle sequence
    /// </summary>
    public void TriggerEncounter()
    {
        this.dungeonController.BattleEncounter();
    }
}
