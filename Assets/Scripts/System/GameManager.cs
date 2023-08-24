using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Player Player { get; private set; }

    [SerializeField]
    private Player _playerPrefab;

    [SerializeField]
    private MapGenerator _mapGenerator;

    [SerializeField]
    private EnemySpawner _enemySpawner;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _mapGenerator.GenerateMap();
        Player = Instantiate(_playerPrefab, _mapGenerator.GetRandomTile(MapTile.Type.PlayerSpawn).transform.position, Quaternion.identity);
        Player.onDeath += OnPlayerDeath;

        _enemySpawner.StartSpawning();
    }

    private void OnPlayerDeath()
    {
    }
}
