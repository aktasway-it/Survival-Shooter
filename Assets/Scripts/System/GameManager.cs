using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : SingletonBehavior<GameManager>
{
    public Player Player { get; private set; }

    [SerializeField]
    private Player _playerPrefab;

    [SerializeField]
    private CinemachineVirtualCamera _cinemachineVirtualCamera;

    void Start()
    {
        MapGenerator.Instance.GenerateMap();
        Player = Instantiate(_playerPrefab, MapGenerator.Instance.GetRandomTile(MapTile.Type.PlayerSpawn).transform.position, Quaternion.identity);
        Player.OnDeath += OnPlayerDeath;

        // Set camera distance
        CinemachineFramingTransposer transposer = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_CameraDistance = MapGenerator.Instance.WorldSize.x * 0.5f;
        _cinemachineVirtualCamera.Follow = Player.transform;
        _cinemachineVirtualCamera.LookAt = Player.transform;

        EnemySpawner.Instance.StartSpawning();
        EnemySpawner.OnNewWave += OnNewWave;
    }

    private void OnNewWave(int waveIndex)
    {
        MapGenerator.Instance.GenerateMap(waveIndex);
        Player.transform.position = MapGenerator.Instance.GetRandomTile(MapTile.Type.PlayerSpawn).transform.position;
    }

    private void OnPlayerDeath()
    {
        Player.OnDeath -= OnPlayerDeath;
    }
}
