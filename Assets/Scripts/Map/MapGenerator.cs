using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.AI.Navigation;
using UnityEngine;

using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

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

    [SerializeField]
    private CinemachineVirtualCamera _cinemachineVirtualCamera;

    private Dictionary<Coord, MapTile> _tileMap;
    private Queue<Coord> _shuffledTileCoords;

    private Map _currentMap;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Generate a new map and destroy old one if it exists
        _currentMap = _maps[_currentMapIndex];
        Random random = new Random(_maps[_currentMapIndex].seed);

        Transform oldMap = transform.Find("Generated Map");
        if (oldMap != null)
        {
            DestroyImmediate(oldMap.gameObject);
        }

        _tileMap = new Dictionary<Coord, MapTile>();
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

                MapTile mapTile = new MapTile(MapTile.Type.Empty, newTile);
                _tileMap.Add(new Coord(x, y), mapTile);
            }
        }

        // Set player spawn tile
        _tileMap[_currentMap.PlayerSpawnCoord].TileType = MapTile.Type.PlayerSpawn;

        // Generate obstacles
        _shuffledTileCoords = new Queue<Coord>(Utility.ShuffleList(_tileMap.Keys.ToList(), _currentMap.seed));
        int obstacleCount = Mathf.RoundToInt(_currentMap.mapSize.x * _currentMap.mapSize.y * _currentMap.obstaclePercent);

        for (int i = 0; i < obstacleCount; i++)
        {
            // Get a random tile where to place an obstacle
            MapTile randomTile = GetRandomTile(MapTile.Type.Empty);
            randomTile.TileType = MapTile.Type.Obstacle;

            // If the random tile is not the player spawn tile and the map is fully accessible, place an obstacle
            if (IsMapFullyAccessible())
            {
                Vector3 obstaclePosition = randomTile.transform.position;
                float obstacleHeight = Mathf.Lerp(_currentMap.minObstacleHeight, _currentMap.maxObstacleHeight, (float) random.NextDouble());
                Transform newObstacle = Instantiate(_obstaclePrefab, obstaclePosition + Vector3.up * (obstacleHeight * 0.5f - _outlinePercent * 0.5f * _tileSize), Quaternion.identity) as Transform;
                
                float obstacleXZ = (1 - _outlinePercent) * _tileSize;
                newObstacle.localScale = new Vector3(obstacleXZ, obstacleHeight, obstacleXZ);
                newObstacle.parent = mapHolder.transform;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                Coord obstacleCoord = PositionToCoord(obstaclePosition);
                float colorPercent = obstacleCoord.y / (float) _currentMap.mapSize.y;

                obstacleMaterial.color = Color.Lerp(_currentMap.foregroundColor, _currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;
            }
            // Otherwise, remove the obstacle
            else
            {
                randomTile.TileType = MapTile.Type.Empty;
            }            
        }

        // Build nav mesh for enemies
        _navMeshSurface.transform.localScale = new Vector3(_currentMap.mapSize.x * _tileSize, 1, _currentMap.mapSize.y * _tileSize);
        _navMeshSurface.BuildNavMesh();

        // Set camera distance
        CinemachineFramingTransposer transposer = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_CameraDistance = _currentMap.mapSize.x * _tileSize * 0.5f;
    }

    /**
     * Check if the map is fully accessible by the player using a flood fill algorithm
     */
    private bool IsMapFullyAccessible()
    {
        bool[,] mapFlags = new bool[_currentMap.mapSize.x, _currentMap.mapSize.y];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(_currentMap.PlayerSpawnCoord);

        mapFlags[_currentMap.PlayerSpawnCoord.x, _currentMap.PlayerSpawnCoord.y] = true;
        Coord currentTileCoord = _currentMap.PlayerSpawnCoord;
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
                        currentTileCoord.x = x;
                        currentTileCoord.y = y;

                        if (!mapFlags[x, y] && _tileMap[currentTileCoord].TileType != MapTile.Type.Obstacle)
                        {
                            mapFlags[x, y] = true;
                            queue.Enqueue(new Coord(x, y));
                            accessibleTileCount++;
                        }
                    }
                }
            }
        }

        int obstacleCount = _tileMap.Count(tile => tile.Value.TileType == MapTile.Type.Obstacle);
        int targetAccessibleTileCount = (int)(_currentMap.mapSize.x * _currentMap.mapSize.y - obstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    private bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < _currentMap.mapSize.x && y >= 0 && y < _currentMap.mapSize.y;
    }

    private Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-_currentMap.mapSize.x / 2f + 0.5f + x, 0, -_currentMap.mapSize.y / 2f + 0.5f + y) * _tileSize;
    }

    private Coord PositionToCoord(Vector3 position)
    {
        return new Coord(Mathf.RoundToInt(position.x / _tileSize + (_currentMap.mapSize.x - 1) / 2f), Mathf.RoundToInt(position.z / _tileSize + (_currentMap.mapSize.y - 1) / 2f));
    }

    public MapTile GetRandomTile(MapTile.Type type)
    {
        while (true)
        {
            Coord randomCoord = GetRandomCoord();
            if (_tileMap[randomCoord].TileType == type)
            {
                return _tileMap[randomCoord];
            }
        }
    }

    private Coord GetRandomCoord()
    {
        Coord randomCoord = _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }
}
