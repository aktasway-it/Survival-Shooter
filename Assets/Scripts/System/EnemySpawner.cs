using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : SingletonBehavior<EnemySpawner>
{
    public static event Action<int> OnNewWave;

    [Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
        public float spawnDelay;
        public Enemy enemyPrefab;
    }

    public int CurrentWaveEnemyCount => _currentWave.enemyCount;

    [SerializeField]
    private Wave[] _waves;

    [SerializeField]
    private AudioClip _waveCompletedSfx;

    private Wave _currentWave;
    private int _currentWaveIndex = -1;
    private int _enemyRemainingToSpawn;
    private int _enemyRemainingAlive;
    private float _timeToNextSpawn;

    public void StartSpawning()
    {
        NextWave();
    }

    private void Update()
    {
        if (ShouldSpawn())
        {
            StartCoroutine(SpawnEnemy());
        }
    }

    private bool ShouldSpawn()
    {
        bool shouldSpawn = GameManager.Instance.Player.IsAlive;
        shouldSpawn &= _enemyRemainingToSpawn > 0 || _currentWave.enemyCount == 0;
        shouldSpawn &= Time.time >= _timeToNextSpawn;
        return shouldSpawn;
    }

    private IEnumerator SpawnEnemy()
    {
        _enemyRemainingToSpawn--;
        _timeToNextSpawn = Time.time + _currentWave.timeBetweenSpawns + _currentWave.spawnDelay;

        bool isPlayerCamping = GameManager.Instance.Player.IsCamping;
        MapTile spawnTile = isPlayerCamping ?
            MapGenerator.Instance.GetTile(GameManager.Instance.Player.transform.position) :
            MapGenerator.Instance.GetRandomTile(MapTile.Type.Empty);
        
        spawnTile.SetBlinkAnimationRunning(true);

        float spawnDelay = _currentWave.spawnDelay;
        while (spawnDelay > 0)
        {
            spawnDelay -= Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(_currentWave.enemyPrefab, spawnTile.transform.position, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnTile.SetBlinkAnimationRunning(false);
    }

    private void NextWave()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex < _waves.Length)
        {
            Debug.Log("Next Wave: " + _currentWaveIndex);
            _currentWave = _waves[_currentWaveIndex];
            _enemyRemainingToSpawn = _currentWave.enemyCount;
            _enemyRemainingAlive = _enemyRemainingToSpawn;
            _timeToNextSpawn = 0;

            OnNewWave?.Invoke(_currentWaveIndex);
        }
    }

    private void OnEnemyDeath()
    {
        _enemyRemainingAlive--;
        if (_enemyRemainingAlive == 0)
        {
            StartCoroutine(NextWaveCoroutine());
        }
    }

    private IEnumerator NextWaveCoroutine()
    {
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.Play(_waveCompletedSfx);
        yield return new WaitForSeconds(2f);
        NextWave();
    }
}
