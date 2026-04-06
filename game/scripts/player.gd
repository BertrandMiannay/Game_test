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

# ─── Input souris ──────────────────────────────────────────────────────────────
func _unhandled_input(event: InputEvent) -> void:
	if GameManager.game_ended:
		if event.is_action_pressed("restart"):
			GameManager.restart()
		return

	if event is InputEventMouseMotion:
		rotate_y(-event.relative.x * MOUSE_SENS)
		head.rotate_x(-event.relative.y * MOUSE_SENS)
		head.rotation.x = clamp(head.rotation.x, -PI / 2.0, PI / 2.0)

	if event.is_action_pressed("weapon_1"):
		_switch_weapon(0)
	elif event.is_action_pressed("weapon_2"):
		_switch_weapon(1)
	elif event.is_action_pressed("ui_cancel"):
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

# ─── Physique ──────────────────────────────────────────────────────────────────
func _physics_process(delta: float) -> void:
	if GameManager.game_ended:
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

	# Tir
	if Input.is_action_pressed("shoot"):
		_try_shoot()
	if Input.is_action_just_pressed("reload"):
		_try_reload()

# ─── Armes ─────────────────────────────────────────────────────────────────────
func _switch_weapon(index: int) -> void:
	if index < 0 or index >= weapons.size():
		return
	if current_weapon_index >= 0 and current_weapon_index < weapons.size():
		weapons[current_weapon_index].visible = false
	current_weapon_index = index
	weapons[current_weapon_index].visible = true
	GameManager.weapon_changed.emit(weapons[current_weapon_index])

func _try_shoot() -> void:
	if current_weapon_index >= 0 and current_weapon_index < weapons.size():
		weapons[current_weapon_index].try_shoot()

func _try_reload() -> void:
	if current_weapon_index >= 0 and current_weapon_index < weapons.size():
		weapons[current_weapon_index].reload()

# ─── Dégâts ────────────────────────────────────────────────────────────────────
func take_damage(amount: float) -> void:
	health = max(0.0, health - amount)
	GameManager.health_changed.emit(health, max_health)
	if health <= 0.0:
		GameManager.player_died()
