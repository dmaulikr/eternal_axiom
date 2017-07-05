using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI Element that will alway look at the camera
/// </summary>
public class UILookAtCamera : MonoBehaviour
{
    [SerializeField]
    Transform mainCamera;
    
	// Use this for initialization
	void Start ()
    {
        this.mainCamera = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // this.transform.position + this.mainCamera.transform.rotation * Vector3.back, 
        // this.mainCamera.transform.rotationVector3.up
        // this.transform.LookAt(Camera.main.transform, Vector3.up);

        Vector3 lookAt = this.mainCamera.position - this.transform.position;
        Vector3 desiredTarget = new Vector3(lookAt.x, 0f, lookAt.z);
        Quaternion targetRotation = Quaternion.LookRotation(desiredTarget, Vector3.up);

        Debug.Log("Euler: " + targetRotation.eulerAngles);

        // Apply the rotation
        this.transform.eulerAngles = targetRotation.eulerAngles;
	}
}
