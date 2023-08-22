using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public struct Coord 
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [SerializeField]
    private Transform _tilePrefab;

    [SerializeField]
    private Transform _obstaclePrefab;

    [SerializeField]
    private Vector2 _mapSize;

    [SerializeField][Range(0, 1)]
    private float _outlinePercent;

    [SerializeField]
    private int _seed;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        _allTileCoords = new List<Coord>();
        GameObject mapHolder = new GameObject("Generated Map");
        mapHolder.transform.parent = transform;

        for (int x = 0; x < _mapSize.x; x++)
        {
            for (int y = 0; y < _mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(_tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - _outlinePercent);
                newTile.parent = mapHolder.transform;

                _allTileCoords.Add(new Coord(x, y));
            }
        }

        _shuffledTileCoords = new Queue<Coord>(Utility.ShuffleList(_allTileCoords.ToArray(), _seed));
        int obstacleCount = 10;
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
            Transform newObstacle = Instantiate(_obstaclePrefab, obstaclePosition + Vector3.up * (0.5f - _outlinePercent * 0.5f), Quaternion.identity) as Transform;
            newObstacle.localScale = Vector3.one * (1 - _outlinePercent);
            newObstacle.parent = mapHolder.transform;
        }
    }

    public Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-_mapSize.x / 2 + 0.5f + x, 0, -_mapSize.y / 2 + 0.5f + y);
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

}
