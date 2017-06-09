using UnityEngine;

/// <summary>
/// Called by Collider objects when a collision has occurred
/// </summary>
public interface ICollideable
{
    void OnCollision(string colliderName, Collider other);
}