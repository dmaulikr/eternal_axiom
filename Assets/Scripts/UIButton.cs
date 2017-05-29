using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UIButtons notify the associated controller when a button
/// has been pressed or released. Controllers are associated 
/// based on the type of the button. The button itself is 
/// passed to the controller to interprit the action
/// </summary>
public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// Different UI button names which help determine the action to perform
    /// </summary>
    public enum Name
    {
        Word,
        Pray,
        Attack,
        Settings,
        Cancel,
        Interact,
        Commit,
    } // Name

    /// <summary>
    /// Different controllers that listen for a button press
    /// </summary>
    public enum Controller
    {
        Dungeon,
        BattleSequence,
    } // Controller

    /// <summary>
    /// Which actions this button invokes
    /// </summary>
    public Name buttonName;

    /// <summary>
    /// Which controller to notify when interacted with
    /// </summary>
    public Controller controller;

    /// <summary>
    /// A reference to the image that represents this button
    /// </summary>
    Image buttonImage;

    /// <summary>
    /// A reference to the dugeon controller
    /// </summary>
    DungeonController dungeonController;

    /// <summary>
    /// A reference to the battle sequence controller
    /// </summary>
    BattleController battleController;


    /// <summary>
    /// Stores all references
    /// </summary>
    void Start()
    {
        this.buttonImage = GetComponent<Image>();
        this.dungeonController = GameObject.FindObjectOfType<DungeonController>();
        this.battleController = GameObject.FindObjectOfType<BattleController>();
    } // Start

    /// <summary>
    /// Returns True when the point of contact was made within the bounds
    /// of this button
    /// </summary>
    /// <param name="ped">Information about the point of contact</param>
    /// <returns>True: interaction occured on this button</returns>
    bool InBounds(PointerEventData ped)
    {
        // Holds the position of the touch
        Vector2 pos;

        // Check if the touch is within the bounds on the background image
        bool inBound = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.buttonImage.rectTransform,
            ped.position,
            ped.pressEventCamera,
            out pos
        );

        return inBound;
    } // InBounds

	/// <summary>
    /// Called once when the touch input is detected
    /// </summary>
    /// <param name="ped">The input information</param>
	public virtual void OnPointerDown(PointerEventData ped)
    {
        // Ignores interactions outside of this button
        if( ! this.InBounds(ped) ) {
            return;
        }

        switch(this.controller) {
            case Controller.Dungeon:
                this.dungeonController.OnButtonPressed(this);
                break;
            case Controller.BattleSequence:
                this.battleController.OnButtonPressed(this);
                break;
            default:
                throw new System.Exception(string.Format("Controller [{0}] is not recognized", this.controller));
        }
    } // OnPointerDown

    /// <summary>
    /// Called once when the touch input is no longer touching
    /// </summary>
    /// <param name="ped"></param>
    public virtual void OnPointerUp(PointerEventData ped)
    {
        // Ignores interactions outside of this button
        if( ! this.InBounds(ped) ) {
            return;
        }

        switch(this.controller) {
            case Controller.Dungeon:
                this.dungeonController.OnButtonReleased(this);
                break;
            case Controller.BattleSequence:
                this.battleController.OnButtonReleased(this);
                break;
            default:
                throw new System.Exception(string.Format("Controller [{0}] is not recognized", this.controller));
        }
    } // OnPointerUp
} // class