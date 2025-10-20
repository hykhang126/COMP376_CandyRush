using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Physics2DRaycaster), typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    #region Events

    public UnityEvent<Tile, Tile> OnTileSwapped;

    #endregion

    #region Singleton

    // SINGLETON
    public static InputManager Instance { get; private set; }
    // -----------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    #region Code

    public Tile firstClickedTile;
    public Tile secondClickedTile;

    private void Start()
    {
        firstClickedTile = null;
        secondClickedTile = null;
    }

    public void UpdateClickedTiles(Tile clickedTile)
    {
        // Deselect tile if clicked again
        if (clickedTile == firstClickedTile)
        {
            clickedTile.transform.localScale = Vector3.one * 0.9f;
            firstClickedTile = null;
            return;
        }

        if (firstClickedTile == null)
        {
            firstClickedTile = clickedTile;
        }
        else if (secondClickedTile == null && clickedTile != firstClickedTile)
        {
            secondClickedTile = clickedTile;
            CheckCanSwapTiles();
        }
        else
        {
            Debug.Log("Both clicked tiles are already set. Resetting...");
            firstClickedTile = clickedTile;
            secondClickedTile = null;
        }
    }

    private void CheckCanSwapTiles()
    {
        // Reset scale of all tiles to indicate deselection
        firstClickedTile.transform.localScale = Vector3.one * 0.9f;
        secondClickedTile.transform.localScale = Vector3.one * 0.9f;

        // Check if the two tiles are adjacent
        int rowDiff = Mathf.Abs(firstClickedTile.gridPosition.First - secondClickedTile.gridPosition.First);
        int colDiff = Mathf.Abs(firstClickedTile.gridPosition.Second - secondClickedTile.gridPosition.Second);

        if ((rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1))
        {
            OnTileSwapped?.Invoke(firstClickedTile, secondClickedTile);
        }
        else
        {
            Debug.Log("Tiles are not adjacent. Cannot swap.");
        }

        // Reset clicked tiles after checking
        firstClickedTile = null;
        secondClickedTile = null;
    }

    #endregion
}
