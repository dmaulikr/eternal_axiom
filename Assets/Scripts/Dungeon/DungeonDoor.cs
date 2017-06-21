using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the interactions with a door to open/close it
/// The enable/disable colliders is used to keep the colliders static
/// instead of animating them and stil allow the player to walk through
/// </summary>
public class DungeonDoor : MonoBehaviour
{
    /// <summary>
    /// The location to teleport the player when collision with the trigger happens
    /// </summary>
    [SerializeField]
    Transform transitionToPosition;

    /// <summary>
    /// Where to change the camera's position to when transitioning to a new room
    /// </summary>
    [SerializeField]
    Vector3 newCameraPosition;

    /// <summary>
    /// A reference to the dungeon controller
    /// </summary>
    DungeonController dungeonController;

    /// <summary>
    /// Init
    /// </summary>
    void Start()
    {
        this.dungeonController = FindObjectOfType<DungeonController>();
    }

    /// <summary>
    /// Triggers the open door anmation trigger
    /// </summary>
	public void Open()
    {
        GetComponent<Animator>().SetTrigger("Open");
    }

    /// <summary>
    /// Triggers the close door anmation trigger
    /// </summary>
    public void Close()
    {
        GetComponent<Animator>().SetTrigger("Close");
    }

    /// <summary>
    /// Triggers the transition to the next room
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player") {
            other.transform.position = this.transitionToPosition.position;
            CameraController.Instance.ActiveCamere.transform.position = this.newCameraPosition;
            this.dungeonController.TriggerDoors();
        }
    }
}