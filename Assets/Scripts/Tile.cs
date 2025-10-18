using UnityEngine;
using UnityEngine.EventSystems;

public enum TileColor
{
    Red,
    Blue,
    Purple,
    Green,
    Yellow
}

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Tile : MonoBehaviour, IPointerClickHandler
{
    public TileColor tileColor;

    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeTileColor(this, tileColor);
    }

    public static void ChangeTileColor(Tile tile, TileColor color)
    {
        switch (color)
        {
            case TileColor.Red:
                tile.spriteRenderer.color = Color.red;
                break;
            case TileColor.Blue:
                tile.spriteRenderer.color = Color.blue;
                break;
            case TileColor.Purple:
                tile.spriteRenderer.color = new Color(0.5f, 0f, 0.5f);
                break;
            case TileColor.Green:
                tile.spriteRenderer.color = Color.green;
                break;
            case TileColor.Yellow:
                tile.spriteRenderer.color = Color.yellow;
                break;
        }
    }

    #region Events Handlers
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{tileColor} clicked at {transform.position}.");
    }
    
    #endregion
}
