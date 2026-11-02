using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class IsometricGrid : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("Grid Size")]
    [SerializeField] int _columns = 30;
    [SerializeField] int _rows = 30;

    [Header("Floor Tile")]
    [SerializeField] TileBase _floorTile;

    [SerializeField] Color _whiteColor = Color.white;
    [SerializeField] Color _blueColor = Color.skyBlue;


    // Methods

    void Awake()
    {
        if (_floorTile == null)
            return;

        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.ClearAllTiles();

        BoundsInt area = new BoundsInt(0, 0, 0, _columns, _rows, 1);
        TileBase[] tiles = new TileBase[_columns * _rows];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = _floorTile;
        }

        tilemap.SetTilesBlock(area, tiles);

        for (int x = 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                tilemap.SetTileFlags(cell, TileFlags.None);

                bool isWhite = (x + y) % 2 == 0;

                tilemap.SetColor(cell, isWhite ? _whiteColor : _blueColor);
            }
        }
    }
}
