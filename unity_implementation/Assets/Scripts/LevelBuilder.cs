using UnityEngine;

/// <summary>
/// Procedurally builds the entire Stargate military base level at runtime.
/// Exact layout ported from Godot's level_builder.gd.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color COL_SAND  = new Color(0.76f, 0.60f, 0.42f);
    static readonly Color COL_WALL  = new Color(0.52f, 0.42f, 0.28f);
    static readonly Color COL_METAL = new Color(0.28f, 0.28f, 0.32f);
    static readonly Color COL_GOLD  = new Color(0.70f, 0.55f, 0.10f);
    static readonly Color COL_CRATE = new Color(0.40f, 0.32f, 0.20f);
    static readonly Color COL_GLOW  = new Color(0.20f, 0.80f, 0.40f);

    // ─────────────────────────────────────────────────────────────────────────
    public void Build()
    {
        BuildFloor();
        BuildExteriorWalls();
        BuildInterior();
        BuildCover();
        BuildDecorations();
    }

    // ── Floor ─────────────────────────────────────────────────────────────────
    void BuildFloor()
    {
        Box("Floor", new Vector3(0, -0.5f, 0), new Vector3(52, 1, 64), COL_SAND);
    }

    // ── Exterior walls ────────────────────────────────────────────────────────
    void BuildExteriorWalls()
    {
        float h = 5f, t = 1f;

        // North wall (Stargate entrance, 10u gap centered at 0)
        Box("WallNL", new Vector3(-13f,  2.5f, -32f), new Vector3(26f, h, t), COL_WALL);
        Box("WallNR", new Vector3( 13f,  2.5f, -32f), new Vector3(26f, h, t), COL_WALL);
        Box("WallNT", new Vector3(  0f,  5.0f, -32f), new Vector3(10f, 1f, t), COL_WALL);  // lintel

        // South wall (player entrance, 8u gap)
        Box("WallSL", new Vector3(-14f,  2.5f, 32f), new Vector3(24f, h, t), COL_WALL);
        Box("WallSR", new Vector3( 14f,  2.5f, 32f), new Vector3(24f, h, t), COL_WALL);
        Box("WallST", new Vector3(  0f,  5.0f, 32f), new Vector3( 8f, 1f, t), COL_WALL);

        // East / West walls
        Box("WallE", new Vector3( 26f, 2.5f, 0f), new Vector3(t, h, 64f), COL_WALL);
        Box("WallW", new Vector3(-26f, 2.5f, 0f), new Vector3(t, h, 64f), COL_WALL);
    }

    // ── Interior layout ───────────────────────────────────────────────────────
    void BuildInterior()
    {
        // Central dividers
        Box("DividerL", new Vector3(-14f, 2.0f,  2f), new Vector3(24f, 4f, 0.8f), COL_WALL);
        Box("DividerR", new Vector3( 14f, 2.0f,  2f), new Vector3(24f, 4f, 0.8f), COL_WALL);

        // Lateral corridor walls
        Box("CorridorL", new Vector3(-22f, 2.0f, -8f), new Vector3(0.8f, 4f, 20f), COL_WALL);
        Box("CorridorR", new Vector3( 22f, 2.0f, -8f), new Vector3(0.8f, 4f, 20f), COL_WALL);

        // Command room (around Stargate)
        Box("CmdFront", new Vector3(0f, 2.0f, -18f),  new Vector3(18f, 4f, 0.8f), COL_METAL);
        Box("CmdLeft",  new Vector3(-9f, 2.0f, -24f), new Vector3(0.8f, 4f, 12f), COL_METAL);
        Box("CmdRight", new Vector3( 9f, 2.0f, -24f), new Vector3(0.8f, 4f, 12f), COL_METAL);

        // Support pillars
        foreach (int zSign in new[] { 1, -1 })
        {
            float z = zSign == 1 ? 8f : -4f;
            foreach (int xSign in new[] { -1, 1 })
            {
                float x = xSign * 8f;
                Box($"Pillar_{x}_{z}", new Vector3(x, 2.5f, z), new Vector3(1.2f, 5f, 1.2f), COL_WALL);
                Box($"Capital_{x}_{z}", new Vector3(x, 5.15f, z), new Vector3(1.6f, 0.3f, 1.6f), COL_GOLD,
                    0.95f, 0.85f);
            }
        }
    }

    // ── Cover elements ────────────────────────────────────────────────────────
    void BuildCover()
    {
        // Player spawn zone crates
        Box("CrateA1", new Vector3(-4f, 0.5f, 22f), new Vector3(2f, 1f, 2f), COL_CRATE);
        Box("CrateA2", new Vector3( 4f, 0.5f, 22f), new Vector3(2f, 1f, 2f), COL_CRATE);
        Box("CrateA3", new Vector3(-4f, 1.5f, 22f), new Vector3(2f, 1f, 2f), COL_CRATE); // stacked

        // Central barricades
        Box("BarrL",   new Vector3(-3f, 0.5f, 10f), new Vector3(4f, 1f, 0.6f), COL_CRATE);
        Box("BarrR",   new Vector3( 3f, 0.5f, 10f), new Vector3(4f, 1f, 0.6f), COL_CRATE);
        Box("BarrMid", new Vector3( 0f, 0.5f, -2f), new Vector3(3f, 1f, 0.6f), COL_METAL);

        // Command consoles (before Stargate)
        Box("ConsoleL", new Vector3(-4f, 0.8f, -12f), new Vector3(3.5f, 1.6f, 1.2f), COL_METAL);
        Box("ConsoleR", new Vector3( 4f, 0.8f, -12f), new Vector3(3.5f, 1.6f, 1.2f), COL_METAL);
        // Glowing strips
        BoxGlow("GlowL", new Vector3(-4f, 1.65f, -11.4f), new Vector3(3.0f, 0.1f, 0.1f), COL_GLOW);
        BoxGlow("GlowR", new Vector3( 4f, 1.65f, -11.4f), new Vector3(3.0f, 0.1f, 0.1f), COL_GLOW);
    }

    // ── Decorations ───────────────────────────────────────────────────────────
    void BuildDecorations()
    {
        // Sarcophagus
        Box("Sarcophagus", new Vector3(-18f, 0.6f, -5f), new Vector3(2.5f, 1.2f, 1.0f), COL_GOLD,
            0.90f, 0.80f);

        // Urns (cylinders)
        Cyl("UrnL", new Vector3(-15f, 0.9f,  18f), 0.30f, 1.8f, COL_GOLD, 0.7f, 0.7f);
        Cyl("UrnR", new Vector3( 15f, 0.9f,  18f), 0.30f, 1.8f, COL_GOLD, 0.7f, 0.7f);

        // Ramp to Stargate (tilted slightly)
        var ramp = Box("Ramp", new Vector3(0f, 0.0f, -16f), new Vector3(10f, 0.6f, 5f), COL_SAND);
        ramp.transform.rotation = Quaternion.Euler(-7f, 0f, 0f);
    }

    // ── Primitive helpers ─────────────────────────────────────────────────────
    GameObject Box(string name, Vector3 pos, Vector3 size, Color col,
                   float metallic = 0f, float smoothness = 0.4f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.position   = pos;
        go.transform.localScale = size;
        SetMat(go, col, metallic, smoothness, false);
        return go;
    }

    void BoxGlow(string name, Vector3 pos, Vector3 size, Color col)
    {
        var go = Box(name, pos, size, col);
        SetMat(go, col, 0f, 0.5f, true);
    }

    void Cyl(string name, Vector3 pos, float radius, float height, Color col,
             float metallic = 0f, float smoothness = 0.5f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        SetMat(go, col, metallic, smoothness, false);
    }

    static void SetMat(GameObject go, Color col, float metallic, float smoothness, bool emissive)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = col;
        mat.SetFloat("_Metallic",   metallic);
        mat.SetFloat("_Glossiness", smoothness);
        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", col * 2f);
        }
        go.GetComponent<MeshRenderer>().material = mat;
    }
}
