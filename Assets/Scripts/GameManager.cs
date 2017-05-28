using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spanws itself by invoking the Instance static property should one not exist
/// Responsible for:
/// - keeping the state of the game 
/// - saving/loading the game
/// - track of the player's progress
/// Controllers communicate with the game manager for all system/top level information
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Instance of the GameManager
    /// </summary>
    static GameManager instance;

    /// <summary>
    /// Active GameManager instance
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if(instance==null) {
                GameObject gmObject = new GameObject("_GameManager", typeof(GameManager));
                instance = gmObject.GetComponent<GameManager>();
            }
            return instance;
        }
    } // Instance

    /// <summary>
    /// The current level the player is on
    /// </summary>
    public int level = 1;


    /// <summary>
    /// Prevents the GameManger from having more that one instance
    /// Prevent the GameManager from being destroy on scene load
    /// </summary>
    void Awake()
    {
        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    } // Awake

} // class