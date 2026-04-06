using UnityEngine;

public class WeaponZat : WeaponBase
{
    // Cycles 1 → 2 → 3 → 1 …
    int _shotMode = 1;

    protected override void Awake()
    {
        weaponName  = "Zat'nik'tel";
        damage      = 40f;
        fireRate    = 0.5f;
        maxAmmo     = 20;
        reserveAmmo = 60;
        reloadTime  = 1.2f;
        isAutomatic = false;
        base.Awake();
    }

    // ── Shoot override: cycle through 3 modes ────────────────────────────────
    protected override void OnHitEnemy(EnemyJaffa enemy)
    {
        switch (_shotMode)
        {
            case 1: // Stun
                enemy.ZatStun();
                break;
            case 2: // Kill
                enemy.TakeDamage(9999f, false);
                break;
            case 3: // Disintegrate
                enemy.TakeDamage(9999f, true);
                break;
        }

        _shotMode = _shotMode >= 3 ? 1 : _shotMode + 1;
    }

    // ── Procedural mesh (golden alien pistol) ─────────────────────────────────
    protected override void BuildMesh()
    {
        var gold      = new Color(0.75f, 0.60f, 0.10f);
        var goldLight = new Color(0.85f, 0.70f, 0.20f);

        // Body (horizontal cylinder)
        AddCylinder(transform, Vector3.zero, 0.025f, 0.22f, gold, 0.8f, 0.8f,
                    Quaternion.Euler(90f, 0f, 0f));
        // Head / emitter
        AddBox(transform, new Vector3(0f, 0f, 0.14f), new Vector3(0.04f, 0.04f, 0.06f), goldLight, 0.9f, 0.8f);
    }

    // ── Procedural Zat sound ──────────────────────────────────────────────────
    protected override AudioClip CreateFireSound() => SoundGenerator.MakeZat();
}
