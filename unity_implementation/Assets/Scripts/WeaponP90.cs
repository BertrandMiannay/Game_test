using UnityEngine;

public class WeaponP90 : WeaponBase
{
    protected override void Awake()
    {
        weaponName  = "P90";
        damage      = 18f;
        fireRate    = 0.08f;
        maxAmmo     = 50;
        reserveAmmo = 200;
        reloadTime  = 2.2f;
        isAutomatic = true;
        base.Awake();
    }

    // ── Procedural mesh (gray metal FN P90) ───────────────────────────────────
    protected override void BuildMesh()
    {
        var gray   = new Color(0.25f, 0.25f, 0.28f);
        var magCol = new Color(0.18f, 0.18f, 0.20f);
        var dark   = new Color(0.15f, 0.15f, 0.15f);

        // Body
        AddBox(transform, Vector3.zero,               new Vector3(0.06f, 0.08f, 0.35f), gray,   0.75f, 0.65f);
        // Magazine (below body, slightly forward)
        AddBox(transform, new Vector3(0f, -0.08f, -0.02f), new Vector3(0.05f, 0.12f, 0.10f), magCol, 0.6f, 0.5f);
        // Barrel (forward, rotated on X so cylinder points Z)
        AddCylinder(transform, new Vector3(0f, 0.005f, 0.30f), 0.012f, 0.25f, dark, 0.8f, 0.4f,
                    Quaternion.Euler(90f, 0f, 0f));
    }

    // ── Procedural P90 fire sound ─────────────────────────────────────────────
    protected override AudioClip CreateFireSound() => SoundGenerator.MakeP90();
}
