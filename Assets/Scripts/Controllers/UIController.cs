using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all interactions with the creation/changing of UIs
/// </summary>
public class UIController : MonoBehaviour
{ 
    /// <summary>
    /// The canvas containing all the UserInterfaces
    /// </summary>
    [SerializeField]
    GameObject MainCanvas;

    /// <summary>
    /// The resource locations of all the UserInterfaces
    /// </summary>
    [SerializeField]
    string pathToUIs = "Dungeon/UserInterfaces";

    /// <summary>
    /// Current UI display
    /// Could be enabled or disabled
    /// </summary>
    GameObject visibleUI;

    /// <summary>
    /// Contains a list of all the UIGOs this controller manages
    /// </summary>
    [SerializeField]
    List<GameObject> allUIdsGO;

    /// <summary>
    /// Contains all UIs availabe to the player
    /// </summary>
    Dictionary<UIDetails.Name, GameObject> UserInterfaces = new Dictionary<UIDetails.Name, GameObject>();

    /// <summary>
    /// Instance of the GameManager
    /// </summary>
    static UIController instance;

    /// <summary>
    /// Active GameManager instance
    /// </summary>
    public static UIController Instance
    {
        get
        {
            if(instance==null) {
                //GameObject gmObject = new GameObject("_UIController", typeof(UIController));
                //instance = gmObject.GetComponent<UIController>();
                instance = FindObjectOfType<UIController>();
                instance.LoadUIs();
            }
            return instance;
        }
    } // Instance

    /// <summary>
    /// Prevents the GameManger from having more that one instance
    /// Prevent the GameManager from being destroy on scene load
    /// </summary>
    void Awake()
    {
        if(instance == null) {
            instance = this;
            instance.LoadUIs();
        }
    } // Awake

    /// <summary>
    /// Loads all UserInterfaces objects under the MainCanvas
    /// All UIs are instantiated and disabled
    /// Call <see cref="SetUIStatus(UIDetails.Name, bool)"/> to enable any of them
    /// </summary>
	public void LoadUIs()
    {
        this.MainCanvas = GameObject.FindObjectOfType<Canvas>().gameObject;
        if(this.MainCanvas == null) {
            throw new System.Exception("No Canvas found to add UserInterfaces");
        }

        //if(string.IsNullOrEmpty(this.pathToUIs)) {
        //    throw new System.Exception("Missing path to UserInterfaces");
        //}

        //GameObject[] allUIs = Utility.LoadResources<GameObject>(this.pathToUIs);
        //if(allUIs.GetLength(0) < 1) {
        //    throw new System.Exception(string.Format("No user interfaces found under path [{0}]", this.pathToUIs));
        //}

        foreach(GameObject uiGO in this.allUIdsGO) {
            UIDetails.Name uiName = uiGO.GetComponent<UIDetails>().uiName;

            // Avoid duplicates
            if(this.UserInterfaces.ContainsKey(uiName)) {
                Debug.Log(string.Format("UserInterface named [{0}] is a duplicated UI", uiName));
                continue;
            }

            // Avoids "(Clone)" in the name
            GameObject newUIGO = GameObject.Instantiate(uiGO, this.MainCanvas.transform, false);
            newUIGO.name = uiGO.name;

            this.UserInterfaces[uiName] = newUIGO;
            newUIGO.SetActive(false);
        }

        // Default UI
        this.visibleUI = this.UserInterfaces[UIDetails.Name.Dungeon];
    } // LoadUIs

    /// <summary>
    /// Disables the current UI
    /// Loads the the new UI and enables it
    /// </summary>
    /// <param name="newUI"></param>
    public void SwitchToUI(UIDetails.Name newUI)
    {
        if( !this.UserInterfaces.ContainsKey(newUI) ) {
            throw new System.Exception(string.Format("Unrecognized UserInterface name [{0}]", newUI));
        }

        this.visibleUI.SetActive(false);
        this.visibleUI = this.UserInterfaces[newUI];
        this.visibleUI.SetActive(true);
    } // SwitchToUI

    /// <summary>
    /// Enables/Disables the given UserInterface based on the "isActive" boolean
    /// </summary>
    /// <param name="uiName">Name of the UI to modify</param>
    /// <param name="isActive">True: enable, False: disable</param>
    public void SetUIStatus(UIDetails.Name uiName, bool isActive)
    {
        if( !this.UserInterfaces.ContainsKey(uiName) ) {
            throw new System.Exception(string.Format("Unrecognized UserInterface name [{0}]", uiName));
        }

        this.UserInterfaces[uiName].SetActive(isActive);
    } // SetUIStatus

    /// <summary>
    /// Adds a new UserInterface to collection if it does not exists
    /// Can be set as current which will turn off the previous current
    /// </summary>
    /// <param name="uiName">Name of the UI</param>
    /// <param name="setAsCurrent">True: replaces current with this one. Defaults to False</param>
    /// <param name="isActive">True: enables the UI. Defaults to False</param>
    public void AddUI(UIDetails.Name uiName, GameObject uiGO, bool setAsCurrent = false, bool isActive = false)
    {
        // Avoids duplicate
        if( ! this.UserInterfaces.ContainsKey(uiName)) {
            this.UserInterfaces[uiName] = GameObject.Instantiate(uiGO, this.MainCanvas.transform, false);
            this.UserInterfaces[uiName].name = uiGO.name;
        }

        if(setAsCurrent) {
            this.SwitchToUI(uiName);
        }

        this.SetUIStatus(uiName, isActive);
    } // AddUI

    /// <summary>
    /// Returns the UserInterface parent game object
    /// This is not the canvas but rather the ui parent gameobject container
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    public GameObject GetUIGameObject(UIDetails.Name uiName)
    {
        GameObject uiGO = null;

        if(this.UserInterfaces.ContainsKey(uiName)) {
            uiGO = this.UserInterfaces[uiName];
        }

        return uiGO;
    } // GetUIGameObject

    /// <summary>
    ///  Disables all UIs including current visible one
    /// </summary>
    public void DisableAll()
    {
        foreach(KeyValuePair<UIDetails.Name, GameObject> uiEntry in this.UserInterfaces) {
            uiEntry.Value.SetActive(false);
        }
    } // DisableAll
} // class