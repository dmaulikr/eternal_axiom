using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    [SerializeField]
    List<WallTile> hiddenWalls = new List<WallTile>();

    [SerializeField]
    List<GameObject> floorTiles = new List<GameObject>();

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
    /// How many tiles ahead/behind can the player see
    /// </summary>
    [SerializeField]
    int tileViewRange = 2;

    /// <summary>
    /// A reference to the canvas manager
    /// </summary>
    CanvasManager canvasManager;

    /// <summary>
    /// Init
    /// </summary>
    void Awake()
    {
        this.canvasManager = FindObjectOfType<CanvasManager>();
    }

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

                string tileName = string.Format("Tile_{0}_{1}", x, z);
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
    /// Returns the tile type located at the given position if a tile
    /// exists at that position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    int GetTileTypeAtPosition(Vector3 position)
    {
        // -1 No tile
        int tileType = -1; 

        int x = (int)position.x;
        int z = (int)position.z;

        bool xInBound = x > -1 && x < this.tileMap[0].Length;
        bool zInBound = z > -1 && z < this.tileMap.Length;

        if(xInBound && zInBound) {
            tileType = this.tileMap[z][x];
        }

        return tileType;
    }

    /// <summary>
    /// Returns the tile GameObject for the given position if one exists
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    GameObject GetTileObjectAtLocation(Vector3 position)
    {
        int x = (int)position.x;
        int z = (int)position.z;

        string tileName = string.Format("Tile_{0}_{1}", x, z);
        return GameObject.Find(tileName);
    }
    
    /// <summary>
    /// Hides walls blocking the player's view
    /// Show walls that are no longer blocking the player
    /// </summary>
    public void ShowHideWalls(Vector3 playerPosition)
    {
        // Creates a list of floor tiles accessible to the player that may be hidden by a wall
        List<Vector3> floorPositions = this.GetFloorTilesInViewRange(playerPosition);

        this.floorTiles.Clear();
        foreach(Vector3 position in floorPositions) {
            GameObject go = this.GetTileObjectAtLocation(position);
            if(go != null) {
                this.floorTiles.Add(go);
            }
        }
        
        // Get a list of the walls obstructing the floor tiles
        List<WallTile> wallTiles = this.GetWallsObstructingFloorTiles(floorPositions, playerPosition);

        // Reveal walls that are no longer obstructing the views
        // and save their key so that we can remove them from the list
        List<WallTile> wallTilesToRemove = new List<WallTile>();

        foreach(WallTile wallTile in this.hiddenWalls) {
            if( ! wallTiles.Contains(wallTile) ) {
                wallTile.HideWall = false;
                wallTilesToRemove.Add(wallTile);
            }
        }

        // Remove the ones no longer obstructing the view
        foreach(WallTile wall in wallTilesToRemove) {
            this.hiddenWalls.Remove(wall);
        }

        // Hide walls obstructing the view
        foreach(WallTile wallTile in wallTiles) {
            // D.R.Y.
            if(this.hiddenWalls.Contains(wallTile)) {
                continue;
            }

            wallTile.HideWall = true;
            this.hiddenWalls.Add(wallTile);
        }
    }

    /// <summary>
    /// Returns a list of tile positions that are in direct line of the player's
    /// view range including tiles behind them. Any tile obstructed by a wall is 
    /// not included in the return list. 
    /// The list includes both the X, and Z view ranges
    /// </summary>
    /// <param name="position"></param>
    /// <returns>List of Vector3 floor tile positions</returns>
    List<Vector3> GetFloorTilesInViewRange(Vector3 position)
    {
        // Always include the player player's current position
        List<Vector3> floorTilesPosition = new List<Vector3>{ position };
        List<Vector3> xTilePositions = new List<Vector3>();
        List<Vector3> zTilePositions = new List<Vector3>();
               
        // We look behind and infront of the player in both x and z axis
        // to get all tiles potentially available to the player
        for(int i = -this.tileViewRange; i <= this.tileViewRange; i++) {
            // How much to increment current tile to get the "next" tile
            int increment = (i == this.tileViewRange) ? 0 : 1;
            
            float x = position.x + i;
            float z = position.z + i;
            
            // X-Axis Tiles
            if(x > -1 && x < this.tileMap[0].Length) {
                int currentTile = this.tileMap[(int)position.z][(int)x];
                int nextTile = currentTile;

                // Prevents the next tile to be beyond the tile map size
                if(x + increment < this.tileMap[0].Length) {
                    nextTile = this.tileMap[(int)position.z][(int)x + increment];
                }
                
                // If the current or next tile is a wall, don't add it
                if (currentTile == 1 && (currentTile != 2 && nextTile != 2)) {
                    xTilePositions.Add( new Vector3(x, 0f, position.z) );
                }
            }

            // Z-Axis Tiles
            if(z > -1 && z < this.tileMap.Length) {
                int currentTile = this.tileMap[(int)z][(int)position.x];
                int nextTile = currentTile;

                // Prevents the next tile to be beyond the tile map size
                if(z + increment < this.tileMap.Length) {
                    nextTile = this.tileMap[(int)z + increment][(int)position.x];
                }

                // If the current or next tile is a wall, don't add it
                if(currentTile == 1 && (currentTile != 2 && nextTile != 2)) {
                    zTilePositions.Add( new Vector3(position.x, 0f, z) );
                }
            }
        }

        floorTilesPosition.AddRange(xTilePositions);
        floorTilesPosition.AddRange(zTilePositions);
        return floorTilesPosition.Distinct().ToList();
    }
    
    /// <summary>
    /// Casts a ray from the camera down to the each floor tile
    /// returning any wall that obstructs a floor tile
    /// </summary>
    /// <param name="floorPositions"></param>
    /// <returns></returns>
    List<WallTile> GetWallsObstructingFloorTiles(List<Vector3> floorPositions, Vector3 playerPosition)
    {
        List<WallTile> wallTiles = new List<WallTile>();

        foreach(Vector3 floorPosition in floorPositions) {
            Vector3 wallPosition = new Vector3();

            if(floorPosition.x == playerPosition.x) {
                wallPosition = new Vector3(floorPosition.x -1, floorPosition.y, floorPosition.z);
            } else {
                wallPosition = new Vector3(floorPosition.x, floorPosition.y, floorPosition.z -1);
            }

            GameObject wallGO = this.GetTileObjectAtLocation(wallPosition);
            if(wallGO != null) {
                WallTile wallTile = wallGO.GetComponentInParent<WallTile>();
                if(wallTile != null) {
                    wallTiles.Add(wallTile);
                }
            }
        }

        return wallTiles;
    }

    /// <summary>
    /// Trigger the start of a battle
    /// </summary>
    public void BattleEncounter()
    {
        this.canvasManager.BattleEncounter();
    }
}