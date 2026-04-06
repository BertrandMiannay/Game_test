using UnityEngine;

/// <summary>
/// Attached to an empty GameObject in MainLevel.unity.
/// Builds the entire scene at runtime: level geometry, player, enemies, Stargate, HUD, lights.
/// No prefabs required – everything is procedural.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    // Enemy spawn positions (matching Godot implementation)
    static readonly Vector3[] EnemySpawns =
    {
        new Vector3(-8f,  0f,  5f),
        new Vector3( 8f,  0f,  5f),
        new Vector3(-15f, 0f, -5f),
        new Vector3( 15f, 0f, -5f),
        new Vector3(-6f,  0f, -15f),
        new Vector3( 6f,  0f, -15f),
    };

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        // 1. GameManager singleton
        CreateGameManager();

        // 2. Level geometry
        CreateLevel();

        // 3. Player
        CreatePlayer();

        // 4. Enemies
        CreateEnemies();

        // 5. Stargate
        CreateStargate();

        // 6. HUD
        CreateHUD();

        // 7. Lighting
        CreateLighting();
    }

    // ── GameManager ───────────────────────────────────────────────────────────
    void CreateGameManager()
    {
        if (GameManager.Instance != null) return;
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    // ── Level ─────────────────────────────────────────────────────────────────
    void CreateLevel()
    {
        var go = new GameObject("Level");
        var builder = go.AddComponent<LevelBuilder>();
        builder.Build();
    }

    // ── Player ────────────────────────────────────────────────────────────────
    void CreatePlayer()
    {
        // Root (CharacterController)
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.transform.position = new Vector3(0f, 1f, 28f);

        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0.1f, 0);

        // Head (vertical rotation pivot)
        var headGO = new GameObject("Head");
        headGO.transform.SetParent(playerGO.transform, false);
        headGO.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        // Camera
        var camGO = new GameObject("Camera");
        camGO.transform.SetParent(headGO.transform, false);
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 75f;
        cam.nearClipPlane = 0.05f;
        camGO.AddComponent<AudioListener>();

        // Weapon holder
        var weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(camGO.transform, false);
        weaponHolder.transform.localPosition = new Vector3(0.20f, -0.18f, 0.35f);

        // Weapons
        var p90Go = new GameObject("WeaponP90");
        p90Go.transform.SetParent(weaponHolder.transform, false);
        var p90 = p90Go.AddComponent<WeaponP90>();

        var zatGo = new GameObject("WeaponZat");
        zatGo.transform.SetParent(weaponHolder.transform, false);
        zatGo.SetActive(false);
        var zat = zatGo.AddComponent<WeaponZat>();

        // Player controller
        var ctrl = playerGO.AddComponent<PlayerController>();
        ctrl.headTransform = headGO.transform;
        ctrl.weapons       = new WeaponBase[] { p90, zat };

        // Small body visual
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(playerGO.transform, false);
        body.transform.localPosition = new Vector3(0, 0.1f, 0);
        body.transform.localScale    = new Vector3(0.8f, 0.9f, 0.8f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<MeshRenderer>().enabled = false; // FPS – hide body

        ctrl.InitWeapons();
    }

    // ── Enemies ───────────────────────────────────────────────────────────────
    void CreateEnemies()
    {
        for (int i = 0; i < EnemySpawns.Length; i++)
        {
            var go = new GameObject($"Jaffa_{i}");
            go.transform.position = EnemySpawns[i];

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.9f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.95f, 0);

            go.AddComponent<EnemyJaffa>();
        }
    }

    // ── Stargate ──────────────────────────────────────────────────────────────
    void CreateStargate()
    {
        var go = new GameObject("Stargate");
        go.transform.position = new Vector3(0f, 4.2f, -28f);
        go.AddComponent<Stargate>();
    }

    // ── HUD ───────────────────────────────────────────────────────────────────
    void CreateHUD()
    {
        var go = new GameObject("HUD");
        go.AddComponent<HUD>();
    }

    // ── Lighting ─────────────────────────────────────────────────────────────
    void CreateLighting()
    {
        // Directional sun (desert afternoon)
        var sun = new GameObject("Sun");
        sun.transform.rotation = Quaternion.Euler(55f, -30f, 0f);
        var dl = sun.AddComponent<Light>();
        dl.type      = LightType.Directional;
        dl.color     = new Color(1f, 0.95f, 0.80f);
        dl.intensity = 1.2f;
        dl.shadows   = LightShadows.Soft;

        // Ambient
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.30f, 0.28f, 0.22f);
        RenderSettings.fog          = false;
    }
}
