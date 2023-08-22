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

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Coord)
            {
                Coord c = (Coord)obj;
                return this == c;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return x ^ y;
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

    [SerializeField][Range(0, 1)]
    private float _obstaclePercent;

    [SerializeField]
    private int _seed;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;

    private Coord _playerSpawnCoord;

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

        _playerSpawnCoord = new Coord((int)_mapSize.x / 2, (int)_mapSize.y / 2);

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
        int obstacleCount = Mathf.RoundToInt(_mapSize.x * _mapSize.y * _obstaclePercent);
        bool[,] obstacleMap = new bool[(int)_mapSize.x, (int)_mapSize.y];
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != _playerSpawnCoord && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(_obstaclePrefab, obstaclePosition + Vector3.up * (0.5f - _outlinePercent * 0.5f), Quaternion.identity) as Transform;
                newObstacle.localScale = Vector3.one * (1 - _outlinePercent);
                newObstacle.parent = mapHolder.transform;
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }            
        }
    }

    private bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(_playerSpawnCoord);
        mapFlags[_playerSpawnCoord.x, _playerSpawnCoord.y] = true;
        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = tile.x - 1; x <= tile.x + 1; x++)
            {
                for (int y = tile.y - 1; y <= tile.y + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.y || x == tile.x))
                    {
                        if (!mapFlags[x, y] && !obstacleMap[x, y])
                        {
                            mapFlags[x, y] = true;
                            queue.Enqueue(new Coord(x, y));
                            accessibleTileCount++;
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(_mapSize.x * _mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    private bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < _mapSize.x && y >= 0 && y < _mapSize.y;
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
