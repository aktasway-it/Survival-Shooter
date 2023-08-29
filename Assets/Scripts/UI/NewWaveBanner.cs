using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewWaveBanner : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _waveNumberText;

    [SerializeField]
    private TextMeshProUGUI _enemyCountText;

    private RectTransform _rectTransform;
    private Animator _animator;

    private string[] _waveNumberStrings = { "One", "Two", "Three", "Four" };
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _animator = GetComponent<Animator>();
    }

    public void Show(int waveNumber, int enemyCount)
    {
        string enemyCountString = enemyCount > 0 ? enemyCount.ToString() : "Infinite";
        _waveNumberText.SetText("- Wave " + _waveNumberStrings[waveNumber] + " -");
        _enemyCountText.SetText("Enemies: " + enemyCountString);
        _animator.SetTrigger("Show");
    }
}
