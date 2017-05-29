using UnityEngine;

/// <summary>
/// Base class for all controllers that receive UI interactions
/// Allows ease grouping of all controllers
/// </summary>
public abstract class BaseController : MonoBehaviour
{
    /// <summary>
    /// Processes the action for the button pressed
    /// </summary>
    /// <param name="button"></param>
    public abstract void OnButtonPressed(UIButton button);

    /// <summary>
    /// Processes the action for the button released
    /// </summary>
    /// <param name="button"></param>
    public abstract void OnButtonReleased(UIButton button);	
} // class