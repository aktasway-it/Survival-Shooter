using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
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

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;
        
        public Coord PlayerSpawnCoord => new Coord(mapSize.x / 2, mapSize.y / 2);
    }

    [SerializeField]
    private Transform _tilePrefab;

    [SerializeField]
    private Transform _obstaclePrefab;

    [SerializeField]
    private float _tileSize = 1;

    [SerializeField][Range(0, 1)]
    private float _outlinePercent;

    [SerializeField]
    private NavMeshSurface _navMeshSurface;

    [SerializeField]
    private Map[] _maps;

    [SerializeField]
    private int _currentMapIndex;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;

    private Map _currentMap;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        _currentMap = _maps[_currentMapIndex];
        Random random = new Random(_maps[_currentMapIndex].seed);

        Transform oldMap = transform.Find("Generated Map");
        if (oldMap != null)
        {
            DestroyImmediate(oldMap.gameObject);
        }

        _allTileCoords = new List<Coord>();
        GameObject mapHolder = new GameObject("Generated Map");
        mapHolder.transform.parent = transform;

        for (int x = 0; x < _currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < _currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(_tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - _outlinePercent) * _tileSize;
                newTile.parent = mapHolder.transform;

                _allTileCoords.Add(new Coord(x, y));
            }
        }

        _shuffledTileCoords = new Queue<Coord>(Utility.ShuffleList(_allTileCoords.ToArray(), _currentMap.seed));
        int obstacleCount = Mathf.RoundToInt(_currentMap.mapSize.x * _currentMap.mapSize.y * _currentMap.obstaclePercent);
        bool[,] obstacleMap = new bool[_currentMap.mapSize.x, _currentMap.mapSize.y];
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != _currentMap.PlayerSpawnCoord && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                float obstacleHeight = Mathf.Lerp(_currentMap.minObstacleHeight, _currentMap.maxObstacleHeight, (float) random.NextDouble());
                Transform newObstacle = Instantiate(_obstaclePrefab, obstaclePosition + Vector3.up * (obstacleHeight * 0.5f - _outlinePercent * 0.5f * _tileSize), Quaternion.identity) as Transform;
                float obstacleXZ = (1 - _outlinePercent) * _tileSize;
                newObstacle.localScale = new Vector3(obstacleXZ, obstacleHeight, obstacleXZ);
                newObstacle.parent = mapHolder.transform;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float) _currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(_currentMap.foregroundColor, _currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }            
        }

        _navMeshSurface.transform.localScale = new Vector3(_currentMap.mapSize.x * _tileSize, 1, _currentMap.mapSize.y * _tileSize);
        _navMeshSurface.BuildNavMesh();

        Camera.main.orthographicSize = Mathf.Max(_currentMap.mapSize.x, _currentMap.mapSize.y) / 2f * _tileSize;
    }

    private bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(_currentMap.PlayerSpawnCoord);
        mapFlags[_currentMap.PlayerSpawnCoord.x, _currentMap.PlayerSpawnCoord.y] = true;
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

        int targetAccessibleTileCount = (int)(_currentMap.mapSize.x * _currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    private bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < _currentMap.mapSize.x && y >= 0 && y < _currentMap.mapSize.y;
    }

    public Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-_currentMap.mapSize.x / 2f + 0.5f + x, 0, -_currentMap.mapSize.y / 2f + 0.5f + y) * _tileSize;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

}
