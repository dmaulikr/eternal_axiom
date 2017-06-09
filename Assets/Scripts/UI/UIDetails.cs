using UnityEngine;

/// <summary>
/// Unique details for a user interface
/// </summary>
public class UIDetails : MonoBehaviour
{
    /// <summary>
    /// A list of the different cameras available 
    /// </summary>
    public enum Name
    {
        Dungeon,
        MainBattle,
        PlayerTurn,
        PlayerAttack,
        BattleMessage,
    }

    /// <summary>
    /// The name for this camera
    /// </summary>
    public Name uiName;

    /// <summary>
    /// Higher values causes UI to be render ontop of lower ones
    /// </summary>
    public float renderOrder = 0f;
}