using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Scripture : MonoBehaviour
{
    /// <summary>
    /// A reference to the verse text ui container
    /// </summary>
    Text verseText;
    Text VerseText
    {
        get
        {
            if(this.verseText == null) {
                this.verseText = this.transform.FindChild("Verse").GetComponentInChildren<Text>();
            }
            return this.verseText;
        }
    } // VerseText

    /// <summary>
    /// True when the verse no longer has any blanks
    /// </summary>
    public bool isVerseFull
    {
        get
        {
            return UnityEditor.ArrayUtility.IndexOf(this.verse, this.blank) < 0;
        }
    } // isVerseFull

    /// <summary>
    /// Holds all the individual words that make up the verse
    /// </summary>
    string[] verse;

    /// <summary>
    /// Represents a missing word in the verse
    /// </summary>
    string blank = "_____";

    /// <summary>
    /// Contains the indexes that have been replaced with blanks
    /// </summary>
    List<int> indexes = new List<int>();


    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        this.verse = this.VerseText.text.Split(' ');

        for(int i = 0; i < this.verse.GetLength(0); i++) {
            string word = this.verse[i];
            if(word == this.blank) {
                this.indexes.Add(i);
            }
        }
    } // Start


    /// <summary>
    /// Updates the verse text to reflect what it is currently in the <see cref="verse"/> array
    /// </summary>
    void RefreshVerseText()
    {
        this.VerseText.text = string.Join(" ", this.verse);
    } // RefreshVerseText


    /// <summary>
    /// Updates the verse with the given word
    /// Position stands for which order the word was selected (first, second, third, etc.)
    /// </summary>
    /// <param name="position"></param>
    /// <param name="word"></param>
    public void SetWordAtPosition(int position, string word)
    {
        if(position >= this.indexes.Count) {
            return;
        }

        int index = this.indexes[position];
        this.verse[index] = "<color=#FFDE70FF>" + word + "</color>";
        this.RefreshVerseText();
    } // SetWordAtPosition


    /// <summary>
    /// Updates the word at the given position to be a blank
    /// </summary>
    /// <param name="position"></param>
    public void RemoveWordAtPosition(int position)
    {
        if(position >= this.indexes.Count) {
            return;
        }

        int index = this.indexes[position];
        this.verse[index] = this.blank;
        this.RefreshVerseText();
    } // RemoveWordAtPosition


    /// <summary>
    /// Re-adds all blanks back to the verse
    /// </summary>
    public void ResetVerse()
    {
        foreach(int index in this.indexes) {
            this.verse[index] = this.blank;
        }

        this.RefreshVerseText();
    } // ResetVerse()

} // Class