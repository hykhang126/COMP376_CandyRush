using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Map size")]
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private Transform mapOrigin;

    [Header("Prefabs")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSpacing = 0;
    [SerializeField] private GameObject tilePrefab;

    public Tile[,] grid;
    public bool[,] matched;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateNewMap();

        RemoveMatchedTiles();
    }

    #region Public Methods

    public void GenerateNewMap()
    {
        ClearMap();
        BuildMap();
        CheckMatches();
    }

    #endregion

    #region Private Methods

    private void BuildMap()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 cellPosition = mapOrigin.position + new Vector3(
                    column * (1 + cellSpacing),
                    row * (1 + cellSpacing),
                    0
                );

                GameObject cellObject = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform);
                cellObject.name = $"Cell ({row}, {column})";

                Vector3 tilePosition = cellPosition;
                GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, cellObject.transform);
                Tile tile = tileObject.GetComponent<Tile>();
                tileObject.name = $"Tile ({row}, {column})";

                tile.tileColor
                    = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                
                grid[row, column] = tile;
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
        Tile[,] grid = new Tile[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int childIndex = row * columns + column;
                if (childIndex < transform.childCount)
                {
                    Transform cell = transform.GetChild(childIndex);
                    Tile tile = null;
                    if (cell.childCount > 0)
                    {
                        Transform tileTransform = cell.GetChild(0);
                        tile = tileTransform ? tileTransform.GetComponent<Tile>() : null;
                    }
                    grid[row, column] = tile;
                }
            }
        }

        // matched flags
        bool[,] matched = new bool[rows, columns];

        // check horizontal matches
        for (int row = 0; row < rows; row++)
        {
            int runStart = 0;
            for (int col = 1; col <= columns; col++)
            {
                Tile prev = grid[row, col - 1];
                Tile cur = (col < columns) ? grid[row, col] : null;
                bool continueRun = (prev != null && cur != null && prev.tileColor == cur.tileColor);

                if (!continueRun)
                {
                    int runLength = col - runStart;
                    if (runLength >= 3)
                    {
                        for (int c = runStart; c < col; c++)
                            matched[row, c] = true;
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
                Tile prev = grid[row - 1, col];
                Tile cur = (row < rows) ? grid[row, col] : null;
                bool continueRun = (prev != null && cur != null && prev.tileColor == cur.tileColor);

                if (!continueRun)
                {
                    int runLength = row - runStart;
                    if (runLength >= 3)
                    {
                        for (int r = runStart; r < row; r++)
                            matched[r, col] = true;
                    }
                    runStart = row;
                }
            }
        }

        // report / mark matched tiles (scale up slightly as a visual marker)
        bool anyMatches = false;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (matched[row, col])
                {
                    anyMatches = true;
                    Tile t = grid[row, col];
                    if (t != null && t.gameObject != null)
                    {
                        t.gameObject.transform.localScale = Vector3.one * 1.2f;
                        Debug.Log($"Match found at ({row}, {col}) color={t.tileColor}");
                    }
                }
            }
        }

        if (!anyMatches)
            Debug.Log("No matches found.");
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
                if (matched[row, col])
                {
                    Destroy(grid[row, col].gameObject);
                }
            }
        }
    }

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
    }
    #endregion
}
