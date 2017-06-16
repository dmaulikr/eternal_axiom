using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creats a field of view based on the length and angle of view
/// Stops when it encounters an obstacle
/// Enables a "target found" tag when a target is within view
/// </summary>
public class FieldOfView : MonoBehaviour
{
    /// <summary>
    /// Distance from center of the unit the view extends to
    /// </summary>
    public float viewRadius;

    /// <summary>
    /// Angle from the center of the unit that degines the width of the angle
    /// </summary>
    [Range(0, 360)]
    public float viewAngle;

    /// <summary>
    /// Collision masks
    /// </summary>
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    /// <summary>
    /// Keeps a list of all targets in view
    /// </summary>
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    /// <summary>
    /// How long to wait between cycles to re-create visible targets list
    /// </summary>
    public float findTargetDelay = .25f;

    /// <summary>
    /// Triggers the visible target finder
    /// </summary>
    void Start()
    {
        StartCoroutine("FindTargetsWithDelay", this.findTargetDelay);
    }

    /// <summary>
    /// Runs throughout the application looking for visible targets
    /// Waits betweens cycles based on the given delay
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while(true) {
            yield return new WaitForSeconds(delay);
            this.FindVisibleTargets();
        }
    }

    /// <summary>
    /// Grabs all colliders within the view radius
    /// Chooses only the ones within the view angle
    /// Removes all colliders blocked by the obstacle mask
    /// </summary>
    void FindVisibleTargets()
    {
        this.visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, this.viewRadius, this.targetMask);

        for(int i = 0; i < targetsInViewRadius.GetLength(0); i++) {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - this.transform.position).normalized;

            // Within angle of view
            if(Vector3.Angle(transform.forward, directionToTarget) < this.viewAngle / 2){

                // Make sure there's no obstacle between this object and the target
                float distanceToTarget = Vector3.Distance(this.transform.position, target.position);

                if( !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, this.obstacleMask) ) {
                    Debug.Log(target.name + " is in view");
                    this.visibleTargets.Add(target);
                }
            }
        } // for
    }

    /// <summary>
    /// Returns a Vector3 that represents the direction an angle is facing in world space
    /// </summary>
    /// <param name="angleInDregrees"></param>
    /// <returns>World Space Direction</returns>
    public Vector3 DirectionFromAngle(float angleInDregrees, bool angleIsGlobal)
    {
        // Convert to global angle
        if(!angleIsGlobal) {
            angleInDregrees += transform.eulerAngles.y;
        }

        // By swapping cosign and sign we transform unity's angle into trig's version
        return new Vector3(
            Mathf.Sin(angleInDregrees * Mathf.Deg2Rad), 
            0f, 
            Mathf.Cos(angleInDregrees * Mathf.Deg2Rad)
        );
    }
}
