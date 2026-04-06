using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ── Stats (set by subclass) ───────────────────────────────────────────────
    public string weaponName  = "Weapon";
    public float  damage      = 25f;
    public float  fireRate    = 0.12f;  // seconds between shots
    public int    maxAmmo     = 30;
    public int    reserveAmmo = 90;
    public float  reloadTime  = 2f;
    public bool   isAutomatic = true;

    // ── State ─────────────────────────────────────────────────────────────────
    protected int  currentAmmo;
    protected bool canShoot   = true;
    protected bool isReloading;

    // ── References ────────────────────────────────────────────────────────────
    protected AudioSource audioSource;
    Camera _cam;

    // ─────────────────────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        currentAmmo = maxAmmo;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D for player weapons
        audioSource.volume       = 0.7f;

        // Build mesh visuals
        BuildMesh();
    }

    void Start()
    {
        // Find camera in parents
        _cam = GetComponentInParent<Camera>();
        if (_cam == null) _cam = Camera.main;

        var ac = CreateFireSound();
        if (ac != null) audioSource.clip = ac;

        BroadcastAmmo();
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void TryShoot()
    {
        if (!canShoot || isReloading) return;
        if (currentAmmo <= 0) { StartReload(); return; }

        currentAmmo--;
        Shoot();
        audioSource.PlayOneShot(audioSource.clip);
        BroadcastAmmo();
        StartCoroutine(FireCooldown());
    }

    public void StartReload()
    {
        if (isReloading || currentAmmo == maxAmmo || reserveAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    protected virtual void Shoot()
    {
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            // Try player (shouldn't happen but safeguard) then enemy
            var enemy = hit.collider.GetComponentInParent<EnemyJaffa>();
            if (enemy != null)
            {
                OnHitEnemy(enemy);
            }
        }
    }

    // Override in subclasses that need special hit behaviour (Zat)
    protected virtual void OnHitEnemy(EnemyJaffa enemy)
    {
        enemy.TakeDamage(damage, false);
    }

    IEnumerator FireCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        int needed   = maxAmmo - currentAmmo;
        int transfer = Mathf.Min(needed, reserveAmmo);
        currentAmmo  += transfer;
        reserveAmmo  -= transfer;
        isReloading   = false;
        BroadcastAmmo();
    }

    void BroadcastAmmo() => GameManager.NotifyAmmoChanged(currentAmmo, reserveAmmo);

    // ── Overrideable by subclasses ─────────────────────────────────────────────
    protected abstract void  BuildMesh();
    protected abstract AudioClip CreateFireSound();

    // ── Helpers ───────────────────────────────────────────────────────────────
    protected static GameObject AddBox(Transform parent, Vector3 localPos, Vector3 size, Color color,
                                       float metallic = 0f, float smoothness = 0.5f)
    {
        var go   = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = size;
        Destroy(go.GetComponent<Collider>());
        ApplyMaterial(go, color, metallic, smoothness);
        return go;
    }

    protected static GameObject AddCylinder(Transform parent, Vector3 localPos, float radius, float height, Color color,
                                             float metallic = 0f, float smoothness = 0.5f,
                                             Quaternion? rot = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        if (rot.HasValue) go.transform.localRotation = rot.Value;
        Destroy(go.GetComponent<Collider>());
        ApplyMaterial(go, color, metallic, smoothness);
        return go;
    }

    static void ApplyMaterial(GameObject go, Color color, float metallic, float smoothness)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);
        go.GetComponent<MeshRenderer>().material = mat;
    }
}
