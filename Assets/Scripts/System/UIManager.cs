using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : SingletonBehavior<UIManager>
{
    [SerializeField]
    private GameObject _mainMenu;

    [SerializeField]
    private CanvasGroup _gameOverCanvasGroup;

    [SerializeField]
    private NewWaveBanner _newWaveBanner;

    [SerializeField]
    private TextMeshProUGUI _playerScoreText;
    
    [SerializeField]
    private PlayerHealthBar _playerHealthBar;

    private void Start()
    {
        if (PlayerPrefs.GetInt("Replay") == 1)
        {
            PlayerPrefs.SetInt("Replay", 0);
            OnPlayButtonClicked();
        }
        else
        {
            _mainMenu.SetActive(true);
        }
    }

    private void OnEnable()
    {
        EnemySpawner.OnNewWave += OnNewWave;
        GameManager.OnScoreUpdated += OnScoreUpdated;
    }

    private void OnDisable()
    {
        EnemySpawner.OnNewWave -= OnNewWave;
        GameManager.OnScoreUpdated -= OnScoreUpdated;
    }

    private void OnScoreUpdated(int score)
    {
        _playerScoreText.text = score.ToString("D8");
    }

    private void OnNewWave(int waveIndex)
    {
        _newWaveBanner.Show(waveIndex, EnemySpawner.Instance.CurrentWaveEnemyCount);
    }

    public void ShowGameOverScreen()
    {
        StartCoroutine(ShowGameOverScreenCoroutine());
    }

    public void OnPlayButtonClicked()
    {
        _mainMenu.SetActive(false);
        GameManager.Instance.StartGame();
        _playerScoreText.gameObject.SetActive(true);
        _playerHealthBar.AttachPlayer(GameManager.Instance.Player);
    }

    public void OnRestartButtonClicked()
    {
        PlayerPrefs.SetInt("Replay", 1);
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
