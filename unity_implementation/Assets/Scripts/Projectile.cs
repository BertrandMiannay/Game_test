using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    public Vector3 direction = Vector3.forward;
    public float   speed     = 18f;
    public float   damage    = 12f;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Build visual: orange glowing sphere
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(transform, false);
        sphere.transform.localScale = Vector3.one * 0.24f;
        Destroy(sphere.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.55f, 0.05f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.35f, 0f) * 4f);
        sphere.GetComponent<MeshRenderer>().material = mat;

        // Trigger collider
        var col = GetComponent<SphereCollider>();
        col.radius    = 0.18f;
        col.isTrigger = true;

        Destroy(gameObject, 4f);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore the enemy that fired this (tagged "Enemy")
        if (other.CompareTag("Enemy") || other.CompareTag("Projectile")) return;

        var player = other.GetComponentInParent<PlayerController>();
        if (player != null) player.TakeDamage(damage);

        Destroy(gameObject);
    }
}
