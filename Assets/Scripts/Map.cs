using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Spawning chances")]
    [SerializeField] private float sameColorSpawnChance = 0.4f;
    [SerializeField] private float differentColorSpawnChance = 0.6f;

    public Tile[,] Grid { get; private set; }
    public List<Tile> verticalMatches;
    public List<Tile> horizontalMatches;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager.Instance.OnTileSwapped.AddListener(HandleTileSwapped);
        OnResolveMatches.AddListener(HandleResolveMatches);

        GenerateNewMap();
    }

    #region Public Methods

    public static Vector3 GridPosToWorldPos(int row, int column, Transform mapOrigin, float cellSpacing)
    {
        Vector3 worldPosition = mapOrigin.position + new Vector3(
            column * (1 + cellSpacing),
            row * (1 + cellSpacing),
            0
        );

        return worldPosition;
    }

    public void AddScore()
    {
        // Add score for matches
        GameManager.Instance.AddScore(verticalMatches.Count + horizontalMatches.Count);
        GameManager.Instance.IncreaseMultiplier();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Build the map by instantiating cells and tiles in a grid layout.
    /// </summary>
    private void BuildMap()
    {
        ClearMap();

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

        // Reset
        ClearMatches();

        // check vertical matches
        for (int col = 0; col < columns; col++)
        {
            int runStart = 0;
            for (int row = 1; row <= rows; row++)
            {
                Tile prev = Grid[row - 1, col];
                Tile cur = (row < rows) ? Grid[row, col] : null;
                bool continueRun = prev != null && cur != null && prev.tileColor == cur.tileColor;

                if (!continueRun)
                {
                    int runLength = row - runStart;
                    if (runLength >= 3)
                    {
                        for (int r = runStart; r < row; r++)
                        {
                            verticalMatches.Add(Grid[r, col]);
                        }
                    }
                    runStart = row;
                }
            }
        }

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
                        {
                            horizontalMatches.Add(Grid[row, c]);
                        }
                    }
                    runStart = col;
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
        if (verticalMatches.Count > 0 || horizontalMatches.Count > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove matched tiles marked from the map.
    /// </summary>
    private void RemoveMatchedTiles(List<Tile> matchedTilesList, bool updateMatchCount = false)
    {
        // Collect color that appear in matched lists, also number of times they appear
        Dictionary<TileColor, int> matchedColorCounts = new();

        // Remove based on the vertical and horizontal matches lists
        foreach (Tile tile in matchedTilesList)
        {
            if (Grid[tile.gridPosition.First, tile.gridPosition.Second])
            {
                Grid[tile.gridPosition.First, tile.gridPosition.Second] = null;
            }
            Destroy(tile.gameObject);

            // Count matches for GameManager
            if (!updateMatchCount) continue;
            if (matchedColorCounts.ContainsKey(tile.tileColor))
            {
                matchedColorCounts[tile.tileColor]++;
            }
            else
            {
                matchedColorCounts[tile.tileColor] = 1;
            }
        }

        // Update match counts in GameManager
        if (!updateMatchCount) return;
        foreach (KeyValuePair<TileColor, int> entry in matchedColorCounts)
        {
            // Count is 1 per 3 tiles matched
            GameManager.Instance.AddMatch(entry.Key, entry.Value / 3);
        }
    }

    /// <summary>
    /// Refill the map by instantiating new tiles in empty grid positions at the start.
    /// Cheat by generating new tiles in empty grid positions instead of moving down existing tiles.
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
    /// Refill scheme 1:
    /// </summary>
    private void RefillMap1()
    {
        verticalMatches.Sort((a, b) => a.gridPosition.First.CompareTo(b.gridPosition.First));
        horizontalMatches.Sort((a, b) => a.gridPosition.First.CompareTo(b.gridPosition.First));

        // After moving existing tiles down, fill in new tiles at the top
        foreach (Tile verticalTile in verticalMatches)
        {
            int row = verticalTile.gridPosition.First;
            int col = verticalTile.gridPosition.Second;

            // If spawned then ignore
            if (Grid[row, col] != null) continue;

            Vector3 spawnPos = GridPosToWorldPos(row + rows, col, mapOrigin, cellSpacing);
            Vector3 targetPos = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

            GameObject tileObject = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
            tileObject.name = $"Tile ({row}, {col})";

            Tile tile = tileObject.GetComponent<Tile>();
            TileColor tileColor;

            // The first element in the vertical match is the bottom left tile
            if (row == 0)
            {
                tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
            }
            else
            {
                // check if this tile has another matching tile below it in the verticalMatches list
                Pair<int, int> belowPos = new(row - 1, col);
                if (verticalMatches.Exists(t => t.gridPosition.Equals(belowPos)))
                {
                    // if there is a matching tile below, then this tile has 60% chance to be the same color
                    TileColor belowTileColor = Grid[row - 1, col].tileColor;
                    if (Random.Range(0f, 1f) < sameColorSpawnChance)
                    {
                        tileColor = belowTileColor;
                    }
                    else
                    {
                        List<TileColor> otherColors = new();
                        foreach (TileColor color in System.Enum.GetValues(typeof(TileColor)))
                        {
                            if (color != belowTileColor)
                            {
                                otherColors.Add(color);
                            }
                        }
                        tileColor = otherColors[Random.Range(0, otherColors.Count)];
                    }
                }
                else
                {
                    if (Grid[row - 1, col] == null)
                    {
                        // if there is no tile below, the color is random
                        tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                        tile.Initialize(tileColor, new Pair<int, int>(row, col), targetPos);
                        Grid[row, col] = tile;
                        continue;
                    }
                    TileColor belowTileColor = Grid[row - 1, col].tileColor;
                    // if no tile below, get color from the tile below in the grid with 40% chance, else random from the rest
                    if (Random.Range(0f, 1f) < differentColorSpawnChance)
                    {
                        tileColor = belowTileColor;
                    }
                    else
                    {
                        List<TileColor> otherColors = new();
                        foreach (TileColor color in System.Enum.GetValues(typeof(TileColor)))
                        {
                            if (color != belowTileColor)
                            {
                                otherColors.Add(color);
                            }
                        }
                        tileColor = otherColors[Random.Range(0, otherColors.Count)];
                    }
                }
            }

            tile.Initialize(tileColor, new Pair<int, int>(row, col), targetPos);
            Grid[row, col] = tile;

        }

        foreach (Tile horizontalTile in horizontalMatches)
        {
            int row = horizontalTile.gridPosition.First;
            int col = horizontalTile.gridPosition.Second;

            // If spawned then ignore
            if (Grid[row, col] != null) continue;

            Vector3 spawnPos = GridPosToWorldPos(row + rows, col, mapOrigin, cellSpacing);
            Vector3 targetPos = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

            GameObject tileObject = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
            tileObject.name = $"Tile ({row}, {col})";

            Tile tile = tileObject.GetComponent<Tile>();
            TileColor tileColor;

            // if tile is on the last row the tile color is random
            if (row == 0)
            {
                tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
            }
            else
            {
                // 60% chance to match the tile below it
                TileColor belowTileColor = Grid[row - 1, col].tileColor;
                if (Random.Range(0f, 1f) < differentColorSpawnChance)
                {
                    tileColor = belowTileColor;
                }
                else
                {
                    List<TileColor> otherColors = new();
                    foreach (TileColor color in System.Enum.GetValues(typeof(TileColor)))
                    {
                        if (color != belowTileColor)
                        {
                            otherColors.Add(color);
                        }
                    }
                    tileColor = otherColors[Random.Range(0, otherColors.Count)];
                }
            }

            tile.Initialize(tileColor, new Pair<int, int>(row, col), targetPos);
            Grid[row, col] = tile;
        }
        
    }

    /// <summary>
    /// Refill scheme 2:
    /// </summary>
    private void RefillMap2()
    {
        verticalMatches.Sort((a, b) => a.gridPosition.First.CompareTo(b.gridPosition.First));
        horizontalMatches.Sort((a, b) => a.gridPosition.First.CompareTo(b.gridPosition.First));

        foreach (Tile tile in verticalMatches)
        {
            int row = tile.gridPosition.First;
            int col = tile.gridPosition.Second;

            // If spawned then ignore
            if (Grid[row, col] != null) continue;

            Vector3 spawnPos = GridPosToWorldPos(row + rows, col, mapOrigin, cellSpacing);
            Vector3 targetPos = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

            GameObject tileObject = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
            tileObject.name = $"Tile ({row}, {col})";

            Tile newTile = tileObject.GetComponent<Tile>();
            TileColor tileColor = TileColor.Red;
            // color is determine by all the neighboring tiles, more common color has higher chance
            Dictionary<TileColor, int> neighborColorCount = new();
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <= 1; c++)
                {
                    if (r == 0 && c == 0) continue;
                    int neighborRow = row + r;
                    int neighborCol = col + c;
                    if (neighborRow >= 0 && neighborRow < rows && neighborCol >= 0 && neighborCol < columns)
                    {
                        Tile neighborTile = Grid[neighborRow, neighborCol];
                        if (neighborTile != null)
                        {
                            if (neighborColorCount.ContainsKey(neighborTile.tileColor))
                            {
                                neighborColorCount[neighborTile.tileColor]++;
                            }
                            else
                            {
                                neighborColorCount[neighborTile.tileColor] = 1;
                            }
                        }
                    }
                }
            }

            // Determine the new tile color based on neighbor colors
            if (neighborColorCount.Count > 0)
            {
                int total = neighborColorCount.Values.Sum();
                int randomValue = Random.Range(0, total);
                foreach (var kvp in neighborColorCount)
                {
                    if (randomValue < kvp.Value)
                    {
                        tileColor = kvp.Key;
                        break;
                    }
                    randomValue -= kvp.Value;
                }
            }

            newTile.Initialize(tileColor, new Pair<int, int>(row, col), targetPos);

            Grid[row, col] = newTile;
        }

        foreach (Tile tile in horizontalMatches)
        {
            int row = tile.gridPosition.First;
            int col = tile.gridPosition.Second;

            // If spawned then ignore
            if (Grid[row, col] != null) continue;

            Vector3 spawnPos = GridPosToWorldPos(row + rows, col, mapOrigin, cellSpacing);
            Vector3 targetPos = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

            GameObject tileObject = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
            tileObject.name = $"Tile ({row}, {col})";

            Tile newTile = tileObject.GetComponent<Tile>();
            TileColor tileColor = TileColor.Red;
            // color is determine by all the neighboring tiles, more common color has higher chance
            Dictionary<TileColor, int> neighborColorCount = new();
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <= 1; c++)
                {
                    if (r == 0 && c == 0) continue;
                    int neighborRow = row + r;
                    int neighborCol = col + c;
                    if (neighborRow >= 0 && neighborRow < rows && neighborCol >= 0 && neighborCol < columns)
                    {
                        Tile neighborTile = Grid[neighborRow, neighborCol];
                        if (neighborTile != null)
                        {
                            if (neighborColorCount.ContainsKey(neighborTile.tileColor))
                            {
                                neighborColorCount[neighborTile.tileColor]++;
                            }
                            else
                            {
                                neighborColorCount[neighborTile.tileColor] = 1;
                            }
                        }
                    }
                }
            }

            // Determine the new tile color based on neighbor colors
            if (neighborColorCount.Count > 0)
            {
                int total = neighborColorCount.Values.Sum();
                int randomValue = Random.Range(0, total);
                foreach (var kvp in neighborColorCount)
                {
                    if (randomValue < kvp.Value)
                    {
                        tileColor = kvp.Key;
                        break;
                    }
                    randomValue -= kvp.Value;
                }
            }

            newTile.Initialize(tileColor, new Pair<int, int>(row, col), targetPos);

            Grid[row, col] = newTile;
        }
    }

    /// <summary>
    /// Refill scheme 3:
    /// </summary>
    private void RefillMap3()
    {
        MoveTilesDown();
        
        // After moving existing tiles down, fill in new tiles at the top
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Grid[row, col] == null)
                {
                    Vector3 spawnPos = GridPosToWorldPos(row + rows, col, mapOrigin, cellSpacing);
                    Vector3 targetPos = GridPosToWorldPos(row, col, mapOrigin, cellSpacing);

                    GameObject tileObject = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
                    tileObject.name = $"Tile ({row}, {col})";

                    Tile tile = tileObject.GetComponent<Tile>();
                    TileColor tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                    tile.Initialize(tileColor, new Pair<int, int>(row, col), targetPos);

                    Grid[row, col] = tile;
                }
            }
        }
    }

    private void ClearMatches()
    {
        verticalMatches.Clear();
        horizontalMatches.Clear();
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

        // Update the name of the tiles for easier debugging
        firstTile.gameObject.name = $"Tile ({secondTileGridPos.First}, {secondTileGridPos.Second})";
        secondTile.gameObject.name = $"Tile ({firstTileGridPos.First}, {firstTileGridPos.Second})";

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

            // Update the name of the tiles for easier debugging
            firstTile.gameObject.name = $"Tile ({firstTileGridPos.First}, {firstTileGridPos.Second})";
            secondTile.gameObject.name = $"Tile ({secondTileGridPos.First}, {secondTileGridPos.Second})";
        }
        else
        {
            OnResolveMatches.Invoke();
        }
    }

    // Handle the OnResolveMatches event
    private void HandleResolveMatches()
    {
        float delay = 1f;
        StartCoroutine(ResolveMatchesCoroutine(delay));
    }
    
    /// <summary>
    /// Coroutine to resolve matches with delay
    /// </summary>
    /// <param name="delay">Delay between each refill</param>
    /// <returns></returns>
    private IEnumerator ResolveMatchesCoroutine(float delay = 1.0f)
    {
        CheckMatches();

        // if there are no matches, exit
        if (!HasMatches())
        {
            GenerateNewMap();
            yield break;
        }
        else
        {
            do
            {
                RemoveMatchedTiles(verticalMatches, true);
                RemoveMatchedTiles(horizontalMatches, true);

                // check if current scene is level 1 or level 2
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level1")
                {
                    RefillMap1();
                }
                else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level2")
                {
                    RefillMap2();
                }
                else
                {
                    RefillMap3();
                }

                AddScore();
                CheckMatches();

                yield return new WaitForSeconds(delay);
            }
            while (HasMatches());
        }

        // Game management updates
        GameManager.Instance.UseMove();
        GameManager.Instance.ResetMultiplier();
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
    private void GenerateNewMap()
    {
        BuildMap();
        CheckMatches();
        while (HasMatches())
        {
            RemoveMatchedTiles(verticalMatches);
            RemoveMatchedTiles(horizontalMatches);
            RefillMapStart();
            CheckMatches();
        }
    }
    
    #endregion
}
