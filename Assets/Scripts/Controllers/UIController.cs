using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all interactions with the creation/changing of UIs
/// </summary>
public class UIController
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
    /// Contains all UIs availabe to the player
    /// </summary>
    Dictionary<UIDetails.Name, GameObject> UserInterfaces = new Dictionary<UIDetails.Name, GameObject>();

    /// <summary>
    /// Single instance of this class
    /// </summary>
    private static UIController instance;

    /// <summary>
    /// Prevents instantiation of the class without using the getter
    /// </summary>
    private UIController() { }

    /// <summary>
    /// Returns a constants instances of the UIController class
    /// with all of the UIs loaded
    /// </summary>
    public static UIController Instance
    {
        get
        {
            if(instance == null) {
                instance = new UIController();
                instance.LoadUIs();
            }
            return instance;
        }
    } // Instance

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

        if(string.IsNullOrEmpty(this.pathToUIs)) {
            throw new System.Exception("Missing path to UserInterfaces");
        }

        GameObject[] allUIs = Utility.LoadResources<GameObject>(this.pathToUIs);
        if(allUIs.GetLength(0) < 1) {
            throw new System.Exception(string.Format("No user interfaces found under path [{0}]", this.pathToUIs));
        }

        foreach(GameObject uiGO in allUIs) {
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
} // class