using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
