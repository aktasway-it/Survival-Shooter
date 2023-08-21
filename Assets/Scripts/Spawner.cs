using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
        public Enemy enemyPrefab;
    }

    [SerializeField]
    private Wave[] _waves;

    private Wave _currentWave;
    private int _currentWaveIndex;
    private int _enemyRemainingToSpawn;
    private int _enemyRemainingAlive;
    private float _timeToNextSpawn;

    private void Start()
    {
        NextWave();
    }

    private void Update()
    {
        if (_enemyRemainingToSpawn > 0 && Time.time >= _timeToNextSpawn)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        _enemyRemainingToSpawn--;
        _timeToNextSpawn = Time.time + _currentWave.timeBetweenSpawns;

        Enemy spawnedEnemy = Instantiate(_currentWave.enemyPrefab, transform.position, transform.rotation);
        spawnedEnemy.onDeath += OnEnemyDeath;
    }

    private void NextWave()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex - 1 < _waves.Length)
        {
            Debug.Log("Next Wave: " + _currentWaveIndex);
            _currentWave = _waves[_currentWaveIndex - 1];
            _enemyRemainingToSpawn = _currentWave.enemyCount;
            _enemyRemainingAlive = _enemyRemainingToSpawn;
            _timeToNextSpawn = 0;
        }
    }

    private void OnEnemyDeath()
    {
        _enemyRemainingAlive--;
        if (_enemyRemainingAlive == 0)
        {
            NextWave();
        }
    }
}
