using UnityEngine;

/// <summary>
/// Unique details for a camera
/// </summary>
public class CameraDetails : MonoBehaviour
{
    /// <summary>
    /// A list of the different cameras available 
    /// </summary>
    public enum Name
    {
        Main,
        Battle,
        Prayer,
        PlayerAttack,
        EnemyAttack,
    }

    /// <summary>
    /// The name for this camera
    /// </summary>
    public Name cameraName;
}