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
    Text indexText;
    Text IndexText
    {
        get
        {
            if(this.indexText == null) {
                this.indexText = this.transform.FindChild("Index").GetComponent<Text>();
            }
            return this.indexText;
        }
    } // IndexText

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

	
    /// <summary>
    /// Defaults index to 0
    /// </summary>
	void Start ()
    {
        this.SetIndex("");	
	} // Start
	

    /// <summary>
    /// Updates the index text value to the given index
    /// This represents in which order this word was selected
    /// </summary>
    /// <param name="index"></param>
    internal void SetIndex(string index)
    {
        //this.IndexText.text = index;
    } // SetIndex

} // Class