using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : SingletonBehavior<UIManager>
{
    [SerializeField]
    private CanvasGroup _gameOverCanvasGroup;

    [SerializeField]
    private NewWaveBanner _newWaveBanner;

    private void OnEnable() {
        EnemySpawner.OnNewWave += OnNewWave;
    }

    private void OnDisable() {
        EnemySpawner.OnNewWave -= OnNewWave;
    }

    private void OnNewWave(int waveIndex)
    {
        _newWaveBanner.Show(waveIndex, EnemySpawner.Instance.CurrentWaveEnemyCount);
    }

    public void ShowGameOverScreen()
    {
        StartCoroutine(ShowGameOverScreenCoroutine());
    }

    public void OnRestartButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ShowGameOverScreenCoroutine()
    {
        yield return new WaitForSeconds(2);
        _gameOverCanvasGroup.gameObject.SetActive(true);
        _gameOverCanvasGroup.interactable = true;
        _gameOverCanvasGroup.blocksRaycasts = true;

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime;
            _gameOverCanvasGroup.alpha = alpha;
            yield return null;
        }
    }
}
