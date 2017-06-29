using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the creation of an isometric dungeon 
/// Receives an array containing the locations of:
///     - floors
///     - walls
///     - door
/// Generates the tiles as well as a tile map where each is position
/// </summary>
public class IsometricDungeonGenerator : MonoBehaviour
{
    /// <summary>
    /// Used to know how much the tiles are scaleb and align them according
    /// As they are instantiated on the 
    /// </summary>
    [SerializeField]
    Vector3 tileScaling = new Vector3(2f, 1f, 2f);

    /// <summary>
    /// The list key represents the int value in the tile map passed during created
    /// while the value indicates which prefab to instantiate
    /// </summary>
    [SerializeField]
    List<GameObject> prefabList = new List<GameObject>();

    /// <summary>
    /// Contains the current map generated
    /// </summary>
    public int[][] tileMap = new int[][] {
        new int[] {0,0,0,0,0,2,2,2,2,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,2,1,2,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,0,2,1,2,0,0,0,0,0,0},
        new int[] {0,0,0,0,0,0,2,1,2,0,0,0,0,0,0},
        new int[] {0,0,0,0,0,0,2,1,2,0,0,0,0,0,0},
        new int[] {2,2,2,2,2,2,2,1,2,2,2,2,2,2,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,2,2,2,1,2,2,2,2,1,2,2,2,2,2},
        new int[] {0,0,2,1,1,1,1,2,1,1,1,1,1,2,0},
        new int[] {0,0,2,1,1,1,1,2,1,1,1,1,1,2,0},
        new int[] {0,0,2,1,1,1,1,2,1,1,1,1,1,2,0},
        new int[] {0,0,2,2,2,2,2,2,2,2,2,2,2,2,0},
    };

    void Start()
    {
        this.CreateDungeon();
    }
    
    /// <summary>
    /// /// <summary>
    /// Instantiates the objects represented in the tileMap based on their 
    /// position in the array and <see cref="this.tileScaling"/>
    /// Auto creates floor tiles underneadth certain objects such as:
    ///     - walls
    ///     - doors
    ///     - spawn points
    /// </summary>
    /// <param name="tileMap">Jagged Int array</param>
    public void CreateDungeon()
    {
        this.DestroyTiles();

        for(int z = 0; z < this.tileMap.GetLength(0); z++) {
            for(int x = 0; x < this.tileMap[z].GetLength(0); x++) {
                Vector3 desitnation = new Vector3(x * this.tileScaling.x,
                                                  0f,
                                                  z * this.tileScaling.z);

                string tileName = string.Format("Tile_{0}_{1}", z, x);
                this.CreateTileAt(this.tileMap[z][x], tileName, desitnation);
            } // for x
        } // for z
    }

    /// <summary>
    /// Removes all child objects
    /// Since this is the parent for dungeon tiles, they are all removed
    /// DestroyImmediate used as for the custom editor script
    /// </summary>
    void DestroyTiles()
    {
        List<GameObject> children = new List<GameObject>();

        foreach(Transform child in this.transform) {
            children.Add(child.gameObject);
        }

        children.ForEach(child => DestroyImmediate(child));
    }

    /// <summary>
    /// Instantiate an instance of the prefab assciated with the given type at the given destination
    /// Specific types such as "walls" have a floor tile created beneath it as well
    /// </summary>
    /// <param name="type"></param>
    /// <param name="destination"></param>
    void CreateTileAt(int type, string tileName, Vector3 destination)
    {
        GameObject prefab = this.prefabList[type];
        GameObject go = Instantiate(prefab, this.transform, false);
        go.transform.position = destination;
        go.name = tileName;
        // Creates a floor tile beneath this tile if it requires it
        switch(type) {
            // Wall
            case 2:
                tileName += "_Wall_Support";
                this.CreateTileAt(1, tileName, destination);
                break;
        }
    }

}
