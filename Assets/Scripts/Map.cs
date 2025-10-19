using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Map : MonoBehaviour
{
    #region Events

    public UnityEvent OnResolveMatches;

    #endregion

    [Header("Map size")]
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private Transform mapOrigin;

    [Header("Prefabs")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSpacing = 0;
    [SerializeField] GameObject cellContainer;
    [SerializeField] private GameObject tilePrefab;

    public Tile[,] Grid { get; private set; }
    public bool[,] Matched { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager.Instance.OnTileSwapped.AddListener(HandleTileSwapped);
        OnResolveMatches.AddListener(HandleResolveMatches);

        RebuildMap();
    }

    #region Public Methods

    public void GenerateNewMap()
    {
        ClearMap();
        BuildMap();
    }

    public void ResolveMatches()
    {
        CheckMatches();
        do
        {
            RemoveMatchedTiles();

            // Different refill schemes based on scene name
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level1")
            {
                RefillMap1();
            }
            else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level2")
            {
                RefillMap2();
            }

            CheckMatches();
        }
        while (HasMatches());
    }

    public static Vector3 GridPosToWorldPos(int row, int column, Transform mapOrigin, float cellSpacing)
    {
        Vector3 worldPosition = mapOrigin.position + new Vector3(
            column * (1 + cellSpacing),
            row * (1 + cellSpacing),
            0
        );

        return worldPosition;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Build the map by instantiating cells and tiles in a grid layout.
    /// </summary>
    private void BuildMap()
    {
        Grid = new Tile[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 cellPosition = GridPosToWorldPos(row, column, mapOrigin, cellSpacing);

                GameObject cellObject = Instantiate(cellPrefab, cellPosition, Quaternion.identity, cellContainer.transform);
                cellObject.name = $"Cell ({row}, {column})";

                GameObject tileObject = Instantiate(tilePrefab, cellPosition, Quaternion.identity, transform);
                tileObject.name = $"Tile ({row}, {column})";

                Tile tile = tileObject.GetComponent<Tile>();
                TileColor color = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                tile.Initialize(color, new Pair<int, int>(row, column), cellPosition);

                Grid[row, column] = tile;
            }
        }
    }

    /// <summary>
    /// Check the map for any matches of 3 or more tiles of the same color in a row or column.
    /// Update the 'matched' array to mark matched tiles.
    /// </summary>
    private void CheckMatches()
    {
        // check a nxn grid to see if there are any matches vertically or horizontally, matches occur when 3 or more tiles of the same color are adjacent
        // build a 2D array of Tile references from the instantiated cells
        Matched = new bool[rows, columns];

        // check horizontal matches
        for (int row = 0; row < rows; row++)
        {
            int runStart = 0;
            for (int col = 1; col <= columns; col++)
            {
                Tile prev = Grid[row, col - 1];
                Tile cur = (col < columns) ? Grid[row, col] : null;
                bool continueRun = prev != null && cur != null && prev.tileColor == cur.tileColor;

                if (!continueRun)
                {
                    int runLength = col - runStart;
                    if (runLength >= 3)
                    {
                        for (int c = runStart; c < col; c++)
                            Matched[row, c] = true;
                    }
                    runStart = col;
                }
            }
        }

        // check vertical matches
        for (int col = 0; col < columns; col++)
        {
            int runStart = 0;
            for (int row = 1; row <= rows; row++)
            {
                Tile prev = Grid[row - 1, col];
                Tile cur = (row < rows) ? Grid[row, col] : null;
                bool continueRun = (prev != null && cur != null && prev.tileColor == cur.tileColor);

                if (!continueRun)
                {
                    int runLength = row - runStart;
                    if (runLength >= 3)
                    {
                        for (int r = runStart; r < row; r++)
                            Matched[r, col] = true;
                    }
                    runStart = row;
                }
            }
        }
    }

    /// <summary>
    /// Check if there are any matched tiles on the map.
    /// </summary>
    /// <returns>True if there are matches, false otherwise.</returns>
    private bool HasMatches()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Matched[row, col])
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Remove matched tiles marked from the map.
    /// </summary>
    private void RemoveMatchedTiles()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Matched[row, col])
                {
                    Destroy(Grid[row, col].gameObject);
                    Grid[row, col] = null;
                    Matched[row, col] = false;
                }
            }
        }
    }

    /// <summary>
    /// Refill the map by instantiating new tiles in empty grid positions at the start.
    /// cheat by generating new tiles in empty grid positions instead of moving down existing tiles.
    /// </summary>
    private void RefillMapStart()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Grid[row, col] == null)
                {
                    Vector3 tilePosition = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

                    GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);
                    Tile tile = tileObject.GetComponent<Tile>();

                    tileObject.name = $"Tile ({row}, {col})";
                    TileColor tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                    tile.Initialize(tileColor, new Pair<int, int>(row, col), tilePosition);

                    Grid[row, col] = tile;
                }
            }
        }
    }

    /// <summary>
    /// Move existing tiles down to fill empty spaces.
    /// </summary>
    private void MoveTilesDown()
    {
        // Move the tiles down to fill empty spaces
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Grid[row, col] == null)
                {
                    // Find the next tile above to move down
                    for (int searchRow = row + 1; searchRow < rows; searchRow++)
                    {
                        if (Grid[searchRow, col] != null)
                        {
                            Grid[row, col] = Grid[searchRow, col];
                            Grid[searchRow, col] = null;
                            Grid[row, col].gridPosition = new Pair<int, int>(row, col);

                            // Update the tile target position
                            Vector3 newPosition = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);
                            Grid[row, col].SetTargetPosition(newPosition);

                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Refille scheme 1:
    /// </summary>
    private void RefillMap1()
    {
        MoveTilesDown();

        // After moving existing tiles down, fill in new tiles at the top
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Grid[row, col] == null)
                {
                    Vector3 tilePosition = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

                    GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);
                    Tile tile = tileObject.GetComponent<Tile>();

                    tileObject.name = $"Tile ({row}, {col})";
                    TileColor tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                    tile.Initialize(tileColor, new Pair<int, int>(row, col), tilePosition);

                    Grid[row, col] = tile;
                }
            }
        }
        
    }

    /// <summary>
    /// Refille scheme 2:
    /// </summary>
    private void RefillMap2()
    {
        MoveTilesDown();
    }

    #region Event Handlers

    // Handle the OnTileSwapped event
    private void HandleTileSwapped(Tile firstTile, Tile secondTile)
    {
        float delay = 0.2f;
        StartCoroutine(SwapTilesCoroutine(firstTile, secondTile, delay));
    }

    // Couroutine to move tiles with delay
    private IEnumerator SwapTilesCoroutine(Tile firstTile, Tile secondTile, float delay)
    {
        // Cache positions and grid pos info
        Vector3 firstTilePos = firstTile.transform.position;
        Vector3 secondTilePos = secondTile.transform.position;
        Pair<int, int> firstTileGridPos = firstTile.gridPosition;
        Pair<int, int> secondTileGridPos = secondTile.gridPosition;

        // Update the 2 tiles target positions and grid positions
        firstTile.SetTargetPosition(secondTilePos);
        secondTile.SetTargetPosition(firstTilePos);
        firstTile.gridPosition = secondTileGridPos;
        secondTile.gridPosition = firstTileGridPos;

        // Update the map grid
        Grid[firstTileGridPos.First, firstTileGridPos.Second] = secondTile;
        Grid[secondTileGridPos.First, secondTileGridPos.Second] = firstTile;

        // Animation delay
        yield return new WaitForSeconds(delay);

        // Check matches after swap, if yes, invoke OnResolveMatches event
        // else swap back
        CheckMatches();
        if (!HasMatches())
        {
            // Swap back
            firstTile.SetTargetPosition(firstTilePos);
            secondTile.SetTargetPosition(secondTilePos);
            firstTile.gridPosition = firstTileGridPos;
            secondTile.gridPosition = secondTileGridPos;

            // Update the map grid
            Grid[firstTileGridPos.First, firstTileGridPos.Second] = firstTile;
            Grid[secondTileGridPos.First, secondTileGridPos.Second] = secondTile;
        }
        else
        {
            OnResolveMatches.Invoke();
        }
    }

    // Handle the OnResolveMatches event
    private void HandleResolveMatches()
    {
        ResolveMatches();
    }
    
    #endregion

    #endregion

    #region Context Menu Actions
    [ContextMenu("Clear Map")]
    private void ClearMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    [ContextMenu("Rebuild Map")]
    private void RebuildMap()
    {
        GenerateNewMap();
        CheckMatches();
        do
        {
            RemoveMatchedTiles();
            RefillMapStart();
            CheckMatches();
        }
        while (HasMatches());
    }
    
    #endregion
}
