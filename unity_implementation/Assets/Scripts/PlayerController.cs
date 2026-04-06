using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Constants ─────────────────────────────────────────────────────────────
    const float WALK_SPEED   = 7f;
    const float SPRINT_SPEED = 12f;
    const float JUMP_VEL     = 5f;
    const float GRAVITY      = 9.8f;
    const float MOUSE_SENS   = 0.002f;
    const float MAX_HEALTH   = 100f;

    // ── References ────────────────────────────────────────────────────────────
    CharacterController _cc;
    public Transform    headTransform;   // child object at eye level
    Camera              _cam;

    // ── Weapons ───────────────────────────────────────────────────────────────
    public WeaponBase[] weapons;
    int    _weaponIndex;
    bool   _fireHeld;
    bool   _firePrev;

    // ── State ─────────────────────────────────────────────────────────────────
    float   _health = MAX_HEALTH;
    Vector3 _velocity;
    float   _yaw;
    float   _pitch;
    bool    _dead;
    bool    _gameOver;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _cc  = GetComponent<CharacterController>();
        _cam = headTransform.GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
        GameManager.OnVictory  += HandleGameOver;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
        GameManager.OnVictory  -= HandleGameOver;
    }

    void HandleGameOver() => _gameOver = true;

    // ── Input ─────────────────────────────────────────────────────────────────
    void Update()
    {
        HandleMouseLook();
        HandleWeaponSwitch();
        HandleReload();
        HandleShooting();
        HandleRestartInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxisRaw("Mouse X");
            float my = Input.GetAxisRaw("Mouse Y");
            _yaw   += mx / Time.deltaTime * MOUSE_SENS * Time.deltaTime * Mathf.Rad2Deg;
            _pitch -= my / Time.deltaTime * MOUSE_SENS * Time.deltaTime * Mathf.Rad2Deg;
            _pitch  = Mathf.Clamp(_pitch, -90f, 90f);
            transform.localRotation          = Quaternion.Euler(0, _yaw, 0);
            headTransform.localRotation      = Quaternion.Euler(_pitch, 0, 0);
        }
    }

    void HandleMovement()
    {
        if (_dead) return;

        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0) _velocity.y = -2f;

        float h = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)  ? 1 : 0)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)   ? 1 : 0);
        float v = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)     ? 1 : 0)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)   ? 1 : 0);

        float speed = Input.GetKey(KeyCode.LeftShift) ? SPRINT_SPEED : WALK_SPEED;
        Vector3 move = (transform.right * h + transform.forward * v).normalized * speed;

        if (grounded && Input.GetKeyDown(KeyCode.Space))
            _velocity.y = JUMP_VEL;

        _velocity.y -= GRAVITY * Time.fixedDeltaTime;

        _cc.Move((move + Vector3.up * _velocity.y) * Time.fixedDeltaTime);
    }

    void HandleWeaponSwitch()
    {
        if (weapons == null || weapons.Length == 0) return;
        int next = _weaponIndex;
        if (Input.GetKeyDown(KeyCode.Alpha1)) next = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) next = 1;
        if (next != _weaponIndex) SwitchWeapon(next);
    }

    void SwitchWeapon(int idx)
    {
        if (idx < 0 || idx >= weapons.Length) return;
        if (weapons[_weaponIndex] != null) weapons[_weaponIndex].gameObject.SetActive(false);
        _weaponIndex = idx;
        if (weapons[_weaponIndex] != null)
        {
            weapons[_weaponIndex].gameObject.SetActive(true);
            GameManager.NotifyWeaponChanged(weapons[_weaponIndex]);
        }
    }

    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && weapons != null && weapons.Length > _weaponIndex)
            weapons[_weaponIndex]?.StartReload();
    }

    void HandleShooting()
    {
        if (weapons == null || weapons.Length <= _weaponIndex) return;
        WeaponBase w = weapons[_weaponIndex];
        if (w == null) return;

        bool fireDown = Input.GetMouseButton(0) && Cursor.lockState == CursorLockMode.Locked;
        bool justPressed = fireDown && !_firePrev;
        _firePrev = fireDown;

        if (w.isAutomatic)
        {
            if (fireDown) w.TryShoot();
        }
        else
        {
            if (justPressed) w.TryShoot();
        }
    }

    void HandleRestartInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) && _gameOver)
            GameManager.Instance?.Restart();
    }

    // ── Damage ────────────────────────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (_dead) return;
        _health = Mathf.Max(0, _health - amount);
        GameManager.NotifyHealthChanged(_health, MAX_HEALTH);
        if (_health <= 0)
        {
            _dead = true;
            GameManager.Instance?.PlayerDied();
        }
    }

    // ── Public accessors ──────────────────────────────────────────────────────
    public WeaponBase CurrentWeapon => (weapons != null && weapons.Length > _weaponIndex) ? weapons[_weaponIndex] : null;
    public Camera     PlayerCamera  => _cam;

    public void InitWeapons()
    {
        if (weapons == null || weapons.Length == 0) return;
        for (int i = 0; i < weapons.Length; i++)
            if (weapons[i] != null) weapons[i].gameObject.SetActive(i == 0);
        _weaponIndex = 0;
        GameManager.NotifyWeaponChanged(weapons[0]);
        GameManager.NotifyHealthChanged(_health, MAX_HEALTH);
    }
}
