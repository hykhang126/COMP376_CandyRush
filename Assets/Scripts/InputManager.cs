using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Physics2DRaycaster))]
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    #region Code

    public Tile firstClickedTile;
    public Tile secondClickedTile;

    private PlayerInput playerInput;

    void Start()
    {
        firstClickedTile = null;
        secondClickedTile = null;

        playerInput = GetComponent<PlayerInput>();

        playerInput.actions.Enable();
        playerInput.actions["Pause"].started += _ => GameManager.Instance.OnGamePause?.Invoke();
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
            GameManager.Instance.OnTileSwapped?.Invoke(firstClickedTile, secondClickedTile);
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
