using UnityEngine;

/// <summary>
/// Used on trigger colliders to notify the parent object when a collision has happened
/// The collider sends information about itself as well as the object it collided with
/// </summary>
public class TriggerCollider : MonoBehaviour
{
    /// <summary>
    /// Parent object to notify on collision enter
    /// </summary>
    public ICollideable parentObject;

    /// <summary>
    /// Attempts to locate the parent
    /// </summary>
    void Start()
    {
        this.parentObject = GetComponentInParent<ICollideable>();
    } // Start

    /// <summary>
    /// Notifies the parent object of collision
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if(this.parentObject != null) {
            this.parentObject.OnCollision(this.name, other);
        }
    } // OnTriggerEnter
} // class