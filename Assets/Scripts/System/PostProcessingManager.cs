using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingManager : SingletonBehavior<PostProcessingManager>
{
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;

    protected override void Awake()
    {
        base.Awake();

        PostProcessVolume postProcessVolume = GetComponent<PostProcessVolume>();
        postProcessVolume.profile.TryGetSettings(out _chromaticAberration);
        postProcessVolume.profile.TryGetSettings(out _vignette);
    }

    private void OnEnable()
    {
        GameManager.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        GameManager.Instance.Player.OnHealthChanged += OnHealthChanged;
        GameManager.Instance.Player.OnDeath += OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        GameManager.Instance.Player.OnHealthChanged -= OnHealthChanged;
        GameManager.Instance.Player.OnDeath -= OnPlayerDeath;
    }

    private void OnHealthChanged(float health)
    {
        float normalizedHealth = health / GameManager.Instance.Player.MaxHealth;
        _chromaticAberration.intensity.value = Mathf.Lerp(0, 1, 1 - normalizedHealth);
        _vignette.intensity.value = Mathf.Lerp(0, 0.6f, 1 - normalizedHealth);
    }
}
