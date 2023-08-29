using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider _healthBar;

    private Player _player;
    public void AttachPlayer(Player player)
    {
        gameObject.SetActive(true);
        _player = player;
        _player.OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float health)
    {
        _healthBar.value = health / _player.MaxHealth;

        if (health <= 0)
        {
            _player.OnHealthChanged -= OnHealthChanged;
            gameObject.SetActive(false);
        }
    }
}
