using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and manages all HUD elements programmatically.
/// No prefabs or TextMeshPro required – uses legacy UI Text + Slider.
/// </summary>
public class HUD : MonoBehaviour
{
    Slider _healthBar;
    Text   _healthLabel;
    Text   _ammoLabel;
    Text   _weaponLabel;
    Text   _messageLabel;
    Text   _enemiesLabel;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildCanvas();
    }

    void OnEnable()
    {
        GameManager.OnHealthChanged  += UpdateHealth;
        GameManager.OnAmmoChanged    += UpdateAmmo;
        GameManager.OnWeaponChanged  += UpdateWeapon;
        GameManager.OnEnemyKilled    += UpdateEnemies;
        GameManager.OnGameOver       += ShowGameOver;
        GameManager.OnVictory        += ShowVictory;
    }

    void OnDisable()
    {
        GameManager.OnHealthChanged  -= UpdateHealth;
        GameManager.OnAmmoChanged    -= UpdateAmmo;
        GameManager.OnWeaponChanged  -= UpdateWeapon;
        GameManager.OnEnemyKilled    -= UpdateEnemies;
        GameManager.OnGameOver       -= ShowGameOver;
        GameManager.OnVictory        -= ShowVictory;
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────
    void UpdateHealth(float current, float max)
    {
        _healthBar.value   = current / max;
        _healthLabel.text  = $"{(int)current} / {(int)max}";
        _healthLabel.color = current < 30f ? Color.red : Color.white;
    }

    void UpdateAmmo(int current, int reserve)
    {
        _ammoLabel.text = $"{current}  |  {reserve}";
    }

    void UpdateWeapon(WeaponBase weapon)
    {
        if (weapon == null) return;
        _weaponLabel.text = weapon.weaponName;
        _ammoLabel.text   = ""; // will be updated by next ammo signal
    }

    void UpdateEnemies(int remaining)
    {
        _enemiesLabel.text = $"Jaffa : {remaining}";
        if (remaining == 0)
            ShowMessage("Tous les Jaffa éliminés !", Color.yellow, 3f);
    }

    void ShowGameOver()
    {
        _messageLabel.color = Color.red;
        _messageLabel.text  = "MISSION ÉCHOUÉE\n\n[Entrée] Recommencer";
    }

    void ShowVictory()
    {
        _messageLabel.color = Color.yellow;
        _messageLabel.text  = "MISSION ACCOMPLIE\n\nVous avez traversé la Porte !";
    }

    // ── Timed message ─────────────────────────────────────────────────────────
    void ShowMessage(string msg, Color col, float duration)
    {
        _messageLabel.color = col;
        _messageLabel.text  = msg;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), duration);
    }

    void ClearMessage() => _messageLabel.text = "";

    // ── Build Canvas ──────────────────────────────────────────────────────────
    void BuildCanvas()
    {
        var canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Health bar (bottom-left) ──────────────────────────────────────────
        _healthBar = MakeSlider(canvasGO, "HealthBar",
            new Vector2(16, 16), new Vector2(280, 22),
            new Vector2(0, 0), new Vector2(0, 0));

        // ── Health label ──────────────────────────────────────────────────────
        _healthLabel = MakeText(canvasGO, "HealthLabel",
            new Vector2(16, 40), new Vector2(200, 24),
            new Vector2(0, 0), new Vector2(0, 0),
            "100 / 100", 16, TextAnchor.MiddleLeft);

        // ── Weapon label ──────────────────────────────────────────────────────
        _weaponLabel = MakeText(canvasGO, "WeaponLabel",
            new Vector2(16, 70), new Vector2(200, 28),
            new Vector2(0, 0), new Vector2(0, 0),
            "P90", 20, TextAnchor.MiddleLeft);
        _weaponLabel.color = new Color(1f, 0.85f, 0.2f);

        // ── Ammo label ────────────────────────────────────────────────────────
        _ammoLabel = MakeText(canvasGO, "AmmoLabel",
            new Vector2(16, 100), new Vector2(200, 28),
            new Vector2(0, 0), new Vector2(0, 0),
            "50  |  200", 18, TextAnchor.MiddleLeft);

        // ── Enemies label (top-right) ─────────────────────────────────────────
        _enemiesLabel = MakeText(canvasGO, "EnemiesLabel",
            new Vector2(-220, -16), new Vector2(210, 28),
            new Vector2(1, 1), new Vector2(1, 1),
            "Jaffa : 6", 18, TextAnchor.MiddleRight);
        _enemiesLabel.color = Color.red;

        // ── Message label (centre screen) ─────────────────────────────────────
        _messageLabel = MakeText(canvasGO, "MessageLabel",
            new Vector2(-300, -60), new Vector2(600, 120),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            "", 32, TextAnchor.MiddleCenter);
        _messageLabel.fontStyle = FontStyle.Bold;

        // Crosshair (simple +)
        var ch = MakeText(canvasGO, "Crosshair",
            new Vector2(-10, -10), new Vector2(20, 20),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            "+", 22, TextAnchor.MiddleCenter);
        ch.color = new Color(1f, 1f, 1f, 0.8f);
    }

    // ── UI Helpers ────────────────────────────────────────────────────────────
    static Slider MakeSlider(GameObject canvas, string name,
        Vector2 anchoredPos, Vector2 sizeDelta,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin      = anchorMin;
        rt.anchorMax      = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta      = sizeDelta;

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value    = 1;

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        // Fill area
        var fill = new GameObject("Fill");
        fill.transform.SetParent(go.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f, 0.85f);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;
        slider.fillRect  = fillRT;

        slider.targetGraphic = fillImg;
        return slider;
    }

    static Text MakeText(GameObject canvas, string name,
        Vector2 anchoredPos, Vector2 sizeDelta,
        Vector2 anchorMin, Vector2 anchorMax,
        string content, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;

        var txt       = go.AddComponent<Text>();
        txt.text      = content;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = fontSize;
        txt.alignment = alignment;
        txt.color     = Color.white;
        txt.supportRichText = true;
        return txt;
    }
}
