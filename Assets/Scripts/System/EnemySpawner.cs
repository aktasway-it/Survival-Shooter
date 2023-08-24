using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
        public float spawnDelay;
        public Enemy enemyPrefab;
    }

    [SerializeField]
    private Wave[] _waves;

    private Wave _currentWave;
    private int _currentWaveIndex;
    private int _enemyRemainingToSpawn;
    private int _enemyRemainingAlive;
    private float _timeToNextSpawn;

    public void StartSpawning()
    {
        NextWave();
    }

    private void Update()
    {
        if (GameManager.Instance.Player.IsAlive && _enemyRemainingToSpawn > 0 && Time.time >= _timeToNextSpawn)
        {
            StartCoroutine(SpawnEnemy());
        }
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
        spawnedEnemy.onDeath += OnEnemyDeath;
        spawnTile.SetBlinkAnimationRunning(false);
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
