using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordContainer : MonoBehaviour
{
    /// <summary>
    /// A reference to the index text ui
    /// </summary>
    Text wordText;
    public Text WordText
    {
        get
        {
            if(this.wordText == null) {
                this.wordText = this.transform.FindChild("WordText").GetComponent<Text>();
            }
            return this.wordText;
        }
    } // IndexText

} // Class