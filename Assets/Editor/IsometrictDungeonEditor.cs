using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IsometricDungeonGenerator))]
public class IsometrictDungeonEditor : Editor
{
    /// <summary>
    /// Instance of the generator
    /// </summary>
    IsometricDungeonGenerator generator;

    void OnEnabled()
    {
        this.generator = (IsometricDungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        IsometricDungeonGenerator generator = (IsometricDungeonGenerator)target;
        base.OnInspectorGUI();
        if( GUILayout.Button("Generate") ) {
            generator.CreateDungeon();

        }
    }
}