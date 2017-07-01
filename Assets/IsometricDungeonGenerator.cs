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
    /// Holds all the walls that are currently hidden
    /// </summary>
    Dictionary<Vector3, WallTile> hiddenWalls = new Dictionary<Vector3, WallTile>();

    /// <summary>
    /// Contains the current map generated
    /// </summary>
    public int[][] tileMap = new int[][] {
        new int[] {0,0,2,2,2,2,2,2,2,2,2,2,2,2,0},
        new int[] {0,0,2,1,1,1,1,2,1,1,1,1,1,2,0},
        new int[] {0,0,2,1,1,1,1,2,1,1,1,1,1,2,0},
        new int[] {0,0,2,1,1,1,1,2,1,1,1,1,1,2,0},
        new int[] {2,2,2,2,1,2,2,2,2,1,2,2,2,2,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,1,1,1,1,1,1,1,1,1,1,1,1,1,2},
        new int[] {2,2,2,2,2,2,2,1,2,2,2,2,2,2,2},
        new int[] {0,0,0,0,0,0,2,1,2,0,0,0,0,0,0},
        new int[] {0,0,0,0,0,0,2,1,2,0,0,0,0,0,0},
        new int[] {0,0,0,0,0,0,2,1,2,0,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,2,1,2,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,0,2,2,2,2,2,0,0,0,0,0},
    };

    /// <summary>
    /// Creates the Dungeon and saves all available floor tiles
    /// </summary>
    void Start()
    {
        this.CreateDungeon();
    }
    
    /// <summary>
    /// Returns TRUE when there's a floor tile at the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsPositionWalkable(Vector3 position)
    {
        int z = (int)position.z;
        int x = (int)position.x;

        bool isWalkable = false;
        bool zInBound = z > -1 && z < this.tileMap.Length;
        bool xInBound = x > -1 && x < this.tileMap[0].Length;

        if(zInBound && xInBound) {
            isWalkable = this.tileMap[z][x] == 1;
        }

        return isWalkable;
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
    }
    
    /// <summary>
    /// Hides walls blocking the player's view
    /// Show walls that are no longer blocking the player
    /// </summary>
    public void ShowHideWalls(Vector3 playerPosition)
    {
        List<Vector3> positions = new List<Vector3> {
            // Two tiles behind
            new Vector3(playerPosition.x - 1, 0f, playerPosition.z + 2),
            // One tile behind
            new Vector3(playerPosition.x - 1, 0f, playerPosition.z + 1),
            // Tile next to the player
            new Vector3(playerPosition.x - 1, 0f, playerPosition.z),
            // One tile ahead
            new Vector3(playerPosition.x - 1, 0f, playerPosition.z - 1),
            // Two tiles ahead
            new Vector3(playerPosition.x - 1, 0f, playerPosition.z - 2)
        };

        // Show walls that are no longer blocking the view
        List<Vector3> keys = new List<Vector3>();
        foreach(KeyValuePair<Vector3, WallTile> entry in this.hiddenWalls) {
            if( ! positions.Contains(entry.Key) ) {
                entry.Value.HideWall = false;
                keys.Add(entry.Key);
            }
        }

        // Remove the wall that is no longer hidden
        foreach(Vector3 key in keys) {
            this.hiddenWalls.Remove(key);
        }

        // Hide the walls blocking the view
        foreach(Vector3 position in positions) {
            int z = (int)position.z;
            int x = (int)position.x;

            bool isWall = this.tileMap[z][x] == 2;
            bool notInList = !this.hiddenWalls.ContainsKey(position);

            if(isWall && notInList) {
                string wallName = string.Format("Tile_{0}_{1}", z, x);
                GameObject wallGO = GameObject.Find(wallName);

                if(wallName != null) {
                    WallTile wall = wallGO.GetComponent<WallTile>();
                    wall.HideWall = true;
                    this.hiddenWalls.Add(position, wall);
                }
            } // if
        } // foreach
    }
}