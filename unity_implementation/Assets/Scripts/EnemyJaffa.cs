using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyJaffa : MonoBehaviour
{
    // ── Stats ─────────────────────────────────────────────────────────────────
    const float MAX_HEALTH       = 80f;
    const float WALK_SPEED       = 2.5f;
    const float CHASE_SPEED      = 4.5f;
    const float DETECTION_RANGE  = 18f;
    const float ATTACK_RANGE     = 7f;
    const float ATTACK_COOLDOWN  = 2f;
    const float GRAVITY          = 9.8f;

    // ── State ─────────────────────────────────────────────────────────────────
    enum EnemyState { PATROL, CHASE, ATTACK, STUNNED, DEAD }
    EnemyState _state = EnemyState.PATROL;

    float   _health = MAX_HEALTH;
    bool    _canAttack = true;
    float   _stunTimer;
    float   _vy;

    // ── Patrol ────────────────────────────────────────────────────────────────
    Vector3[] _patrolPoints;
    int       _patrolIndex;

    // ── Animation ─────────────────────────────────────────────────────────────
    float _animTime;
    float _attackAnim;
    Transform _leftLegPivot, _rightLegPivot;
    Transform _leftArmPivot, _rightArmPivot;

    // ── Refs ──────────────────────────────────────────────────────────────────
    CharacterController _cc;
    Transform           _player;
    AudioSource         _audio;
    AudioClip           _staffSound;

    // ── Projectile prefab ─────────────────────────────────────────────────────
    static GameObject _projectilePrefab;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        // Register with GameManager
        GameManager.Instance?.RegisterEnemy();

        // Build patrol points (square, 4 units)
        Vector3 s = transform.position;
        _patrolPoints = new Vector3[]
        {
            s,
            s + new Vector3(4, 0, 0),
            s + new Vector3(4, 0, 4),
            s + new Vector3(0, 0, 4)
        };

        // Audio
        _audio      = gameObject.AddComponent<AudioSource>();
        _audio.spatialBlend = 1f;
        _audio.maxDistance  = 40f;
        _staffSound = SoundGenerator.MakeStaff();

        // Build procedural mesh
        BuildMesh();
        tag = "Enemy";
    }

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (_state == EnemyState.DEAD) return;

        // Gravity
        if (_cc.isGrounded) _vy = -2f;
        else _vy -= GRAVITY * Time.deltaTime;

        switch (_state)
        {
            case EnemyState.PATROL:  UpdatePatrol();  break;
            case EnemyState.CHASE:   UpdateChase();   break;
            case EnemyState.ATTACK:  UpdateAttack();  break;
            case EnemyState.STUNNED: UpdateStunned(); break;
        }
    }

    // ── Patrol ────────────────────────────────────────────────────────────────
    void UpdatePatrol()
    {
        Vector3 target = _patrolPoints[_patrolIndex];
        target.y = transform.position.y;
        Vector3 dir = (target - transform.position);

        if (dir.magnitude < 1.5f)
        {
            _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
        }
        else
        {
            MoveTowards(dir.normalized, WALK_SPEED);
            Animate(WALK_SPEED);
        }

        _cc.Move(Vector3.up * _vy * Time.deltaTime);

        if (CanSeePlayer())
            _state = EnemyState.CHASE;
    }

    // ── Chase ─────────────────────────────────────────────────────────────────
    void UpdateChase()
    {
        if (_player == null) { _state = EnemyState.PATROL; return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= ATTACK_RANGE && HasLineOfSight())
        {
            _state = EnemyState.ATTACK;
            return;
        }

        Vector3 dir = (_player.position - transform.position);
        dir.y = 0;
        MoveTowards(dir.normalized, CHASE_SPEED);
        Animate(CHASE_SPEED);
        _cc.Move(Vector3.up * _vy * Time.deltaTime);
    }

    // ── Attack ────────────────────────────────────────────────────────────────
    void UpdateAttack()
    {
        if (_player == null) { _state = EnemyState.PATROL; return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist > ATTACK_RANGE || !HasLineOfSight())
        {
            _state = EnemyState.CHASE;
            return;
        }

        // Face player
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        _cc.Move(Vector3.up * _vy * Time.deltaTime);

        if (_canAttack)
            StartCoroutine(AttackRoutine());

        // Attack animation
        _attackAnim = Mathf.Clamp01(_attackAnim + Time.deltaTime * 2.5f);
        ApplyAttackAnim();
    }

    // ── Stunned ───────────────────────────────────────────────────────────────
    void UpdateStunned()
    {
        _stunTimer -= Time.deltaTime;
        _cc.Move(Vector3.up * _vy * Time.deltaTime);
        if (_stunTimer <= 0f)
            _state = EnemyState.CHASE;
    }

    // ── Coroutines ────────────────────────────────────────────────────────────
    IEnumerator AttackRoutine()
    {
        _canAttack = false;
        _attackAnim = 1f;
        FireProjectile();
        yield return new WaitForSeconds(ATTACK_COOLDOWN);
        _canAttack = true;
    }

    void FireProjectile()
    {
        if (_player == null) return;
        _audio.PlayOneShot(_staffSound, 0.9f);

        Vector3 muzzle = transform.position + Vector3.up * 1.2f;
        Vector3 dir    = (_player.position + Vector3.up * 0.9f - muzzle).normalized;

        var go = new GameObject("EnemyProjectile");
        go.transform.position = muzzle + dir * 0.6f;
        go.tag = "Projectile";
        var proj = go.AddComponent<Projectile>();
        proj.direction = dir;
        proj.damage    = 12f;
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void TakeDamage(float amount, bool disintegrate)
    {
        if (_state == EnemyState.DEAD) return;
        _health -= amount;
        if (_health <= 0)
            StartCoroutine(DiRoutine(disintegrate));
    }

    public void ZatStun()
    {
        if (_state == EnemyState.DEAD) return;
        _state     = EnemyState.STUNNED;
        _stunTimer = 3f;
    }

    // ── Death ─────────────────────────────────────────────────────────────────
    IEnumerator DiRoutine(bool disintegrate)
    {
        _state = EnemyState.DEAD;
        _cc.enabled = false;
        GameManager.Instance?.NotifyEnemyKilled();

        float t = 0f;
        float dur = disintegrate ? 0.5f : 0.8f;
        Vector3 startScale = transform.localScale;

        while (t < dur)
        {
            t += Time.deltaTime;
            float frac = t / dur;
            if (!disintegrate && frac < 0.4f)
            {
                // Tip over
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y,
                    Mathf.Lerp(0f, 90f, frac / 0.4f));
            }
            else
            {
                float scaleFrac = disintegrate ? (1f - frac) : (1f - (frac - 0.4f) / 0.6f);
                transform.localScale = startScale * Mathf.Max(0.01f, scaleFrac);
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void MoveTowards(Vector3 dir, float speed)
    {
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
        _cc.Move(dir * speed * Time.deltaTime);
    }

    bool CanSeePlayer()
    {
        if (_player == null) return false;
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > DETECTION_RANGE) return false;
        return HasLineOfSight();
    }

    bool HasLineOfSight()
    {
        if (_player == null) return false;
        Vector3 head   = transform.position + Vector3.up * 0.9f;
        Vector3 target = _player.position + Vector3.up * 0.9f;
        Vector3 dir    = target - head;
        if (Physics.Raycast(head, dir.normalized, out RaycastHit hit, dir.magnitude))
        {
            return hit.collider.GetComponentInParent<PlayerController>() != null;
        }
        return false;
    }

    // ── Animation ─────────────────────────────────────────────────────────────
    void Animate(float speed)
    {
        _animTime += Time.deltaTime * 5f;
        float legAngle = Mathf.Sin(_animTime) * 26f;  // degrees
        float armAngle = Mathf.Sin(_animTime) * 17f;

        if (_leftLegPivot)  _leftLegPivot.localRotation  = Quaternion.Euler(legAngle, 0, 0);
        if (_rightLegPivot) _rightLegPivot.localRotation  = Quaternion.Euler(-legAngle, 0, 0);
        if (_leftArmPivot)  _leftArmPivot.localRotation   = Quaternion.Euler(-armAngle, 0, 0);
        if (_rightArmPivot) _rightArmPivot.localRotation  = Quaternion.Euler(armAngle, 0, 0);
    }

    void ApplyAttackAnim()
    {
        if (_rightArmPivot)
            _rightArmPivot.localRotation = Quaternion.Euler(-_attackAnim * 68.8f, 0, 0);
        _attackAnim = Mathf.Max(0, _attackAnim - Time.deltaTime * 2.5f);
    }

    // ── Procedural Jaffa Mesh ─────────────────────────────────────────────────
    void BuildMesh()
    {
        Color armor = new Color(0.10f, 0.07f, 0.06f);
        Color gold  = new Color(0.72f, 0.55f, 0.08f);
        Color skin  = new Color(0.52f, 0.36f, 0.22f);
        Color metal = new Color(0.18f, 0.16f, 0.14f);

        Transform root = transform;

        // ── Legs ─────────────────────────────────────────────────────────────
        _leftLegPivot  = CreatePivot(root, new Vector3(-0.12f, 0.95f, 0f));
        _rightLegPivot = CreatePivot(root, new Vector3( 0.12f, 0.95f, 0f));
        AddCylPart(_leftLegPivot,  Vector3.down * 0.475f, 0.10f, 0.95f, armor, 0.85f, 0.65f);
        AddCylPart(_rightLegPivot, Vector3.down * 0.475f, 0.10f, 0.95f, armor, 0.85f, 0.65f);
        AddBoxPart(_leftLegPivot,  new Vector3(0f, -0.35f, 0.07f), new Vector3(0.13f, 0.08f, 0.07f), gold,  0.95f, 0.85f);
        AddBoxPart(_rightLegPivot, new Vector3(0f, -0.35f, 0.07f), new Vector3(0.13f, 0.08f, 0.07f), gold,  0.95f, 0.85f);

        // ── Belt ─────────────────────────────────────────────────────────────
        AddBoxPart(root, new Vector3(0f, 0.95f, 0f), new Vector3(0.38f, 0.09f, 0.30f), armor, 0.85f, 0.65f);

        // ── Torso ─────────────────────────────────────────────────────────────
        AddCylPart(root, new Vector3(0f, 1.18f, 0f), 0.20f, 0.44f, armor, 0.85f, 0.65f);
        AddBoxPart(root, new Vector3(0f, 1.22f, 0.13f), new Vector3(0.34f, 0.28f, 0.06f), armor, 0.85f, 0.65f);
        AddBoxPart(root, new Vector3(0f, 1.28f, 0.17f), new Vector3(0.10f, 0.10f, 0.04f), gold, 0.95f, 0.85f);

        // ── Arms ─────────────────────────────────────────────────────────────
        _leftArmPivot  = CreatePivot(root, new Vector3(-0.25f, 1.46f, 0f));
        _rightArmPivot = CreatePivot(root, new Vector3( 0.25f, 1.46f, 0f));
        AddCylPart(_leftArmPivot,  Vector3.down * 0.22f, 0.065f, 0.44f, armor, 0.85f, 0.65f);
        AddCylPart(_rightArmPivot, Vector3.down * 0.22f, 0.065f, 0.44f, armor, 0.85f, 0.65f);
        AddBoxPart(_leftArmPivot,  new Vector3(0f, 0.04f, 0.04f), new Vector3(0.15f, 0.08f, 0.22f), gold, 0.95f, 0.85f);
        AddBoxPart(_rightArmPivot, new Vector3(0f, 0.04f, 0.04f), new Vector3(0.15f, 0.08f, 0.22f), gold, 0.95f, 0.85f);

        // ── Staff weapon (right arm) ──────────────────────────────────────────
        AddCylPart(_rightArmPivot, new Vector3(0.18f, -0.55f, 0f), 0.022f, 1.80f, metal, 0.80f, 0.60f);
        AddSphPart(_rightArmPivot, new Vector3(0.18f, 0.35f, 0f),  0.060f, gold, 0.95f, 0.85f);
        AddSphPart(_rightArmPivot, new Vector3(0.18f, 0.43f, 0f),  0.055f, gold, 0.95f, 0.85f);
        AddBoxPart(_rightArmPivot, new Vector3(0.18f, 0.52f, 0f),  new Vector3(0.035f, 0.10f, 0.035f), gold, 0.95f, 0.85f);

        // ── Neck + Head ──────────────────────────────────────────────────────
        AddCylPart(root, new Vector3(0f, 1.60f, 0f), 0.072f, 0.10f, skin, 0f, 0.85f);
        AddSphPart(root, new Vector3(0f, 1.72f, 0f), 0.155f, skin, 0f, 0.85f);

        // ── Helmet (cobra) ───────────────────────────────────────────────────
        AddSphPart(root, new Vector3(0f, 1.76f, 0f), 0.185f, armor, 0.85f, 0.65f);
        AddBoxPart(root, new Vector3(0f,      2.00f, -0.04f), new Vector3(0.055f, 0.22f, 0.055f), gold, 0.95f, 0.85f);
        AddBoxPart(root, new Vector3(-0.16f,  1.91f, -0.06f), new Vector3(0.055f, 0.17f, 0.04f),  gold, 0.95f, 0.85f);
        AddBoxPart(root, new Vector3( 0.16f,  1.91f, -0.06f), new Vector3(0.055f, 0.17f, 0.04f),  gold, 0.95f, 0.85f);
    }

    // ── Mesh helpers ──────────────────────────────────────────────────────────
    static Transform CreatePivot(Transform parent, Vector3 localPos)
    {
        var go = new GameObject("Pivot");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    static void AddBoxPart(Transform parent, Vector3 pos, Vector3 size, Color col, float metal, float smooth)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = size;
        Destroy(go.GetComponent<Collider>());
        SetMat(go, col, metal, smooth);
    }

    static void AddCylPart(Transform parent, Vector3 pos, float radius, float height, Color col, float metal, float smooth)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        Destroy(go.GetComponent<Collider>());
        SetMat(go, col, metal, smooth);
    }

    static void AddSphPart(Transform parent, Vector3 pos, float radius, Color col, float metal, float smooth)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = Vector3.one * radius * 2f;
        Destroy(go.GetComponent<Collider>());
        SetMat(go, col, metal, smooth);
    }

    static void SetMat(GameObject go, Color col, float metal, float smooth)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = col;
        mat.SetFloat("_Metallic",    metal);
        mat.SetFloat("_Glossiness",  smooth);
        go.GetComponent<MeshRenderer>().material = mat;
    }
}
