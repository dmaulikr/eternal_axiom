using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles interactions with cameras
/// </summary>
public class CameraController : MonoBehaviour
{
	/// <summary>
    /// The resource locations of all the cameras available to the dungeon
    /// </summary>
    [SerializeField]
    string pathToCameras = "Dungeon/Cameras";

    /// <summary>
    /// Current active camera 
    /// </summary>
    [SerializeField]
    Camera visibleCamera;

    /// <summary>
    /// Contains a list of all the UIGOs this controller manages
    /// </summary>
    [SerializeField]
    List<GameObject> allCamerasGO;

    /// <summary>
    /// Contains all cameras available to the dungeon along with a descriptive name
    /// </summary>
    Dictionary<CameraDetails.Name, Camera> Cameras = new Dictionary<CameraDetails.Name, Camera>();

     /// <summary>
    /// Instance of the GameManager
    /// </summary>
    static CameraController instance;

    /// <summary>
    /// Active GameManager instance
    /// </summary>
    public static CameraController Instance
    {
        get
        {
            if(instance==null) {
                instance = FindObjectOfType<CameraController>();
                instance.LoadCameras();
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
            instance.LoadCameras();
        }
    } // Awake

    /// <summary>
    /// Loads all cameras resources available to the dungeon
    /// Excluding <see cref="visibleCamera"/> disables all other cameras
    /// </summary>
    public void LoadCameras()
    {
        // Always starts with the current "main" camera
        this.visibleCamera = Camera.main;

        //if(string.IsNullOrEmpty(this.pathToCameras)) {
        //    throw new System.Exception("Missing path to dungeon cameras");
        //}

        //GameObject[] allCameras = Utility.LoadResources<GameObject>(this.pathToCameras);
        //if(allCameras.GetLength(0) < 1) {
        //    throw new System.Exception(string.Format("No cameras found under path [{0}]", this.pathToCameras));
        //}

        // Store the current one too
        CameraDetails.Name cameraName = this.visibleCamera.GetComponentInParent<CameraDetails>().cameraName;
        this.Cameras[cameraName] = this.visibleCamera;

        foreach(GameObject cameraGO in this.allCamerasGO) {
            cameraName = cameraGO.GetComponent<CameraDetails>().cameraName;

            // Avoid duplicates
            if(this.Cameras.ContainsKey(cameraName)) {
                Debug.Log(string.Format("Camera named [{0}] is a duplicate camera", cameraName));
                continue;
            }

            // Spawns under the container and avoids having (Clone) in the name
            GameObject newCameraGO = GameObject.Instantiate(cameraGO);
            newCameraGO.name = cameraGO.name;

            this.Cameras[cameraName] = newCameraGO.GetComponentInChildren<Camera>();
            this.Cameras[cameraName].enabled = false;
        }
    } // LoadCameras

    /// <summary>
    /// Disables the current <see cref="visibleCamera"/> 
    /// Switches its value to the given new camera
    /// Enables the new camera
    /// </summary>
    /// <param name="newCamera">Name of the new camera to turn on</param>
    public void SwitchToCamera(CameraDetails.Name newCamera)
    {
        if( !this.Cameras.ContainsKey(newCamera) ) {
            throw new System.Exception(string.Format("Unrecognized camera name [{0}]", newCamera));
        }

        this.visibleCamera.enabled = false;
        this.visibleCamera = this.Cameras[newCamera];
        this.visibleCamera.enabled = true;
    } // SwitchToCamera
} // class