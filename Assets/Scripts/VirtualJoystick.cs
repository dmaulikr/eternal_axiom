using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// The higher the number the closer to the center the joystick image remains 
    /// on the horizontal axis
    /// </summary>
    [SerializeField]
    float horizontalClamp = 2f;

    /// <summary>
    /// The higher the number the closer to the center the joystick image remains 
    /// on the vertical axis
    /// </summary>
    [SerializeField]
    float verticalClamp = 2f;

    Image backgroundImg;
    Image joystickImg;
    Vector3 inputVector;
    public Vector3 InputVector
    {
        get
        {
            return this.inputVector;
        }
    }

    void Start()
    {
        this.backgroundImg = GetComponent<Image>();
        this.joystickImg = this.transform.Find("Joystick").GetComponent<Image>();
        this.inputVector = Vector3.zero;
    }

    /// <summary>
    /// Called continually while the touch point is draged 
    /// </summary>
    /// <param name="ped"></param>
    public virtual void OnDrag(PointerEventData ped)
    {
        // Holds the position of the touch
        Vector2 pos;

        // Check if the touch is within the bounds on the background image
        bool inBound = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.backgroundImg.rectTransform,
            ped.position,
            ped.pressEventCamera,
            out pos
        );
        
        if(inBound) {
            // Transform the position into a direction, i.e [Left, Down](-1,-1) (0, 0) (1, 1) [Up, Right]
            pos.x = (pos.x / this.backgroundImg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / this.backgroundImg.rectTransform.sizeDelta.y);

            // Based on the pivot point of the background image, we will need to translate
            // the position to match the location of the click. 
            // 1 == pivot is on the right, -1 == pivot is on the left
            // the assumption being the pivot points are either full left or right
            float x = (this.backgroundImg.rectTransform.pivot.x == 1 ? pos.x * 2 + 1 : pos.x * 2 - 1);
            float y = (this.backgroundImg.rectTransform.pivot.y == 1 ? pos.y * 2 + 1 : pos.y * 2 - 1);

            // Store the translated position
            this.inputVector = new Vector3(x, 0f, y);

            // Normalize to prevent dpad-like movement
            if(this.inputVector.magnitude > 1f) {
                this.inputVector.Normalize();
            }

            // Update Joystick image to show movement
            this.joystickImg.rectTransform.anchoredPosition = new Vector3(
                this.inputVector.x * (this.backgroundImg.rectTransform.sizeDelta.x/this.horizontalClamp),
                this.inputVector.z * (this.backgroundImg.rectTransform.sizeDelta.y/this.verticalClamp)
            );
        }

    } // OnDrag

    /// <summary>
    /// Called once when the touch input is detected
    /// </summary>
    /// <param name="ped">The input information</param>
	public virtual void OnPointerDown(PointerEventData ped)
    {
        this.OnDrag(ped);
    }

    /// <summary>
    /// Called once when the touch input is no longer touching
    /// </summary>
    /// <param name="ped"></param>
    public virtual void OnPointerUp(PointerEventData ped)
    {
        this.inputVector = Vector3.zero;
        this.joystickImg.rectTransform.anchoredPosition = Vector3.zero;
    }
} // class