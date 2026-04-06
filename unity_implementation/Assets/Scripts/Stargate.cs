using System.Collections;
using UnityEngine;

public class Stargate : MonoBehaviour
{
    bool _active;

    // Visuals
    Transform _ring;
    MeshRenderer _portalRenderer;
    Material _portalMat;

    // Colors
    static readonly Color COL_INACTIVE = new Color(0.1f,  0.1f,  0.15f, 0.4f);
    static readonly Color COL_ACTIVE   = new Color(0.2f,  0.6f,  1.0f,  0.85f);
    static readonly Color EMIT_ACTIVE  = new Color(0.1f,  0.4f,  0.9f)  * 3f;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildMesh();
    }

    void OnEnable()  => GameManager.OnEnemyKilled += OnEnemyKilled;
    void OnDisable() => GameManager.OnEnemyKilled -= OnEnemyKilled;

    void OnEnemyKilled(int remaining)
    {
        if (remaining == 0) Activate();
    }

    void Activate()
    {
        _active = true;
        _portalMat.color = COL_ACTIVE;
        _portalMat.EnableKeyword("_EMISSION");
        _portalMat.SetColor("_EmissionColor", EMIT_ACTIVE);
    }

    void Update()
    {
        if (_active && _ring != null)
            _ring.Rotate(0, 0, 0.6f * Mathf.Rad2Deg * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_active)
        {
            // Pulse feedback when inactive
            StartCoroutine(PulseRoutine());
            return;
        }
        if (other.GetComponentInParent<PlayerController>() != null)
            GameManager.Instance?.PlayerWon();
    }

    IEnumerator PulseRoutine()
    {
        if (_ring == null) yield break;
        float t = 0f;
        while (t < 0.1f) { t += Time.deltaTime; _ring.localScale = Vector3.one * Mathf.Lerp(1f, 1.12f, t / 0.1f); yield return null; }
        t = 0f;
        while (t < 0.3f) { t += Time.deltaTime; _ring.localScale = Vector3.one * Mathf.Lerp(1.12f, 1f, t / 0.3f); yield return null; }
        _ring.localScale = Vector3.one;
    }

    // ── Procedural mesh ───────────────────────────────────────────────────────
    void BuildMesh()
    {
        // Outer ring (torus approximated with a cylinder torus using a thick ring)
        _ring = new GameObject("StargateRing").transform;
        _ring.SetParent(transform, false);
        _ring.localPosition = Vector3.zero;

        Color ringCol = new Color(0.60f, 0.55f, 0.10f);

        // 12 ring segments (rotated boxes forming a torus)
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            float r     = 3.8f;
            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.transform.SetParent(_ring, false);
            Destroy(seg.GetComponent<Collider>());
            seg.transform.localPosition = new Vector3(Mathf.Sin(angle) * r, Mathf.Cos(angle) * r, 0);
            seg.transform.localRotation = Quaternion.Euler(0, 0, i * 30f);
            seg.transform.localScale    = new Vector3(0.55f, 2.0f, 0.55f);
            SetMat(seg, ringCol, 0.85f, 0.75f, false);
        }

        // Glyphs on ring
        Color glyphCol = new Color(0.90f, 0.75f, 0.15f);
        for (int i = 0; i < 9; i++)
        {
            float angle = i * 40f * Mathf.Deg2Rad;
            float r     = 3.8f;
            var gl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gl.transform.SetParent(_ring, false);
            Destroy(gl.GetComponent<Collider>());
            gl.transform.localPosition = new Vector3(Mathf.Sin(angle) * r, Mathf.Cos(angle) * r, -0.28f);
            gl.transform.localRotation = Quaternion.Euler(0, 0, i * 40f);
            gl.transform.localScale    = new Vector3(0.20f, 0.35f, 0.10f);
            SetMat(gl, glyphCol, 0.95f, 0.90f, false);
        }

        // Inner portal disc
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portal.name = "Portal";
        portal.transform.SetParent(transform, false);
        portal.transform.localPosition = new Vector3(0, 0, 0.02f);
        portal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        portal.transform.localScale    = new Vector3(7.2f, 0.05f, 7.2f);
        Destroy(portal.GetComponent<Collider>());

        _portalMat      = new Material(Shader.Find("Standard"));
        _portalMat.color = COL_INACTIVE;
        _portalMat.SetFloat("_Mode", 3);   // Transparent
        _portalMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _portalMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _portalMat.SetInt("_ZWrite", 0);
        _portalMat.DisableKeyword("_ALPHATEST_ON");
        _portalMat.EnableKeyword("_ALPHABLEND_ON");
        _portalMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        _portalMat.renderQueue = 3000;
        portal.GetComponent<MeshRenderer>().material = _portalMat;
        _portalRenderer = portal.GetComponent<MeshRenderer>();

        // Frame base (stone arch)
        Color stone = new Color(0.45f, 0.40f, 0.32f);
        var frameGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameGo.name = "StargateBase";
        frameGo.transform.SetParent(transform, false);
        frameGo.transform.localPosition = new Vector3(0, -4.8f, 0);
        frameGo.transform.localScale    = new Vector3(9f, 1.5f, 1f);
        SetMat(frameGo, stone, 0.2f, 0.4f, false);

        // Trigger volume
        var col = gameObject.AddComponent<BoxCollider>();
        col.size      = new Vector3(7f, 8f, 1f);
        col.isTrigger = true;
    }

    static void SetMat(GameObject go, Color col, float metallic, float smoothness, bool emissive)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = col;
        mat.SetFloat("_Metallic",   metallic);
        mat.SetFloat("_Glossiness", smoothness);
        if (emissive) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", col * 2f); }
        go.GetComponent<MeshRenderer>().material = mat;
    }
}
