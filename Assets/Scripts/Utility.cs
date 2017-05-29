using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
	/// <summary>
    /// Attempts to return the resources found in the given path if found
    /// Null when the resources was not loaded
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T LoadResource<T>(string path) where T : Object
    {
        T resource = null;        
            
        // The resources may not exist so let's be careful
        try {            
            resource = Resources.Load<T>(path);
        } catch( UnityException exception ) {
            Debug.Log( "Failed load resource. Reason = " + exception);
        } // try

        return resource;
    } // LoadResource

    /// <summary>
    /// Attempts to return an array of all resources located in the given path
    /// of a certain type.
    /// </summary>
    /// <typeparam name="T">Resource Type</typeparam>
    /// <param name="path">Path to resources excluding the root /Resources</param>
    /// <returns>Null or array of resources</returns>
    public static T[] LoadResources<T>(string path) where T : Object
    {
        T[] resources = null;

        // The resources may not exist so let's be careful
        try {            
            resources = Resources.LoadAll<T>(path);
        } catch( UnityException exception ) {
            Debug.Log( "Failed load resources. Reason = " + exception);
        } // try

        return resources;
    } // LoadResources
} // class