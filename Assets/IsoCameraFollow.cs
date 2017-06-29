using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoCameraFollow : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    float followSpeed = 5f;

    /// <summary>
    /// Moves after the player has moved
    /// </summary>
    void LateUpdate()
    {
        Vector3 targetPosition = this.target.position;
        this.transform.position = Vector3.Lerp(this.transform.position, 
                                               targetPosition, 
                                               this.followSpeed * Time.deltaTime);
    }
}
