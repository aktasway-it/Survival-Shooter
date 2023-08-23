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
    private Animator _animator;

    public MapTile(Type type, Transform transform)
    {
        this.TileType = type;
        this.transform = transform;
        _animator = transform.GetComponent<Animator>();
    }

    public void SetBlinkAnimationRunning(bool isRunning)
    {
        _animator.SetBool("Blinking", isRunning);
    }
}
