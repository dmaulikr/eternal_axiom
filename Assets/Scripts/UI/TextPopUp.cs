using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animated UI Text component that displays the damages 
/// inflicted on a given target
/// </summary>
public class TextPopUp : MonoBehaviour 
{
    /// <summary>
    /// References the AnimatorController component of the child Text component
    /// </summary>
    [SerializeField]
    Animator childAnimator;

    /// <summary>
    /// References the child text component
    /// </summary>
    [SerializeField]
    Text childTextComponent;


    /// <summary>
    /// Initializes the class
    /// Sets a timer to destroy the object once the animation is done
    /// </summary>
    void Start() 
    {
        this.childAnimator      = GetComponentInChildren<Animator>();
        this.childTextComponent = GetComponentInChildren<Text>();

        // Grab the information for the current clip in the AnimatorController
        // 0 because we don't any other clips right now
        AnimatorClipInfo[] clipInfo = this.childAnimator.GetCurrentAnimatorClipInfo(0);
        float time = clipInfo[0].clip.length;

        // Destroys the object after a given time
        // We use the clip's info to determine how long
        Destroy(this.gameObject, time);
    } // start


    /// <summary>
    /// Sets the text of the child's text component
    /// </summary>
    /// <param name="text">The text to set it to</param>
    public void setValue(string text) 
    {
        this.childTextComponent.text = text;
    } // setValue
} // class