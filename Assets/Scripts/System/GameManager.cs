using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : SingletonBehavior<GameManager>
{
    public static event Action OnGameStarted;
    public static event Action<int> OnScoreUpdated;
    public Player Player { get; private set; }
    public int Score { get; private set; }

    [SerializeField]
    private Player _playerPrefab;

    [SerializeField]
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private GameObject _audioListener;

    void Start()
    {
        _audioListener = new GameObject("Audio Listener");
        _audioListener.AddComponent<AudioListener>();

        AudioManager.Instance.PlayThemeMusic();

        MapGenerator.Instance.GenerateMap(0);
        // Set camera distance
        CinemachineFramingTransposer transposer = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_CameraDistance = MapGenerator.Instance.WorldSize.x * 0.5f;
    }

    private void OnNewWave(int waveIndex)
    {
        MapGenerator.Instance.GenerateMap(waveIndex);
        Player.transform.position = MapGenerator.Instance.GetRandomTile(MapTile.Type.PlayerSpawn).transform.position;
    }

    private void OnPlayerDeath()
    {
        _audioListener.transform.SetParent(null);

        Player.OnDeath -= OnPlayerDeath;
        EnemySpawner.OnNewWave -= OnNewWave;

        UIManager.Instance.ShowGameOverScreen();
        Cursor.visible = true;

        AudioManager.Instance.PlayThemeMusic();
    }

    public void StartGame()
    {
        Cursor.visible = false;
        
        Player = Instantiate(_playerPrefab, MapGenerator.Instance.GetRandomTile(MapTile.Type.PlayerSpawn).transform.position, Quaternion.identity);
        Player.OnDeath += OnPlayerDeath;

        _audioListener.transform.position = Player.transform.position;
        _audioListener.transform.SetParent(Player.transform);

        _cinemachineVirtualCamera.Follow = Player.transform;
        _cinemachineVirtualCamera.LookAt = Player.transform;

        EnemySpawner.Instance.StartSpawning();
        EnemySpawner.OnNewWave += OnNewWave;
        EnemySpawner.OnEnemyDeath += OnEnemyDeath;

        AudioManager.Instance.PlayGameMusic();
        OnGameStarted?.Invoke();
    }

    private void OnEnemyDeath()
    {
        Score += (EnemySpawner.Instance.CurrentWaveIndex + 1) * 40;
        OnScoreUpdated?.Invoke(Score);
    }
}
