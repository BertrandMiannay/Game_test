using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Events ──────────────────────────────────────────────────────────────
    public static event Action<float, float> OnHealthChanged;
    public static event Action<int, int>     OnAmmoChanged;
    public static event Action<WeaponBase>   OnWeaponChanged;
    public static event Action<int>          OnEnemyKilled;
    public static event Action               OnGameOver;
    public static event Action               OnVictory;

    // ── State ────────────────────────────────────────────────────────────────
    public int  EnemiesRemaining { get; private set; }
    public bool GameEnded        { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Enemy management ────────────────────────────────────────────────────
    public void RegisterEnemy()
    {
        EnemiesRemaining++;
    }

    public void NotifyEnemyKilled()
    {
        EnemiesRemaining = Mathf.Max(0, EnemiesRemaining - 1);
        OnEnemyKilled?.Invoke(EnemiesRemaining);
    }

    // ── Game flow ────────────────────────────────────────────────────────────
    public void PlayerDied()
    {
        if (GameEnded) return;
        GameEnded = true;
        OnGameOver?.Invoke();
    }

    public void PlayerWon()
    {
        if (GameEnded) return;
        GameEnded = true;
        OnVictory?.Invoke();
    }

    public void Restart()
    {
        GameEnded        = false;
        EnemiesRemaining = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── Signal helpers (called by other scripts) ─────────────────────────────
    public static void NotifyHealthChanged(float current, float max)  => OnHealthChanged?.Invoke(current, max);
    public static void NotifyAmmoChanged(int current, int reserve)    => OnAmmoChanged?.Invoke(current, reserve);
    public static void NotifyWeaponChanged(WeaponBase weapon)         => OnWeaponChanged?.Invoke(weapon);
}
