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
    [Header("Tile Settings")]
    public TileColor tileColor;
    public Pair<int, int> gridPosition;

    [Header("Movement Settings")]
    public Vector3 targetPosition;
    public float speed = 10f;

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
        MoveToPosition();
    }

    public void Initialize(TileColor color, Pair<int, int> position, Vector3 worldPosition)
    {
        tileColor = color;
        gridPosition = position;
        targetPosition = worldPosition;
        transform.position = worldPosition;
    }

    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    private void MoveToPosition()
    {
        if (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
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
        InputManager.Instance.UpdateClickedTiles(this);
    }
    
    #endregion
}

public class Pair<T1, T2>
{
    public T1 First { get; set; }
    public T2 Second { get; set; }

    public Pair(T1 first, T2 second)
    {
        First = first;
        Second = second;
    }
}