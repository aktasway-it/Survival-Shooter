using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile
{
    public enum Type
    {
        Empty,
        Obstacle,
        PlayerSpawn,
        EnemySpawn
    }

    public Type TileType { get; set; }
    public readonly Transform transform;

    public MapTile(Type type, Transform transform)
    {
        this.TileType = type;
        this.transform = transform;
    }
}
