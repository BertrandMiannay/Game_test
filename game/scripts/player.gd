extends CharacterBody3D

# ─── Constantes de mouvement ───────────────────────────────────────────────────
const WALK_SPEED    := 7.0
const SPRINT_SPEED  := 12.0
const JUMP_VELOCITY := 5.0
const GRAVITY       := 9.8
const MOUSE_SENS    := 0.002

# ─── Stats ─────────────────────────────────────────────────────────────────────
@export var max_health: float = 100.0
var health: float

# ─── Armes ─────────────────────────────────────────────────────────────────────
var weapons: Array[Node] = []
var current_weapon_index: int = -1

# ─── État tir ──────────────────────────────────────────────────────────────────
var shooting: bool      = false
var shoot_just_pressed: bool = false

# ─── Références ────────────────────────────────────────────────────────────────
@onready var head: Node3D          = $Head
@onready var camera: Camera3D      = $Head/Camera3D
@onready var weapon_holder: Node3D = $Head/Camera3D/WeaponHolder

# ─── Initialisation ────────────────────────────────────────────────────────────
func _ready() -> void:
	health = max_health
	add_to_group("player")
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	call_deferred("_setup_weapons")

func _setup_weapons() -> void:
	for child in weapon_holder.get_children():
		if child.has_method("try_shoot"):
			weapons.append(child)
			child.visible = false
	if weapons.size() > 0:
		_switch_weapon(0)

# ─── Capture de tous les events avant tout autre handler ───────────────────────
func _input(event: InputEvent) -> void:
	# Rotation caméra
	if event is InputEventMouseMotion:
		if not GameManager.game_ended:
			rotate_y(-event.relative.x * MOUSE_SENS)
			head.rotate_x(-event.relative.y * MOUSE_SENS)
			head.rotation.x = clamp(head.rotation.x, -PI / 2.0, PI / 2.0)
		return

	# Clic souris
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				if Input.get_mouse_mode() != Input.MOUSE_MODE_CAPTURED:
					Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
				else:
					shooting = true
					shoot_just_pressed = true
			else:
				shooting = false
		return

	# Clavier
	if event is InputEventKey and event.pressed and not event.echo:
		match event.physical_keycode:
			KEY_ESCAPE:
				Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
				shooting = false
			KEY_1:
				_switch_weapon(0)
			KEY_2:
				_switch_weapon(1)
			KEY_R:
				_try_reload()
			KEY_ENTER:
				if GameManager.game_ended:
					GameManager.restart()

# ─── Physique ──────────────────────────────────────────────────────────────────
func _physics_process(delta: float) -> void:
	if GameManager.game_ended:
		shooting = false
		return

	# Gravité
	if not is_on_floor():
		velocity.y -= GRAVITY * delta

	# Saut
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Déplacement
	var input_dir := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	var speed := SPRINT_SPEED if Input.is_action_pressed("sprint") else WALK_SPEED

	if direction.length() > 0.1:
		velocity.x = direction.x * speed
		velocity.z = direction.z * speed
	else:
		velocity.x = move_toward(velocity.x, 0, speed)
		velocity.z = move_toward(velocity.z, 0, speed)

	move_and_slide()

	# Tir : auto = maintien, semi-auto = front montant seulement
	var w := _current_weapon()
	if shooting and w and (w.is_automatic or shoot_just_pressed):
		w.try_shoot()
	shoot_just_pressed = false

# ─── Armes ─────────────────────────────────────────────────────────────────────
func _current_weapon() -> WeaponBase:
	if current_weapon_index >= 0 and current_weapon_index < weapons.size():
		return weapons[current_weapon_index] as WeaponBase
	return null

func _switch_weapon(index: int) -> void:
	if index < 0 or index >= weapons.size():
		return
	var prev := _current_weapon()
	if prev:
		prev.visible = false
	current_weapon_index = index
	weapons[current_weapon_index].visible = true
	GameManager.weapon_changed.emit(weapons[current_weapon_index])

func _try_shoot() -> void:
	var w := _current_weapon()
	if w:
		w.try_shoot()

func _try_reload() -> void:
	var w := _current_weapon()
	if w:
		w.reload()

# ─── Dégâts ────────────────────────────────────────────────────────────────────
func take_damage(amount: float) -> void:
	health = max(0.0, health - amount)
	GameManager.health_changed.emit(health, max_health)
	if health <= 0.0:
		GameManager.player_died()
